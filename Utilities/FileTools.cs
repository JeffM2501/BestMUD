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

            FileInfo file = new FileInfo(Path.Combine(root.FullName, path));
            if (!file.Exists)
            {
                LogCache.Log(LogCache.BasicLog, "Requested File not found: " + path);
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
    }
}
