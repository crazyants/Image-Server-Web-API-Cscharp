using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ImageManager
{
    public partial class WebManager : ImageManagerBase
    {
        private string directory;

        public static byte[] GetImageByteArray(FileUpload file)
        {
            byte[] buffer = new byte[file.PostedFile.ContentLength];
            file.PostedFile.InputStream.Read(buffer, 0, file.PostedFile.ContentLength);
            return buffer;
        }

        public static IImageInfo GetImageInfo(FileUpload file)
        {
            IImageInfo info = GetImageInfo(file.PostedFile.InputStream, file.PostedFile.ContentLength);
            info.FileName = file.FileName;
            info.ContentType = file.PostedFile.ContentType;
            return info;
        }

        public static IImageInfo GetImageInfoV2(HttpPostedFile file)
        {
            IImageInfo info = GetImageInfo(file.InputStream, file.ContentLength);
            info.FileName = file.FileName;
            info.ContentType = file.ContentType;
            return info;
        }

        public static IImageInfo GetImageInfoByByte(byte[] byteArr, string fileName, string type)
        {
            IImageInfo info = GetImageInfo(byteArr);
            info.FileName = fileName;
            info.ContentType = type;
            return info;
        }

        public static void DeleteImageSetFromFileSystem(string filename, string[] paths)
        {
            foreach (string p in paths)
            {
                DeleteImageFromFileSystem(filename, p);
            }
        }

        public static void DeleteImagesFromFileSystem(IImageInfo[] images)
        {
            foreach (IImageInfo img in images)
            {
                DeleteImageFromFileSystem(img);
            }
        }

        public static void DeleteImageFromFileSystem(IImageInfo image)
        {
            DeleteImageFromFileSystem(image.FileName, image.Path);
        }

        
    }
}
