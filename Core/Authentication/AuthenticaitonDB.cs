using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core.Authentication
{
    public static class AuthenticaitonDB
    {
        private static FileInfo AuthDB = null;

        internal class UserInfo
        {
            public string UserGUID = string.Empty;
            public string UserName = string.Empty;
            public string CryptoPass = string.Empty;
            public string AccessFlags = string.Empty;
        }

        public static void Setup(string path)
        {
            AuthDB = new FileInfo(path);
            if (!AuthDB.Exists)
            {
                // do SQLite create
            }
            
            // do SQLite load
        }

        public bool UserExists(string name)
        {
            if (name.Trim() == string.Empty)
                return true;

            return false;
          //  if (file)
        }

        public bool CreateUser(string name, string password, string accessFlags)
        {
            return false;
        }
    }
}
