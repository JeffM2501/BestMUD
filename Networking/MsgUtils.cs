using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Networking
{
    public static class MsgUtils
    {
        public static void SendUserFileMessage(Connection user, string path)
        {
            user.SendOutboundMessage(GetFileMessage(path));
        }

        public static void SendUserFileMessage(Connection user, string path, Dictionary<string, string> repacements)
        {
            user.SendOutboundMessage(GetFileMessage(path,repacements));
        }
        public static void SendUserFileMessage(Connection user, string path, string key, string value)
        {
            user.SendOutboundMessage(GetFileMessage(path,key,value));
        }

        public static void SendUserFileMessage(Connection user, string path, string key1, string value1, string key2, string value2)
        {
            user.SendOutboundMessage(GetFileMessage(path, key1, value1,key2,value2));
        }

        public static string GetFileMessage(string path)
        {
            string data = FileTools.GetFileContents(Paths.DataPath, path, true);
            if (data == null)
                data = path;
            return data;
        }

        public static string GetFileMessage(string path, Dictionary<string, string> repacements)
        {
            string data = FileTools.GetFileContents(Paths.DataPath, path, true);
            if (data == null)
                data = path;

            foreach (var r in repacements)
                data = data.Replace(r.Key, r.Value);

            return data;
        }
        public static string GetFileMessage(string path, string key, string value)
        {
            string data = FileTools.GetFileContents(Paths.DataPath, path, true);
            if (data == null)
                data = path;

            data = data.Replace(key, value);

            return data;
        }

        public static string GetFileMessage(string path, string key1, string value1, string key2, string value2)
        {
            string data = FileTools.GetFileContents(Paths.DataPath, path, true);
            if (data == null)
                data = path;

            data = data.Replace(key1, value1);
            data = data.Replace(key2, value2);

            return data;
        }
    }
}
