using Networking;
using Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class LandingProcessor : IMessageProcessor
    {
        protected DirectoryInfo DataPath = null;

        public void SetDataPath(string path)
        {
            DataPath = new DirectoryInfo(path);
        }

        public void ProcessAccept(Connection con)
        {
            con.SendOutboundMessage(FileTools.GetFileContents(DataPath, "login/logon.data",true));
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
    }

}
