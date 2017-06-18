using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public interface IProtocol
    {
        bool TranslateInbound(byte[] buffer, Connection user);
        bool TranslateOutbound(string text, StringBuilder buffer, Connection user);

        void AddConnection(Connection user);
        void RemoveConnection(Connection user);
    }
}
