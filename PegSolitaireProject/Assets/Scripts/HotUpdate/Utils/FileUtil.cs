using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HotUpdate.Utils
{
    /// <summary>
    /// File-system helpers used by hot-update code.
    /// </summary>
    public static class FileUtil
    {
        public static string GetStreamSHA1(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using (var sha1 = SHA1.Create())
            {
                return ToLowerHex(sha1.ComputeHash(stream));
            }
        }

        public static string GetFileSHA1(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A file path is required.", nameof(filePath));

            using (var stream = File.OpenRead(filePath))
            {
                return GetStreamSHA1(stream);
            }
        }

        public static string GetBytesSHA1(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            using (var sha1 = SHA1.Create())
            {
                return ToLowerHex(sha1.ComputeHash(bytes));
            }
        }

        public static string GetStringSHA1(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return GetBytesSHA1(Encoding.UTF8.GetBytes(value));
        }

        public static void EnsureFileNotExist(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A file path is required.", nameof(filePath));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void EnsureDirectoryExist(string directoryPath, bool deleteExist = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentException("A directory path is required.", nameof(directoryPath));

            if (deleteExist && Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            Directory.CreateDirectory(directoryPath);
        }

        public static void EnsureDirectoryForCreatingFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A file path is required.", nameof(filePath));

            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                EnsureDirectoryExist(directoryPath);
            }

            EnsureFileNotExist(filePath);
        }

        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (string.IsNullOrWhiteSpace(sourceDir)) throw new ArgumentException("A source directory is required.", nameof(sourceDir));
            if (string.IsNullOrWhiteSpace(targetDir)) throw new ArgumentException("A target directory is required.", nameof(targetDir));
            if (!Directory.Exists(sourceDir)) throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDir}");

            Directory.CreateDirectory(targetDir);

            foreach (var sourceFile in Directory.GetFiles(sourceDir))
            {
                var targetFile = Path.Combine(targetDir, Path.GetFileName(sourceFile));
                File.Copy(sourceFile, targetFile, true);
            }

            foreach (var sourceSubdirectory in Directory.GetDirectories(sourceDir))
            {
                var targetSubdirectory = Path.Combine(targetDir, Path.GetFileName(sourceSubdirectory));
                CopyDirectory(sourceSubdirectory, targetSubdirectory);
            }
        }

        private static string ToLowerHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
