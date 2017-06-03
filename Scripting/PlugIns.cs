using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

namespace Scripting
{
    public interface IBestMudPlugin
    {
        void ActivatePlugIn();
        void DeactivatePlugIn();
    }

    public static class PlugInLoader
    {
        public static List<IBestMudPlugin> Plugins = new List<IBestMudPlugin>();

        public static void LoadPluginsForAssembly(Assembly ass)
        {
            if (ass == null)
                return;

            foreach(var t in ass.GetTypes())
            {
                if (t.GetInterface(typeof(IBestMudPlugin).Name) != null)
                {
                    IBestMudPlugin p = Activator.CreateInstance(t) as IBestMudPlugin;
                    if (p == null)
                        continue;

                    lock (Plugins)
                    {
                        Plugins.Add(p);
                    }

                    lock(p)
                        p.ActivatePlugIn();
                }
            }
        }
        
        public static void LoadPluginsFromDir(string dir)
        {
            DirectoryInfo folder = new DirectoryInfo(dir);
            foreach(var f in folder.GetFiles("*.dll"))
            {
                try { LoadPluginsForAssembly(Assembly.LoadFile(f.FullName)); }
                catch (Exception /*ex*/) { }
            }
        }

        public static void UnloadPlugins()
        {
            lock (Plugins)
            {
                foreach(var p in Plugins)
                {
                    lock (p)
                        p.DeactivatePlugIn();
                }
                Plugins.Clear();
            }
        }
    }
}
