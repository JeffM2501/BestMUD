using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripting
{
    public static class API
    {
        private static Dictionary<string, object> GlobalDataCache = new Dictionary<string, object>();

        public static void SetGlobalDataObject(string name, object data)
        {
            lock(GlobalDataCache)
            {
                if (data == null)
                {
                    if (GlobalDataCache.ContainsKey(name))
                        GlobalDataCache.Remove(name);
                }
                else
                {
                    if (GlobalDataCache.ContainsKey(name))
                        GlobalDataCache[name] = data;
                    else
                        GlobalDataCache.Add(name, data);
                }
             }
        }

        public static void SetGlobalDataObject(string name)
        {
            SetGlobalDataObject(name, null);
        }

        public static object GetGlobalDataObject(string name)
        {
            lock (GlobalDataCache)
            {
                if (GlobalDataCache.ContainsKey(name))
                    return GlobalDataCache[name];
            }

            return null;
        }
    }
}
