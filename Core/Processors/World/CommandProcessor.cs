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

        protected delegate void ProcessCommandCB(Connection user, string cmd, string args);

        protected Dictionary<string, ProcessCommandCB> CommandProcessors = new Dictionary<string, ProcessCommandCB>();

        public override void Setup()
        {
            base.Setup();

            CommandProcessors.Add("who", Who);
            CommandProcessors.Add("say", Say);
        }

        public override void ProcessAccept(Connection user)
        {
            base.ProcessAccept(user);

            int zone = ZoneDB.Instance.GetRoomZone(user.ActiveCharacter.CurrentRoom);
            ZoneInstanceManager.SetUserToZone(user, zone, user.ActiveCharacter.CurrentRoom);

        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            return base.ProcessUserMessage(user, msg);
            if (msg == string.Empty)
                return false;


            // process commands and push them to the zone processor. the zone handles the results.
            // this processor is just one to parse out commands and call the zone or character APIs.

            string[] args = msg.Split(" ".ToCharArray(), 2);
            if (args.Length == 0)
                return false;

            string cmd = args[0].ToLower();
            string p = string.Empty;
            if (args.Length == 2)
                p = args[1];


            if (CommandProcessors.ContainsKey(cmd))
                CommandProcessors[cmd](user,cmd, p);

            return false;
        }

        protected virtual void Who(Connection user, string cmd, string args)
        {

        }

        protected virtual void Say(Connection user, string cmd, string args)
        {
            if (user.CurrentZoneProcessor != null)
                user.CurrentZoneProcessor.PlayerSay(user, args);
        }
    }
}
