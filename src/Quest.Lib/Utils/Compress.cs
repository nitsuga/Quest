using System.IO;
using System.IO.Compression;

namespace Quest.Lib.Utils
{
    public static class Compress
    {
        public static string CompressFile(string source, string destination)
        {
            using (var sourceStream = File.OpenRead(source))
            {
                using (var destStream = File.Create(destination))
                {
                    using (var compressionStream = new GZipStream(destStream, CompressionLevel.Optimal))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }
                }
            }
            return destination;
        }

        public static void Decompress(string fileToDecompress, string newFileName)
        {
            using (var originalFileStream = File.OpenRead(fileToDecompress))
            {
                using (var decompressedFileStream = File.Create(newFileName))
                {
                    using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }
    }
}