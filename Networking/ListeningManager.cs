using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Networking
{
    public static class ListeningManager
    {

        private static Dictionary<int, TcpListener> ListenPorts = new Dictionary<int, TcpListener>();

        public static bool AddPort(int port)
        {
            if (ListenPorts.ContainsKey(port))
                return false;

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.BeginAcceptTcpClient(ListenAcceptTCPClient, listener);
            ListenPorts.Add(port, listener);

            return true;
        }

        public static void StopAll()
        {
            foreach(var l in ListenPorts)
            {
                l.Value.Stop();
            }
        }

        private static void ListenAcceptTCPClient(IAsyncResult ar)
        {
            try
            {
                TcpListener listener = ar.AsyncState as TcpListener;
                if (listener == null || listener.Server == null)
                    return;

                var client = listener.EndAcceptTcpClient(ar);
                listener.BeginAcceptTcpClient(ListenAcceptTCPClient, listener);
            }
            catch(Exception /*ex*/)
            {

            }
        }
    }
}
