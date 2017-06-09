using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace Core.Config
{
    public class ServerConfig
    {
        public string DataDir = string.Empty;
        public string DatabaseDir = string.Empty;
        public string LogDir = string.Empty;

        public string ScriptsDir = string.Empty;
        public string PluginsDir = string.Empty;

        public int MaxConnectionManagerUsers = 200;
        public List<int> ListenPorts = new List<int>();

        public int ConnectionManagers = 1;
        public int LandingThreads = 1;
        public int CharacterSelectThreads = 2;
        public int CharacterCreateThreads = 1;
        public int CommandProcesseorThreads = 3;

        public int MaxZonePlayers = 100;

        public int MobThreads = 3;

        public static ServerConfig Load(string path)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
                return new ServerConfig();

            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(ServerConfig));
                var sr = file.OpenText();
                ServerConfig cfg = xml.Deserialize(sr) as ServerConfig;
                sr.Close();
                if (cfg == null)
                    cfg = new ServerConfig();

                return cfg;
            }
            catch (Exception /*ex*/)
            {
                return new ServerConfig();
            }
        }

        public void Save(string path)
        {
            try
            {
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                    file.Delete();

                XmlSerializer xml = new XmlSerializer(typeof(ServerConfig));

                var fs = file.OpenWrite();
                xml.Serialize(fs, this);
                fs.Close();
            }
            catch (Exception /*ex*/)
            {

            }
        }
    }
}
