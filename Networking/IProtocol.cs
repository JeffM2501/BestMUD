using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public interface IProtocol
    {
        bool TranslateInbound(byte[] buffer, Connection con);
        bool TranslateOutbound(string text, StringBuilder buffer, Connection con);

        void AddConnection(Connection con);
        void RemoveConnection(Connection con);
    }
}
