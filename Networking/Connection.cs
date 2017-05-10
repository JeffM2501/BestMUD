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

        public object MessageProcessorTag { get; protected set; }

        protected Dictionary<string, object> MessageProcessorTags = new Dictionary<string, object>();

        public IMessageProcessor MessageProcessor { get; protected set; }


        public Connection(TcpClient soc)
        {
            MessageProcessorTag = null;
            MessageProcessor = null;
            Socket = soc;
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
