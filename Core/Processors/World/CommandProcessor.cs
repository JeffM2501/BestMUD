using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Databases.GameData;
using Networking;

namespace Core.Processors.World
{
    public class CommandProcessor : PooledProcessor
    {
        public event EventHandler<Connection> CharacterExit = null;

        public override void ProcessAccept(Connection con)
        {
            base.ProcessAccept(con);

            int zone = ZoneDB.Instance.GetRoomZone(con.ActiveCharacter.CurrentRoom);
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            return base.ProcessUserMessage(user, msg);
        }
    }
}
