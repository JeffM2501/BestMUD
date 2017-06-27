using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Game.Characters;
using Core.Data.Game.World;

namespace Networking
{
    public class Connection : EventArgs
    {
        public static Connection None = new Connection(null);

        public TcpClient Socket = null;
        public NetworkStream DataStream = null;

        protected List<string> InboundMessages = new List<string>();
        protected List<string> OutboundMessages = new List<string>();

        public int LanguageID = 0;

        public object ProtocolTag = null;

        public bool SentHeader = true;

        public int UserID = -1;
        public List<string> AccessFlags = new List<string>();

        public PlayerCharacter ActiveCharacter = null;

        public ZoneInstance CurrentZoneProcessor = null;

        protected readonly bool DebugEcho = false;

        public object MessageProcessorTag { get; protected set; }

        protected Dictionary<string, object> MessageProcessorTags = new Dictionary<string, object>();

        public IMessageProcessor MessageProcessor { get; protected set; }

        public event EventHandler<Connection> Disconnected = null;


        public Connection(TcpClient soc)
        {
            MessageProcessorTag = null;
            MessageProcessor = null;
            Socket = soc;
            if (soc != null)
                DataStream = Socket.GetStream();
        }

        public void SetMessageProcessor(IMessageProcessor processor)
        {
            if (MessageProcessor != null)
                MessageProcessor.ProcessorDetatch(this);

            MessageProcessor = processor;
            if (MessageProcessor != null)
                MessageProcessor.ProcessorAttach(this);
        }

        public void SetMessageProcessorTag(string name, object tag)
        {
            if (MessageProcessorTags.ContainsKey(name))
                MessageProcessorTags[name] = tag;
            else
                MessageProcessorTags.Add(name, tag);

            MessageProcessorTag = tag;
        }

        public void SetMessageProcessorTag(string name)
        {
            if (MessageProcessorTags.ContainsKey(name))
                MessageProcessorTag = MessageProcessorTags[name];
            else
                MessageProcessorTag = null;
        }

        public object GetMesssageProcessorTag(string name)
        {
            if (MessageProcessorTags.ContainsKey(name))
                return MessageProcessorTags[name];
            else
                return null;
        }

        public T GetMesssageProcessorTag<T>() where T: class
        {
            return MessageProcessorTag as T;
        }

        public void PushInboundMessage(string msg)
        {
            if (msg == null || msg == string.Empty)
                return;

            lock (InboundMessages)
                InboundMessages.Add(msg);

            if (MessageProcessor != null)
                MessageProcessor.ProcessInbound(msg, this);

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

        public bool HasPendingInbound()
        {
            lock (InboundMessages)
                return InboundMessages.Count > 0;
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

        public virtual void OnDisconnect()
        {
            Disconnected?.Invoke(this, this);
        }

        public virtual void Disconnect()
        {
            Socket.Close();
            Socket.Dispose();
            Socket = null;
        }

        public bool HasAccessFlag(string flag)
        {
            lock (AccessFlags)
                return AccessFlags.Find(x => x.ToLower() == flag.ToLower()) != string.Empty;
        }
    }
}
