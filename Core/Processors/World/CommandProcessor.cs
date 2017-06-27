using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Databases.GameData;
using Networking;
using Core.World;
using Core.Databases.PlayerData;
using Core.Data.Common;

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
            CommandProcessors.Add("look", Look);
            CommandProcessors.Add("exit", Exit);

            CommandProcessors.Add("north", Move);
            CommandProcessors.Add("n", Move);
            CommandProcessors.Add("south", Move);
            CommandProcessors.Add("s", Move);
            CommandProcessors.Add("east", Move);
            CommandProcessors.Add("e", Move);
            CommandProcessors.Add("west", Move);
            CommandProcessors.Add("w", Move);
        }

        public override void ProcessorAttach(Connection user)
        {
            base.ProcessorAttach(user);

            int zone = ZoneDB.Instance.GetRoomZone(user.ActiveCharacter.CurrentRoom);
            ZoneInstanceManager.SetUserToZone(user, zone, user.ActiveCharacter.CurrentRoom);
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            base.ProcessUserMessage(user, msg);
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
            if (user.CurrentZoneProcessor != null)
                user.CurrentZoneProcessor.PlayerWho(user.UserID);
        }

        protected virtual void Say(Connection user, string cmd, string args)
        {
            if (user.CurrentZoneProcessor != null)
                user.CurrentZoneProcessor.PlayerSay(user.UserID, args);
        }
        protected virtual void Look(Connection user, string cmd, string args)
        {
            if (user.CurrentZoneProcessor == null)
                return;

            if (args == string.Empty)
                user.CurrentZoneProcessor.PlayerLookEnviron(user.UserID);
        }

        protected Directions ParseDirection(string word)
        {
            if (word == "down" || word == "d")
                return Directions.Down;

            if (word == "up" || word == "u")
                return Directions.Up;

            if (word == "north" || word == "n")
                return Directions.North;

            if (word == "northeast" || word == "ne")
                return Directions.NorthEast;

            if (word == "east" || word == "e")
                return Directions.East;

            if (word == "southeast" || word == "se")
                return Directions.SouthEast;

            if (word == "south" || word == "s")
                return Directions.South;

            if (word == "southwest" || word == "sw")
                return Directions.SouthWest;

            if (word == "west" || word == "w")
                return Directions.West;

            if (word == "northwest" || word == "nw")
                return Directions.South;

            if (word == "middle" || word == "m")
                return Directions.South;

            return Directions.Unknown;          
        }

        protected virtual void Move(Connection user, string cmd, string args)
        {
            if (user.CurrentZoneProcessor == null)
                return;

            v

            if (args == string.Empty)
                user.CurrentZoneProcessor.PlayerLookEnviron(user.UserID);
        }

        protected virtual void Exit(Connection user, string cmd, string args)
        {
            if (user.CurrentZoneProcessor != null)
                (user.CurrentZoneProcessor as RuntimeZoneInstance).RemoveUser(user);

            PlayerCharacterDB.Instance.CheckInCharacter(user.ActiveCharacter);
            user.ActiveCharacter = null;

           // user.SetMessageProcessor("CharacterSelect");
           // pop processor names here
            CharacterExit?.Invoke(user, user);
        }
    }
}
