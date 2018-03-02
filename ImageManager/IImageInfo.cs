using System;
using System.Collections.Generic;
using System.Text;

namespace ImageManager
{
    public interface IImageInfo
    {
        string Path { get; set; }
        string ContentType { get; set; }
        string FileName { get; set; }
        int FileSize { get; set; }
        byte[] PhotoStream { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        void Save();
        void Save(string newPath);
        void Save(string newPath, string newFilename);
        IImageInfo ResizeMe(int? maxHeight, int? maxWidth);
        IImageInfo ResizeMeV2(int? maxHeight, int? maxWidth);
    }
}
