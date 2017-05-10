using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public interface IMessageProcessor
    {
        void ProcessAccept(Connection con);
        void ProcessDisconnect(Connection con);

        void ProcessInbound(string message, Connection con);

        void ProcessorAttach(Connection con);
        void ProcessorDetatch(Connection con);
    }

    public interface IMessageProcessorFactory
    {
        IMessageProcessor CreateMessageProcessor(Connection con);
    }
}
