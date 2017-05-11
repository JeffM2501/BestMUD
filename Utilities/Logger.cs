using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Utilities
{
    public class Logger
    {
        public static bool EchoToConsole = true;

        public static bool LogsInUTC = true;

        protected StreamWriter LogStream = null;
        protected bool UseTimestamps = false;
        protected bool UseDatestamps = false;
        protected IDecorator Decorator = null;


        public Logger(IDecorator dec, FileInfo file, string title, bool timeStamp, bool datestamp)
        {
            if (dec == null)
                return;

            Decorator = dec;

            LogStream = null;
            try
            {
                bool header = !file.Exists;

                LogStream = new StreamWriter(file.OpenWrite());

                if (!file.Exists)
                    LogStream.Write(Decorator.LogFileHeader(file.Name));

                LogStream.Write(Decorator.LogSessionOpen());
                WriteLogLine("Session opened.", true, true);
            }
            catch (Exception /*ex*/)
            {
                if (LogStream != null)
                    LogStream.Close();

                LogStream = null;
            }
        }

        ~Logger()
        {
            if (LogStream != null)
            {
                WriteLogLine("Session closed.", true, true);
                LogStream.Write(Decorator.LogSessionClose());
               
                LogStream.Close();
            }
            LogStream = null;
        }

        public void Log(string entry)
        {
            WriteLogLine(entry, UseTimestamps, UseDatestamps);
        }

        protected DateTime GetNow()
        {
            return LogsInUTC ? DateTime.UtcNow : DateTime.Now;
        }

        protected void WriteLogLine(string entry, bool timestamp, bool datestamp)
        {
            string data = string.Empty;

            DateTime now = GetNow();

            if (datestamp)
                data += "[" + now.ToShortDateString() + "] ";

            if (timestamp)
                data += "[" + now.ToShortTimeString() + "] ";

            data += entry;

            string line = Decorator.LogDecorate(data);
            LogStream.Write(line);

            LogStream.Flush();
            Console.WriteLine(line);
        }
    }
}
