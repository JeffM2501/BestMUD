using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Data.Game;
using Networking;
using Core.Data.Game.World;
using Core.Data.Game.Characters;
using System.Threading;
using Core.Data.Common;

namespace Core.World
{
    public class RuntimeZoneInstance : ZoneInstance
    {
        protected List<Connection> PendingPlayers = new List<Connection>();
        protected List<Tuple<int,PlayerCharacter>> PendingRemovals = new List<Tuple<int, PlayerCharacter>>();

        public Dictionary<int,Connection> ConnectedPlayers = new Dictionary<int,Connection>();

        protected DateTime LastConnectionTime = DateTime.MinValue;

        protected bool GotAtLeastOne = false;

        public bool Delitable()
        {
            lock (ConnectedPlayers)
            {
                int pendCount = 0;
                lock (PendingPlayers)
                    pendCount = PendingPlayers.Count;

                return GotAtLeastOne && pendCount == 0 && IsEmpty() && (DateTime.Now - LastConnectionTime).Seconds > 30;
            }
                
        }

        public override bool Full() { lock (ConnectedCharacters) return Worker == null ? true: (PendingPlayers.Count + ConnectedCharacters.Count) >= MaxPlayers; }
        public override bool IsEmpty() { lock (ConnectedCharacters) return ( PendingPlayers.Count + ConnectedCharacters.Count) == 0; }

        protected Thread Worker = null;

        protected Connection GetUser(int id)
        {
            lock (ConnectedPlayers)
            {
                if (ConnectedPlayers.ContainsKey(id))
                    return ConnectedPlayers[id];
            }
            return null;
        }

        public void Kill()
        {
            if (Worker != null)
                Worker.Abort();

            lock (PendingPlayers)
                PendingPlayers.Clear();

            lock (ConnectedCharacters)
                ConnectedCharacters.Clear();

            lock (ConnectedPlayers)
                ConnectedPlayers.Clear();
        }

        public RuntimeZoneInstance(Zone z) : base(z)
        {
            HostedZone = z;
        }

        public void Run()
        {
            if (Worker != null)
                Worker.Abort();

            Worker = new Thread(new ThreadStart(Execute));
            Worker.Start();
        }

        protected void Execute()
        {
            Startup();

            while (true)
            {
                Update();
                Thread.Sleep(25);
                if (Delitable())
                    break;
            }

            Worker = null;
        }

        public virtual void Update()
        {
            lock(PendingPlayers)
            {
                foreach (var p in PendingPlayers)
                    ProcessAdd(p);
            }
            PendingPlayers.Clear();

            lock (PendingRemovals)
            {
                foreach (var p in PendingRemovals)
                    ProcessRemove(p.Item1,p.Item2);
            }
            PendingRemovals.Clear();

            lock (ConnectedPlayers)
            {
                if (ConnectedPlayers.Count > 0)
                    LastConnectionTime = DateTime.Now;
            }

            // do some kind of logic on pending events?
        }

        protected virtual void ProcessAdd(Connection user)
        {
            lock (ConnectedPlayers)
            {
                GotAtLeastOne = true;
                ConnectedPlayers.Add(user.UserID, user);
            }

            lock (ConnectedCharacters)
                ConnectedCharacters.Add(user.ActiveCharacter);

            string msg = user.ActiveCharacter.Name + " has entered";

            SendToRoom(msg, user, user.ActiveCharacter.CurrentRoom);

            MsgUtils.SendUserFileMessage(user, "world/room_join_message.data", "<!ROOM_NAME>", HostedZone.Rooms[user.ActiveCharacter.CurrentRoom].Description);
        }

        protected virtual void ProcessRemove(int room, PlayerCharacter pc)
        {
            string msg = pc.Name + " has left";

            SendToRoom(msg, null, room);
        }

        protected virtual void Startup()
        {
            Scripting.Register.CallOnZoneStartup(this);
        }

        public void AddUser(Connection user)
        {
            lock (PendingPlayers)
                PendingPlayers.Add(user);
        }

        public void RemoveUser(Connection user)
        {
            lock (ConnectedCharacters)
                ConnectedCharacters.Remove(user.ActiveCharacter);
            lock (ConnectedPlayers)
                ConnectedPlayers.Remove(user.UserID);

            // delay any removals until the update thread.
            lock (PendingRemovals)
                PendingRemovals.Add(new Tuple<int,PlayerCharacter>(user.ActiveCharacter.CurrentRoom, user.ActiveCharacter));
        }

        protected void SendToRoom(string msg, Connection except, int room)
        {
            lock (ConnectedPlayers)
            {
                foreach(var c in ConnectedPlayers.Values)
                {
                    if (c == except || c.ActiveCharacter.CurrentRoom != room)
                        continue;

                    c.SendOutboundMessage(msg);
                }
            }
        }

        protected void SendToZone(string msg, Connection except)
        {
            lock (ConnectedPlayers)
            {
                foreach (var c in ConnectedPlayers.Values)
                {
                    if (c == except)
                        continue;

                    c.SendOutboundMessage(msg);
                }
            }
        }

        public override void PlayerSay(int userID, string text)
        {
            var con = GetUser(userID);
            if (con == null)
                return;

            SendToRoom(MsgUtils.GetFileMessage("world/room_join_message.data", "<!USER_NAME>", con.ActiveCharacter.Name, "<!MESSAGE>", text), con, con.ActiveCharacter.CurrentRoom);
        }

        public override void PlayerLookEnviron(int userID)
        {
            var user = GetUser(userID);
            if (user == null)
                return;

            MsgUtils.SendUserFileMessage(user, "world/room_look_message.data", "<!ROOM_NAME>", HostedZone.Rooms[user.ActiveCharacter.CurrentRoom].Description);

            user.SendOutboundMessage("Exits\r\n");
            foreach (var exit in HostedZone.Rooms[user.ActiveCharacter.CurrentRoom].Exits)
                user.SendOutboundMessage( exit.Direction.ToString() + " " + exit.Description + "\r\n");
        }

        public override void PlayerMove(int userID, Directions dir)
        {
            var user = GetUser(userID);
            if (user == null)
                return;

            Room.Exit exit = null;

            foreach (var e in HostedZone.Rooms[user.ActiveCharacter.CurrentRoom].Exits)
            {
                if (e.Direction == dir)
                {
                    exit = e;
                    break;
                }
            }

            if (exit == null)
                MsgUtils.SendUserFileMessage(user, "world/room_invalid_exit_dir.data", "<!DIR_NAME>", dir.ToString());
            else
            {

            }
        }

        public override void PlayerWho(int userID)
        {
            var con = GetUser(userID);
            if (con == null)
                return;

            // TODO, cache this list
            List<string> names = new List<string>();
            bool isAdmin = con.HasAccessFlag("admin");

            lock (ConnectedPlayers)
            {
                foreach ( var u in ConnectedPlayers.Values)
                {
                    string n = "\t" + u.ActiveCharacter.Name;
                    if (isAdmin)
                        n += " " + u.UserID.ToString();

                    n += "\r\n";

                    names.Add(n);
                }
            }

            MsgUtils.SendUserFileMessage(con, "world/who_header.data");
            foreach (var u in names)
                con.SendOutboundMessage(u);
        }
    }
}
