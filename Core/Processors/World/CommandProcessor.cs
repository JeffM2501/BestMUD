using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Databases.GameData;
using Networking;
using Core.World;

namespace Core.Processors.World
{
    public class CommandProcessor : PooledProcessor
    {
        public event EventHandler<Connection> CharacterExit = null;

        public override void ProcessAccept(Connection con)
        {
            base.ProcessAccept(con);

            int zone = ZoneDB.Instance.GetRoomZone(con.ActiveCharacter.CurrentRoom);
            ZoneInstanceManager.SetUserToZone(con, zone, con.ActiveCharacter.CurrentRoom);
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            return base.ProcessUserMessage(user, msg);

            // process commands and push them to the zone processor. the zone handles the results.
            // this processor is just one to parse out commands and call the zone or character APIs.


        }
    }
}
