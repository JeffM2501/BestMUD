using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class LogCache
    {
        public static readonly int BasicLog = 0;
        public static readonly int NetworkLog = 1;

        public static bool UseTimestamps = true;
        public static bool UseDatestamps = true;

        private static Dictionary<int, Logger> Logs = new Dictionary<int, Logger>();

        public static void Setup(int logID, string file, string title)
        {
            Setup(logID, new Decorators.TextDecorator(), file, title);
        }

        public static void Setup(int logID, IDecorator dec, string file, string title)
        {
            if (Logs.ContainsKey(logID))
                return;

            Logger l = new Logger(dec, new System.IO.FileInfo(file), title, UseTimestamps, UseDatestamps);

            Logs.Add(logID, l);
        }

        public static bool MutliplexLog(int existingLog, int newLog)
        {
            if (!Logs.ContainsKey(existingLog) || Logs.ContainsKey(newLog))
                return false;

            Logs.Add(newLog, Logs[existingLog]);

            return true;
        }

        public static void Clear()
        {
            Logs.Clear();
        }

        public static void Log(int logID, string entry)
        {
            if(!Logs.ContainsKey(logID))
                return;

            Logs[logID].Log(entry);
        }
    }
}
