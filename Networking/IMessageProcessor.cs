using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public interface IMessageProcessor
    {
        void ProcessAccept(Connection user);
        void ProcessDisconnect(Connection user);

        void ProcessInbound(string message, Connection user);

        void ProcessorAttach(Connection user);
        void ProcessorDetatch(Connection user);
    }
}
