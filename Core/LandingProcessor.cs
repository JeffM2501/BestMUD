using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class LandingProcessor : IMessageProcessor, IMessageProcessorFactory
    {
        public void ProcessAccept(Connection con)
        {
        }


        public void ProcessorAttach(Connection con)
        {

        }
        public void ProcessorDetatch(Connection con)
        {

        }

        public void ProcessDisconnect(Connection con)
        {

        }

        public void ProcessInbound(string message, Connection con)
        {

        }

        public IMessageProcessor CreateMessageProcessor(Connection con)
        {
            return new LandingProcessor();
        }
    }

}
