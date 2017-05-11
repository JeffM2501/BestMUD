using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telnet
{
    public class ProtocolProcessor : IProtocol
    {
        protected readonly char[] NewlineArray = "\r".ToCharArray();

        public void AddConnection(Connection con)
        {
            con.ProtocolTag = null;
        }

        public void RemoveConnection(Connection con)
        {
            con.ProtocolTag = null;
        }

        public bool TranslateInbound(byte[] buffer, Connection con)
        {
            string overflow = string.Empty;

            if ((con.ProtocolTag as string) != null)
                overflow = (con.ProtocolTag as string);

            con.ProtocolTag = null;

            string data = Encoding.ASCII.GetString(buffer).Replace("\r",string.Empty);
            bool complete = data[data.Length-1] == '\n';

            string[] parts = (overflow+data).Split(NewlineArray);
 
            for (int i = 0; i < parts.Length; i++)
            {
                string message = parts[i];
                if (i == parts.Length - 1 && !complete)
                    overflow = message;
                else
                {
                    StringBuilder builder = new StringBuilder();
                    foreach(char c in message)
                    {
                        if (c == 127 && builder.Length > 0)
                            builder.Remove(builder.Length - 1, 1);
                        else if (c >= 32 && c < 127)
                            builder.Append(c);
                    }

                    con.PushInboundMessage(builder.ToString());
                }
            }

            return true;
        }

        public bool TranslateOutbound(string text, StringBuilder buffer, Connection con)
        {
            text = text.Replace("\r", string.Empty);

            // todo process colors
            StringBuilder builder = new StringBuilder();

            List<string> codes = new List<string>();
            bool inTag = false;
            foreach (var c in text)
            {
                if (c == '<')
                {
                    if (!inTag && builder.Length > 0)
                        codes.Add(builder.ToString());
                    builder.Clear();

                    inTag = true;
                    builder.Append(c);
                }
                else
                {
                    if (c == '>' && inTag)
                    {
                        builder.Append(c);
                        inTag = false;
                        if (builder.Length > 0)
                            codes.Add(builder.ToString());
                        builder.Clear();
                    }
                    else
                    {
                        if (c == '\n')
                            builder.Append('\r');
                        else if (c == '\r')
                        {
                            int q = 20;
                        }
                        builder.Append(c);
                    }
                }
            }
            if (builder.Length > 0)
                codes.Add(builder.ToString());

            string debugString = string.Empty;
            foreach (var c in codes)
            {
                if (c == string.Empty)
                    continue;
                if (c[0] == '<' && c[c.Length - 1] == '>')
                {
                    string code = TelnetColors.TranslateBMCode(c);
                    debugString += code;
                    buffer.Append(code);
                }
                else
                {
                    debugString += c;
                    buffer.Append(c);
                }
                    
            }

            buffer.Append("\r\n");
            return true;
        }
    }
}
