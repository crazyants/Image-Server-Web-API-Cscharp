using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageManager
{
    public partial class FileManager : ImageManagerBase
    {
        public void DeleteImageFromFileSystem(string fileName)
        {
            DeleteImageFromFileSystem(fileName, null);
        }

        public void DeleteImageFromFileSystem(string fileName, string folder)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            FileInfo fi = new FileInfo(GetPath(fileName, folder));
            if (fi.Exists)
                File.Delete(fi.FullName);
            else
                throw new FileNotFoundException("The image file was not found.");
        }

        
    }
}
