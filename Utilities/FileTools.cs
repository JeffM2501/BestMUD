using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class FileTools
    {
        private static Dictionary<string, string> StaticFileCache = new Dictionary<string, string>();

        public static string GetFileContents(DirectoryInfo root, string path, bool useCache)
        {
            if (root == null)
                return null;

            return GetFileContents(new FileInfo(Path.Combine(root.FullName, path)),useCache);
        }

        public static string GetFileContents(DirectoryInfo root, string dir, string name, bool useCache)
        {
            if (root == null)
                return null;

            return GetFileContents(new FileInfo(Path.Combine(root.FullName, dir, name)), useCache);
        }

        public static string GetFileContents(DirectoryInfo root, string dir, string dir2, string name, bool useCache)
        {
            if (root == null)
                return null;

            return GetFileContents(new FileInfo(Path.Combine(root.FullName, dir, dir2, name)), useCache);
        }

        public static string GetFileContents(FileInfo file, bool useCache)
        {
            if (!file.Exists)
            {
                LogCache.Log(LogCache.BasicLog, "Requested File not found: " + file.Name);
                return null;
            }

            if (useCache && StaticFileCache.ContainsKey(file.FullName))
                return StaticFileCache[file.FullName];

            var sr = file.OpenText();
            string t = sr.ReadToEnd();
            sr.Close();

            if (useCache)
                StaticFileCache.Add(file.FullName, t);

            return t;
        }

        public static void SetFileContents(FileInfo file, string data)
        {
            if (file.Exists)
                file.Delete();

            var fs = file.OpenWrite();
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Close();
            fs.Close();
        }

        public static void SetFileContents(DirectoryInfo root, string path, string data)
        {
            if (root == null)
                return ;

            SetFileContents(new FileInfo(Path.Combine(root.FullName, path)), data);
        }

        public static void SetFileContents(DirectoryInfo root, string dir, string name, string data)
        {
            if (root == null)
                return;

            SetFileContents(new FileInfo(Path.Combine(root.FullName, dir, name)), data);
        }

        public static void SetFileContents(DirectoryInfo root, string dir, string dir2, string name, string data)
        {
            if (root == null)
                return;

            SetFileContents(new FileInfo(Path.Combine(root.FullName, dir, dir2, name)), data);
        }


        public static void DeleteFile (FileInfo file)
        {
            if (file.Exists)
                file.Delete();
        }

        public static void DeleteFile(DirectoryInfo root, string path)
        {
            if (root == null)
                return;

            DeleteFile(new FileInfo(Path.Combine(root.FullName, path)));
        }

        public static void DeleteFile(DirectoryInfo root, string dir, string name)
        {
            if (root == null)
                return;

            DeleteFile(new FileInfo(Path.Combine(root.FullName, dir, name)));
        }

        public static void DeleteFile(DirectoryInfo root, string dir, string dir2, string name)
        {
            if (root == null)
                return;

            DeleteFile(new FileInfo(Path.Combine(root.FullName, dir, dir2, name)));
        }
    }
}
