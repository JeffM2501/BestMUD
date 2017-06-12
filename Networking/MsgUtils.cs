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
            string data = FileTools.GetFileContents(Paths.DataPath, path, true);
            if (data == null)
                data = path;
            user.SendOutboundMessage(data);
        }

        public static void SendUserFileMessage(Connection user, string path, Dictionary<string, string> repacements)
        {
            string data = FileTools.GetFileContents(Paths.DataPath, path, true);
            if (data == null)
                data = path;

            foreach (var r in repacements)
                data = data.Replace(r.Key, r.Value);

            user.SendOutboundMessage(data);
        }
        public static void SendUserFileMessage(Connection user, string path, string key, string value)
        {
            string data = FileTools.GetFileContents(Paths.DataPath, path, true);
            if (data == null)
                data = path;

            data = data.Replace(key, value);

            user.SendOutboundMessage(data);
        }
    }
}
