using System;
using System.Collections.Generic;
using System.Text;

namespace ImageManager
{
    public class ImageInfo : IImageInfo
    {
        private string path;
        private string contentType;
        private string fileName;
        private int fileSize;
        private byte[] photoStream;
        private int width;
        private int height;

        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }

        public string ContentType
        {
            get { return this.contentType; }
            set { this.contentType = value; }
        }

        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        public int FileSize
        {
            get { return this.fileSize; }
            set { this.fileSize = value; }
        }

        public byte[] PhotoStream
        {
            get { return this.photoStream; }
            set { this.photoStream = value; }
        }

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public void Save(string newPath)
        {
            this.path = newPath;
            Save();
        }

        public void Save(string newPath, string newFileName)
        {
            this.fileName = newFileName;
            this.path = newPath;
            Save();
        }

        public void Save()
        {
            FileManager fMgr = new FileManager();
            fMgr.SaveImage(this);
        }

        public IImageInfo ResizeMe(int? maxHeight, int? maxWidth)
        {
            FileManager fMgr = new FileManager();
            return fMgr.GetResizedImage(this, maxHeight, maxWidth);
        }

        public IImageInfo ResizeMeV2(int? maxHeight, int? maxWidth)
        {
            FileManager fMgr = new FileManager();
            return fMgr.GetResizedImageV2(this, maxHeight, maxWidth);
        }
    }
}
