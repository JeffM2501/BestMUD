using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public class Connection
    {
        public TcpClient Socket = null;
        public NetworkStream DataStream = null;

        protected List<string> InboundMessages = new List<string>();
        protected List<string> OutboundMessages = new List<string>();

        public object ProtocolTag = null;

        protected readonly bool DebugEcho = true;


        public Connection(TcpClient soc)
        {
            Socket = soc;
            DataStream = Socket.GetStream();
        }

        public void PushInboundMessage(string msg)
        {
            if (msg == null || msg == string.Empty)
                return;

            lock (InboundMessages)
                InboundMessages.Add(msg);

            if (DebugEcho)
                SendOutboundMessage(msg);
        }

        public string PopInboundMessage()
        {
            lock (InboundMessages)
            {
                if (InboundMessages.Count == 0)
                    return string.Empty;

                string msg = InboundMessages[0];
                InboundMessages.RemoveAt(0);
                return msg;
            }
        }

        public void SendOutboundMessage(string msg)
        {
            if (msg == null || msg == string.Empty)
                return;

            lock (OutboundMessages)
                OutboundMessages.Add(msg);
        }

        public string PopOutboundMessage()
        {
            lock(OutboundMessages)
            {
                if (OutboundMessages.Count == 0)
                    return string.Empty;

                string msg = OutboundMessages[0];
                OutboundMessages.RemoveAt(0);
                return msg;
            }
        }

        public string[] PopNOutboundMessages(int n)
        {
            string[] a = new string[0];

            lock (OutboundMessages)
            {
                if (OutboundMessages.Count == 0)
                    return a;

                if (n > OutboundMessages.Count)
                    n = OutboundMessages.Count;

                if (n == OutboundMessages.Count)
                {
                    a = OutboundMessages.ToArray();
                    OutboundMessages.Clear();
                }
                else
                {
                    a = OutboundMessages.GetRange(0, n).ToArray();
                    OutboundMessages.RemoveRange(0, n);
                }
            }
            return a;
        }

        public bool HasOutboundData()
        {
            lock (OutboundMessages)
                return OutboundMessages.Count > 0;
        }
    }
}
