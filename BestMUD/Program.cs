using Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core;
using Utilities;
using Core.Authentication;

namespace BestMUD
{
    public class Program
    {
        public static LandingProcessor Lander = new LandingProcessor();

        static void Main(string[] args)
        {
            // data path
            string dataPath = FindDataDir();
            if (dataPath == string.Empty)
                return;

            Lander.SetDataPath(dataPath);
            string basicLogPath = Path.Combine(Path.GetDirectoryName(dataPath),"logs");
            if (!Directory.Exists(basicLogPath))
                Directory.CreateDirectory(basicLogPath);

            // logs
            basicLogPath = Path.Combine(basicLogPath, "log.txt");
            LogCache.Setup(LogCache.BasicLog, basicLogPath, "BestMudLog");
            LogCache.MutliplexLog(LogCache.BasicLog, LogCache.NetworkLog);

            //databases

            AuthenticaitonDB.Setup(Path.Combine(dataPath, "databases/authentication.db3"));

            // connetions
            ListeningManager.AddConnectionManager(new ConnectionManager(new Telnet.ProtocolProcessor(), GetMessageProcessor, 10));
            ListeningManager.AddPort(2525);

            while (true)
            {
                Lander.ProcessAllConnections();
                System.Threading.Thread.Sleep(10);
            }
            ListeningManager.StopAll();
        }

        public static IMessageProcessor GetMessageProcessor(Connection con)
        {
            return Lander;
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
