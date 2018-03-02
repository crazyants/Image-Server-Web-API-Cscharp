using ImageManager;
using Model;
using Model.Repository;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace ImageServer.Controllers
{
    public class PictureController : ApiController
    {
        private readonly string LogPath = HttpContext.Current.Server.MapPath("~/Log");
        private string physicalPath = null;

        // POST: api/picture/upload?username=USERNAME&token=TOKEN
        [HttpPost]
        public HttpResponseMessage Upload()
        {
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            string username = httpRequest.Params.Get("username");
            string token = httpRequest.Params.Get("token");

            if (!ValidPermission(username, token))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission Denied");
            }
            if (httpRequest.ContentLength > 5002000) // >= 5MB + req
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Maximum size is 5MB");
            }

            if (httpRequest.Files.Count > 0)
            {
                try
                {
                    var postedFile = httpRequest.Files[0];

                    IImageInfo img = WebManager.GetImageInfoV2(postedFile);
                    img.Path = physicalPath + "\\" + img.FileName;
                    if (!File.Exists(img.Path))
                    {
                        img.Save();
                        result = Request.CreateResponse(HttpStatusCode.Created, "Created Successfull");
                        LogAction(username, token, ActionType.Create, img.Path);
                    }
                    else
                    {
                        result = Request.CreateResponse(HttpStatusCode.OK, "Existed image");
                    }
                }
                catch (Exception e)
                {
                    result = Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
                }
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest, "No Content");
            }

            return result;
        }

        // POST: api/picture/Delete?name=NAME&username=USERNAME&token=TOKEN
        [HttpPost]
        public HttpResponseMessage Delete(string name)
        {
            var httpRequest = HttpContext.Current.Request;
            string username = httpRequest.Params.Get("username");
            string token = httpRequest.Params.Get("token");

            Folder currFolder = ValidPermissionV2(username, token);
            if (currFolder == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission Denied");
            }
            if (ValidPermission(username, token))
            {
                var fileDir = physicalPath + "\\" + name;
                //bool isDeleted = new FoldersRepository().RemoveRecord(currFolder);
                if (File.Exists(fileDir))
                {
                    File.Delete(fileDir);
                    LogAction(username, token, ActionType.Delete, fileDir);
                    return Request.CreateResponse(HttpStatusCode.OK, "Deleted Successfull");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Not exist");
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission denied");
        }

        // POST: api/picture/Update?fileName=FILENAME&username=USERNAME&token=TOKEN
        [HttpPost]
        public HttpResponseMessage Update(string fileName)
        {
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            string username = httpRequest.Params.Get("username");
            string token = httpRequest.Params.Get("token");

            if (!ValidPermission(username, token))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission Denied");
            }
            if (httpRequest.ContentLength > 5000000) // >= 5MB
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            bool isExist = false;
            string fileDir = "";
            fileDir = physicalPath + "\\" + fileName;
            if (File.Exists(fileDir))
            {
                isExist = true;
                File.Delete(fileDir);
                if (httpRequest.Files.Count > 0)
                {
                    try
                    {
                        var postedFile = httpRequest.Files[0];

                        var filePath = HttpContext.Current.Server.MapPath("~/" + postedFile.FileName);
                        //postedFile.SaveAs(filePath);
                        //docfiles.Add(filePath);

                        IImageInfo img = WebManager.GetImageInfoV2(postedFile);

                        img.Save(physicalPath + "/" + img.FileName);
                    }
                    catch (Exception e)
                    {
                        result = Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
                    }
                }
                else
                {
                    result = Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }

            if (!isExist)
            {
                result = Request.CreateResponse(HttpStatusCode.OK, "Not exist");
            }
            else
            {
                LogAction(username, token, ActionType.Update, fileDir);
                result = Request.CreateResponse(HttpStatusCode.Created, "Updated successfully");
            }

            return result;
        }

        // GET: api/picture/getimage?name=NAME&wd=INT?&hg=INT?&username=USERNAME&token=TOKEN
        [HttpGet]
        public HttpResponseMessage GetImage(string name, int? wd, int? hg)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            var httpRequest = HttpContext.Current.Request;
            string username = httpRequest.Params.Get("username");
            string token = httpRequest.Params.Get("token");

            if (!ValidPermission(username, token))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission Denied");
            }

            string directory = physicalPath + "/" + name;
            if (File.Exists(directory))
            {
                FileStream fs = new FileStream(directory, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] imageByte = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();
                string imageTypeFormName = name.Split('.')[1];
                Enum imageTypeEnum = (Enum)System.Enum.Parse(typeof(ImageType), imageTypeFormName);
                string imageType = ImageType.jpg.GetDisplayName();
                IImageInfo img = WebManager.GetImageInfoByByte(imageByte, name, imageType);

                IImageInfo newImg = img.ResizeMe(hg, wd);

                response.Content = new ByteArrayContent(newImg.PhotoStream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(imageType);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Exsited");
            }
            return response;
        }

        // GET: api/picture/getWHImage?name=NAME&wd=INT&hg=INT&username=USERNAME&token=TOKEN
        [HttpGet]
        public HttpResponseMessage GetWHImage(string name, int? wd, int? hg)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            var httpRequest = HttpContext.Current.Request;
            string username = httpRequest.Params.Get("username");
            string token = httpRequest.Params.Get("token");

            if (!ValidPermission(username, token))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission Denied");
            }

            string directory = physicalPath + "/" + name;
            if (File.Exists(directory))
            {
                FileStream fs = new FileStream(directory, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] imageByte = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();
                string imageTypeFormName = name.Split('.')[1];
                Enum imageTypeEnum = (Enum)System.Enum.Parse(typeof(ImageType), imageTypeFormName);
                string imageType = ImageType.jpg.GetDisplayName();
                IImageInfo img = WebManager.GetImageInfoByByte(imageByte, name, imageType);

                IImageInfo newImg = img.ResizeMeV2(hg, wd);

                response.Content = new ByteArrayContent(newImg.PhotoStream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(imageType);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Exsited");
            }

            return response;
        }

        // POST: api/picture/CreateFolder?name=NAME&username=USERNAME&token=TOKEN
        [HttpPost]
        public HttpResponseMessage CreateFolder(string name)
        {
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            string username = httpRequest.Params.Get("username");
            string token = httpRequest.Params.Get("token");// token of parentPath

            Folder currFolder = ValidPermissionV2(username, token);
            if (currFolder == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission Denied");
            }
            string path = physicalPath + "\\" + name;
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Existed Folder");
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
                string newToken = new FoldersRepository().InsertNew(currFolder.Brand, path, currFolder.Id);
                result = Request.CreateResponse(HttpStatusCode.OK, "Create Successfully. New folder's token: " + newToken);

                LogAction(username, token, ActionType.Create, path);
            }
            catch (Exception e)
            {
                result = Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }

            return result;
        }

        // POST: api/picture/RemoveFolder?name=NAME&username=USERNAME&token=TOKEN
        [HttpPost]
        public HttpResponseMessage RemoveFolder(string name)
        {
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            string username = httpRequest.Params.Get("username");
            string token = httpRequest.Params.Get("token");//token of delete path

            Folder currFolder = ValidPermissionV2(username, token);
            if (currFolder == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Permission Denied");
            }
            string path = physicalPath;
            try
            {
                // Determine whether the directory exists.
                if (!Directory.Exists(path))
                {
                    result = Request.CreateResponse(HttpStatusCode.OK, "Not Existed Folder");
                }

                bool isDeleted = new FoldersRepository().RemoveRecord(currFolder);
                if (isDeleted)
                {
                    // Delete the directory.
                    Directory.Delete(path);
                    result = Request.CreateResponse(HttpStatusCode.OK, "Remove Successfully");
                    LogAction(username, token, ActionType.Delete, path);
                }
                else
                {
                    //Can't delete in DB --> no delete directory
                    result = Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Accepted. But failed to remove folder!");
                }
            }
            catch (Exception e)
            {
                result = Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }

            return result;
        }

        /// <summary>
        /// Check username and token which provided are allowed and return boolean as result
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool ValidPermission(string username, string token)
        {
            if (username != null && token != null)
            {
                Folder folder = new FoldersRepository().GetByToken(token);
                if (folder == null)
                {
                    return false;
                }
                physicalPath = folder.Path;
                if (folder.Brand != null)
                {
                    if (new CredentialsRepository().CheckUserBrand(username, folder.Brand))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check username and token which provided are allowed and return Folder as result for later on
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Folder ValidPermissionV2(string username, string token)
        {
            if (username != null && token != null)
            {
                Folder folder = new FoldersRepository().GetByToken(token);
                physicalPath = folder.Path;
                return folder;
            }
            return null;
        }

        private void LogAction(string username, string token, ActionType actionType, string path)
        {
            string content = String.Format(("| Date: {0,23} | User: {1,20} | Token: {2,20} | Action: {3,7} | Path: {4,-250} ||"),
                                            DateTime.Now.ToString(), username, token, actionType.GetDisplayName(), path);
            Logging logging = new Logging(LogPath, content);
            logging.LogAction();
        }
    }

    public class Logging
    {
        public string LogPath { get; set; } // ~/server/Log
        private string DestinationPath { get; set; } // ~/server/Log/28_02_2018
        public string Content { get; set; }

        public Logging(string logPath, string content)
        {
            LogPath = logPath;
            Content = content;
            DestinationPath = logPath + "/" + MakeFileName();
        }

        public void LogAction()
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(DestinationPath, true))
            {
                file.WriteLine(Content);
            }
        }

        //By week of month
        private string MakeFileName()
        {
            DateTime currDay = DateTime.Now;
            string fileName = currDay.GetWeekOfMonth() + "_" + currDay.Month + "_" + currDay.Year + ".txt";
            return fileName;
        }
    }

    public enum ImageType
    {
        [Display(Name = "image/jpeg")]
        jpg = 1,
        [Display(Name = "image/png")]
        png = 2,
        [Display(Name = "image/gif")]
        gif = 3
    }

    public enum ActionType
    {
        [Display(Name = "Create")]
        Create,
        [Display(Name = "Read")]
        Read,
        [Display(Name = "Update")]
        Update,
        [Display(Name = "Delete")]
        Delete,
    }

    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the <see cref="DisplayAttribute.Name" /> property on the <see cref="DisplayAttribute" />
        /// of the current enum value, or the enum's member name if the <see cref="DisplayAttribute" /> is not present.
        /// </summary>
        /// <param name="val">This enum member to get the name for.</param>
        /// <returns>The <see cref="DisplayAttribute.Name" /> property on the <see cref="DisplayAttribute" /> attribute, if present.</returns>
        public static string GetDisplayName(this Enum val)
        {
            return val.GetType()
                      .GetMember(val.ToString())
                      .FirstOrDefault()
                      ?.GetCustomAttribute<DisplayAttribute>(false)
                      ?.Name
                      ?? val.ToString();
        }
    }

    public static class DateTimeExtensions
    {
        static GregorianCalendar _gc = new GregorianCalendar();
        /// <summary>
        /// Return the number of the week of month which contains current date in integer as result
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int GetWeekOfMonth(this DateTime time)
        {
            DateTime first = new DateTime(time.Year, time.Month, 1);
            return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
        }

        /// <summary>
        /// Return the number of the week of year which contains current date in integer as result
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        static int GetWeekOfYear(this DateTime time)
        {
            return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }
    }
}
