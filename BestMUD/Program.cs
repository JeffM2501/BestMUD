using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMUD
{
    class Program
    {
        static void Main(string[] args)
        {
            ListeningManager.AddConnectionManager(new ConnectionManager(new Telnet.ProtocolProcessor(), 10));
            ListeningManager.AddPort(2525);

            while (true)
                System.Threading.Thread.Sleep(10);

            ListeningManager.StopAll();
        }
    }
}
