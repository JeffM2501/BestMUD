using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripting
{
    public static class Register
    {
        internal static Dictionary<string, string> RegisteredFunctionNames = new Dictionary<string, string>();

        internal static void Clear()
        {
            RegisteredFunctionNames.Clear();
        }

        public static void HandlerFunction(string handler, string name)
        {
            if (RegisteredFunctionNames.ContainsKey(handler.ToLower()))
                RegisteredFunctionNames[handler.ToLower()] = name;
            else
                RegisteredFunctionNames.Add(handler.ToLower(), name);
        }
    }
}
