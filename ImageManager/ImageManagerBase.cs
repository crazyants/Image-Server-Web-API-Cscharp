using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace ImageManager
{
    public abstract class ImageManagerBase
    {
        public static System.Drawing.Image GetImageFromStream(byte[] stream)
        {
            return System.Drawing.Image.FromStream(new MemoryStream(stream));
        }

        public static byte[] GetImageByteArray(Stream stream, int contentLength)
        {
            byte[] buffer = new byte[contentLength];
            stream.Read(buffer, 0, contentLength);
            return buffer;
        }

        public static IImageInfo GetImageInfo(byte[] stream)
        {
            IImageInfo info = new ImageInfo();
            info.FileSize = stream.Length;
            info.PhotoStream = stream;
            Image img = GetImageFromStream(stream);
            info.Width = img.Size.Width;
            info.Height = img.Size.Height;
            return info;
        }

        public static IImageInfo GetImageInfo(Stream stream, int contentLength)
        {
            IImageInfo info = new ImageInfo();
            byte[] imgBuffer = GetImageByteArray(stream, contentLength);
            info.FileSize = imgBuffer.Length;
            info.PhotoStream = imgBuffer;
            Image img = GetImageFromStream(imgBuffer);
            info.Width = img.Size.Width;
            info.Height = img.Size.Height;
            return info;
        }

        public IImageInfo GetResizedImage(IImageInfo image, int? maxHeight, int? maxWidth)
        {
            if ((!maxHeight.HasValue && !maxWidth.HasValue))
                throw new ArgumentOutOfRangeException("maxHeight", "You must provide a non-zero maxHeight or maxWidth");

            byte[] resizedStream = GetResizedImageStream(image.PhotoStream, maxHeight, maxWidth, image.ContentType);
            Image newImg = GetImageFromStream(resizedStream);
            IImageInfo info = new ImageInfo();
            info.ContentType = image.ContentType;
            info.FileName = image.FileName;
            info.FileSize = resizedStream.Length;
            info.PhotoStream = resizedStream;
            info.Width = newImg.Size.Width;
            info.Height = newImg.Size.Height;
            return info;
        }

        public IImageInfo GetResizedImageV2(IImageInfo image, int? maxHeight, int? maxWidth)
        {
            if ((!maxHeight.HasValue && !maxWidth.HasValue))
                throw new ArgumentOutOfRangeException("maxHeight", "You must provide a non-zero maxHeight or maxWidth");

            byte[] resizedStream = GetResizedImageStreamV2(image.PhotoStream, maxHeight, maxWidth, image.ContentType);
            Image newImg = GetImageFromStream(resizedStream);
            IImageInfo info = new ImageInfo();
            info.ContentType = image.ContentType;
            info.FileName = image.FileName;
            info.FileSize = resizedStream.Length;
            info.PhotoStream = resizedStream;
            info.Width = newImg.Size.Width;
            info.Height = newImg.Size.Height;
            return info;
        }

        public static string GetImageSize(IImageInfo image)
        {
            string retVal = "Unknown";
            FileInfo fi = new FileInfo(GetPath(image.FileName, image.Path));
            if (fi.Exists)
            {
                retVal = string.Format("{0} Kb", ((int)(fi.Length / 1000)).ToString());
            }
            return retVal;
        }

        public byte[] GetResizedImageStream(byte[] stream, int? maxHeight, int? maxWidth, string contentType)
        {
            byte[] buffer = stream;
            Image img = GetImageFromStream(stream);
            
            int width = img.Size.Width;
            int height = img.Size.Height;
            int mWidth = (maxWidth.HasValue) ? maxWidth.Value : 0;
            int mHeight = (maxHeight.HasValue) ? maxHeight.Value : 0;

            bool doWidthResize = (mWidth > 0 && width > mWidth && width > mHeight);
            bool doHeightResize = (mHeight > 0 && height > mHeight && height > mWidth);

            //only resize if the image is bigger than the max
            if (doWidthResize || doHeightResize)
            {
                int iStart;
                Decimal divider;
                if (doWidthResize)
                {
                    iStart = width;
                    divider = Math.Abs((Decimal)iStart / (Decimal)mWidth);
                    width = mWidth;
                    height = (int)Math.Round((Decimal)(height / divider));
                }
                else
                {
                    iStart = height;
                    divider = Math.Abs((Decimal)iStart / (Decimal)mHeight);
                    height = mHeight;
                    width = (int)Math.Round((Decimal)(width / divider));
                }

                Image newImg = img.GetThumbnailImage(width, height, null, new System.IntPtr());
                using (MemoryStream ms = new MemoryStream())
                {
                    if (contentType.IndexOf("jpeg") > -1)
                        newImg.Save(ms, ImageFormat.Jpeg);
                    else if (contentType.IndexOf("png") > -1)
                        newImg.Save(ms, ImageFormat.Png);
                    else
                        newImg.Save(ms, ImageFormat.Gif);

                    buffer = ms.ToArray();
                }
            }
            
            return buffer;
        }

        public byte[] GetResizedImageStreamV2(byte[] stream, int? maxHeight, int? maxWidth, string contentType)
        {
            byte[] buffer = stream;
            Image img = GetImageFromStream(stream);

            int width = img.Size.Width;
            int height = img.Size.Height;
            int mWidth = (maxWidth.HasValue) ? maxWidth.Value : 0;
            int mHeight = (maxHeight.HasValue) ? maxHeight.Value : 0;

            bool doWidthResize = (mWidth > 0 && width > mWidth && width > mHeight);
            bool doHeightResize = (mHeight > 0 && height > mHeight && height > mWidth);

            //only resize if the image is bigger than the max
            if (doWidthResize || doHeightResize)
            {
                int iStart;
                Decimal divider;
                if (doWidthResize)
                {
                    iStart = width;
                    divider = Math.Abs((Decimal)iStart / (Decimal)mWidth);
                    width = mWidth;
                    //height = (int)Math.Round((Decimal)(height / divider));
                }
                if (doHeightResize)
                {
                    iStart = height;
                    divider = Math.Abs((Decimal)iStart / (Decimal)mHeight);
                    height = mHeight;
                    //width = (int)Math.Round((Decimal)(width / divider));
                }

                Image newImg = img.GetThumbnailImage(width, height, null, new System.IntPtr());
                using (MemoryStream ms = new MemoryStream())
                {
                    if (contentType.IndexOf("jpeg") > -1)
                        newImg.Save(ms, ImageFormat.Jpeg);
                    else if (contentType.IndexOf("png") > -1)
                        newImg.Save(ms, ImageFormat.Png);
                    else
                        newImg.Save(ms, ImageFormat.Gif);

                    buffer = ms.ToArray();
                }
            }

            return buffer;
        }

        public void SaveImage(IImageInfo image)
        {
            //save file to file system
            //string path = GetPath(image.FileName, image.Path);
            string path = image.Path;
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
            fs.Write(image.PhotoStream, 0, image.FileSize);

            SaveImageFile(fs, path);
            fs.Dispose();
            fs.Close();
        }

        internal static string GetPath(string fileName, string path)
        {
            string directory = (path != null) ? string.Format("{0}/", path) : "";
            return HttpContext.Current.Server.MapPath(string.Format("{0}{1}", directory, fileName));
        }

        internal void SaveImageFile(System.IO.FileStream fs, string path)
        {
            Bitmap bmp = new System.Drawing.Bitmap(fs);
            if (Path.GetExtension(path).Equals(".gif"))
                bmp.Save(fs, ImageFormat.Gif);
            else if (Path.GetExtension(path).Equals(".png"))
                bmp.Save(fs, ImageFormat.Png);
            else
                bmp.Save(fs, ImageFormat.Jpeg);

            bmp.Dispose();
        }

        public static void DeleteImageFromFileSystem(string fileName, string path)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            FileInfo fi = new FileInfo(GetPath(fileName, path));
            if (fi.Exists)
                File.Delete(fi.FullName);
            else
                throw new FileNotFoundException("The image file was not found.");
        }
    }
}
