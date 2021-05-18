using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileStorage
{
    public static class FileStorageService
    {
        public const int ok = 1;
        public const int bad = 2;
        public const int notFound = 4;

        private const string _path = @"C:\Storage";
         
        public static string GetFullPath(string filePath)
        {
            if (filePath == null)
            {
                return _path;
            }
            else
            {
                return Path.Combine(_path, filePath);
            }
        }

        public static List<string> GetDirectoryInfo(string path)
        {
            List<string> information = new List<string>();
            string[] directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                information.Add($"Folder: {new DirectoryInfo(directory).Name}");
            }

            string[] files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                information.Add($"File: {new FileInfo(file).Name}");
            }

            return information;
        }

        public static string GetMimeType(string path)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(path).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();

            return mimeType;
        }

        public static void AddFileInfo(string path)
        {

        }

        public static List<string> ProcessDirectory(string directory)
        {
            List<string> result = new List<string>();

            string[] directoryEntities = Directory.GetDirectories(directory);
            foreach(string directoryName in directoryEntities)
            {
                result.Add($"Directory: {new DirectoryInfo(directoryName).Name}");
            }

            string[] fileEntries = Directory.GetFiles(directory);
            foreach (string fileName in fileEntries)
            {
                result.Add($"File: {Path.GetFileName(fileName)}");
            }

            return result;
        }       

        public static Dictionary<string, string> FileHeader(string filePath)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            FileInfo fileInfo = new FileInfo(filePath);

            header.Add("Name: ", fileInfo.Name);
            header.Add("Size: ", fileInfo.Length.ToString());
            header.Add("Extension: ", fileInfo.Extension);
            header.Add("Creation time: ", fileInfo.CreationTime.ToString());
            header.Add("Last write time:", fileInfo.LastWriteTime.ToString());
            header.Add("Last access time: ", fileInfo.LastAccessTime.ToString());

            return header;
        }

        public static int CopyFile(string srcPath, string destPath)
        {
            srcPath = Path.Combine(_path, srcPath);

            if (!File.Exists(srcPath))
            {
                return notFound;
            }

            if (!Directory.Exists(Path.GetDirectoryName(destPath)))
            {
                return notFound;
            }

            if (!Path.GetFileName(srcPath).Contains(".") || !Path.GetFileName(destPath).Contains("."))
            {
                return bad;
            }

            if (srcPath != destPath)
            {
                using FileStream sourceFile = new FileStream(srcPath, FileMode.Open);
                using FileStream destFile = new FileStream(destPath, FileMode.OpenOrCreate);
                sourceFile.CopyTo(destFile);
            }
            
            return ok;
        }

    }
}
