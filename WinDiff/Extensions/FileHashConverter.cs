using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using System.IO.Hashing;

namespace WinDiff.Extensions
{
    public class FileHashConverter
    {
        readonly IImageHash algorithm;

        public FileHashConverter()
        {
            this.algorithm = new AverageHash();
            //this.algorithm = new DifferenceHash();
            //this.algorithm = new PerceptualHash();
        }

        public string? ConvertOrDefault(string? path)
        {
            try
            {
                return Convert(path);
            }
            catch
            {
                return null;
            }
        }
        public string? Convert(string? path)
        {
            if (path == null) return null;

            switch (Path.GetExtension(path).ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".heic":
                case ".bmp":
                case ".cr2":
                case ".nef":
                    return ImageConvert(path);
                default:
                    return FileConvert(path);
            }
        }
        public string? ImageConvert(string? path)
        {
            if (path == null) return null;

            return algorithm.Hash(File.OpenRead(path)).ToString("X16");
        }
        public string? FileConvert(string? path)
        {
            if (path == null) return null;

            return XxHash32.HashToUInt32(File.ReadAllBytes(path)).ToString("X8");
        }
    }
}
