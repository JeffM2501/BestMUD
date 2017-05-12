using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Utilities
{
    public static class CryptoTools
    {
        public static string LocalCryptString(string data)
        {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(data), new byte[0], DataProtectionScope.LocalMachine));
        }

        public static string LocalDecryptString(string data)
        {
            char[] d = data.ToCharArray();
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64CharArray(d,0,d.Length),new byte[0],DataProtectionScope.LocalMachine));
        }
    }
}
