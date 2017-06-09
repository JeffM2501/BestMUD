using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Core.Data;
using Core.Processors;
using Core.Databases.Authentication;
using Core.Databases.PlayerData;
using Core.Databases.GameData;
using Core.Processors.Characters;
using Core.Config;

using Networking;
using Utilities;
using Scripting;
using Core.Processors.World;

namespace BestMUD
{
    public class Program
    {
        private static bool Exit = false;

        public static ServerConfig Config = new ServerConfig();

        private static object ExitLocker = new object();
        public static void Quit()
        {
            lock (ExitLocker)
                Exit = true;
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string cfg = args[0];
                if (File.Exists(cfg))
                    Config = ServerConfig.Load(cfg);
                else
                    Config.Save(cfg);
            }

            // data paths
            string dataPath = FindDataDir();
            if (Config.DataDir != string.Empty)
                dataPath = Config.DataDir;

            if (dataPath == string.Empty || !Directory.Exists(dataPath))
            {
                Console.WriteLine("Invalid data path " + dataPath);
                return;
            }

            Core.Data.Paths.DataPath = new DirectoryInfo(dataPath);

            string dbPath = Path.Combine(dataPath, "databases");
            if (Config.DatabaseDir != string.Empty)
                dbPath = Config.DatabaseDir;

            string basicLogPath = Path.Combine(Path.GetDirectoryName(dataPath), "logs");
            if (Config.LogDir != string.Empty)
                basicLogPath = Config.LogDir;

            if (!Directory.Exists(basicLogPath))
                Directory.CreateDirectory(basicLogPath);

            // logs
            basicLogPath = Path.Combine(basicLogPath, "log.txt");
            LogCache.Setup(LogCache.BasicLog, basicLogPath, "BestMudLog");
            LogCache.MutliplexLog(LogCache.BasicLog, LogCache.NetworkLog);

            //databases

            AuthenticaitonDB.Instance.Setup(Path.Combine(dbPath, "authentication.db3"));
            PlayerCharacterDB.Instance.Setup(Path.Combine(dbPath, "player_characters.db3"));
            ClassDB.Instance.Setup(Path.Combine(dbPath, "default_race_class.db3"));
            RaceDB.Instance.Setup(Path.Combine(dbPath, "default_race_class.db3"));
            ZoneDB.Instance.Setup(Path.Combine(dbPath, "zones.db3"));

            // default rules
            Core.DefaultRules.DefaultRuleset.Init();

            // processor pools
            ProcessorPool.SetupProcessorPool("Landing", typeof(LandingProcessor), Config.LandingThreads, false, true, (s, e) => (s as LandingProcessor).AuthenticationComplete += (s1, c) => c.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterSelect", c)));
            ProcessorPool.SetupProcessorPool("CharacterCreate", typeof(CharacterCreateProcessor), Config.CharacterCreateThreads, true, true, (s, e) => (s as CharacterCreateProcessor).CharacterCreateComplete += (s1, c) => c.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterSelect", c)));
            ProcessorPool.SetupProcessorPool("CharacterSelect", typeof(CharacterSelectProcessor), Config.CharacterSelectThreads, true, true, (s, e) => (s as CharacterSelectProcessor).CharacterSelectionComplete += (s1, c) => c.SetMessageProcessor(ProcessorPool.GetProcessor("CommandProcessor", c)));
            ProcessorPool.SetupProcessorPool("CommandProcessor", typeof(CommandProcessor), Config.CommandProcesseorThreads, true, true, (s, e) => (s as CommandProcessor).CharacterExit += (s1, c) => c.SetMessageProcessor(ProcessorPool.GetProcessor("ExitProcessor", c)));

            // plugins
            PlugInLoader.LoadPluginsForAssembly(System.Reflection.Assembly.GetExecutingAssembly()); // this program
            PlugInLoader.LoadPluginsForAssembly(typeof(ServerConfig).Assembly); // the core DLL

            if (Config.PluginsDir != string.Empty || Directory.Exists(Config.PluginsDir))
                PlugInLoader.LoadPluginsFromDir(Config.PluginsDir);

            // scripts


            // listeners

            for (int i =0; i < Config.ConnectionManagers; i++)
                ListeningManager.AddConnectionManager(new ConnectionManager(new Telnet.ProtocolProcessor(), GetMessageProcessor, Config.MaxConnectionManagerUsers)); // todo, read config and get number of connection threads and connection counts

            // connections
            if (Config.ListenPorts.Count == 0)
               ListeningManager.AddPort(2525);
            else
            {
                foreach (int p in Config.ListenPorts)
                {
                    try{ListeningManager.AddPort(p);}catch (Exception ex) { Console.WriteLine("Unable to listen on port " + p.ToString() + " " + ex.ToString()); }
                }
            }

            while (true)
            {
                ProcessorPool.UpdateProcessorsPools();
                System.Threading.Thread.Sleep(10);

                lock (ExitLocker)
                {
                    if (Exit)
                        break;
                }
            }
            ListeningManager.StopAll();
        }

        public static IMessageProcessor GetMessageProcessor(Connection con)
        {
            return ProcessorPool.GetProcessor("Landing",con);
        }

        public static string FindDataDir()
        {
            string appLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            DirectoryInfo checkDir = new DirectoryInfo(Path.GetDirectoryName(appLoc));

            while (checkDir != null)
            {
                string t = Path.Combine(checkDir.FullName,"data");
                if (Directory.Exists(t))
                    return t;

                checkDir = checkDir.Parent;

            }
            Console.WriteLine("Unable to locate data dir!");
            return string.Empty;
        }
    }
}
