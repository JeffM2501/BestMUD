using Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core.Data;
using Core.Processors;
using Utilities;
using Core.Databases.Authentication;
using Core.Databases.PlayerData;
using Core.Databases.GameData;
using Core.Processors.Characters;

namespace BestMUD
{
    public class Program
    {
        private static bool Exit = false;

        private static object ExitLocker = new object();
        public static void Quit()
        {
            lock (ExitLocker)
                Exit = true;
        }

        static void Main(string[] args)
        {
            // data paths
            string dataPath = FindDataDir();
            if (dataPath == string.Empty)
                return;

            Core.Data.Paths.DataPath = new DirectoryInfo(dataPath);

            string basicLogPath = Path.Combine(Path.GetDirectoryName(dataPath), "logs");
            if (!Directory.Exists(basicLogPath))
                Directory.CreateDirectory(basicLogPath);

            // logs
            basicLogPath = Path.Combine(basicLogPath, "log.txt");
            LogCache.Setup(LogCache.BasicLog, basicLogPath, "BestMudLog");
            LogCache.MutliplexLog(LogCache.BasicLog, LogCache.NetworkLog);

            //databases

            AuthenticaitonDB.Instance.Setup(Path.Combine(dataPath, "databases/authentication.db3"));
            PlayerCharacterDB.Instance.Setup(Path.Combine(dataPath, "databases/player_characters.db3"));
            ClassDB.Instance.Setup(Path.Combine(dataPath, "databases/default_race_class.db3"));
            RaceDB.Instance.Setup(Path.Combine(dataPath, "databases/default_race_class.db3"));

            // default rules
            Core.DefaultRules.DefaultRuleset.Init();

            // processor pools
           // ProcessorPool.SetupProcessorPool("Landing", typeof(LandingProcessor), 10, false, true, (s, e)=>(s as LandingProcessor).AuthenticationComplete += LandingProcessor_AuthenticationComplete);

            ProcessorPool.SetupProcessorPool("Landing", typeof(LandingProcessor), 10, false, true, (s, e) => (s as LandingProcessor).AuthenticationComplete += (s1, c) => c.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterSelect", c)));

            ProcessorPool.SetupProcessorPool("CharacterCreate", typeof(CharacterCreateProcessor), 2, true, true, (s, e) => (s as CharacterCreateProcessor).CharacterCreateComplete += (s1, c) => c.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterSelect", c)));
            ProcessorPool.SetupProcessorPool("CharacterSelect", typeof(CharacterSelectProcessor), 2, true, true, (s, e) => (s as CharacterSelectProcessor).CharacterSelectionComplete += (s1, c) => c.SetMessageProcessor(ProcessorPool.GetProcessor("ZoneProcessor", c)));

            // connections
            ListeningManager.AddConnectionManager(new ConnectionManager(new Telnet.ProtocolProcessor(), GetMessageProcessor, 200)); // todo, read config and get number of connection threads and connection counts
            ListeningManager.AddPort(2525);

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
