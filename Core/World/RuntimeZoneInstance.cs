using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Data.Game;
using Networking;
using Core.Data.Game.World;
using Core.Data.Game.Characters;
using System.Threading;

namespace Core.World
{
    public class RuntimeZoneInstance : ZoneInstance
    {
        protected List<Connection> PendingPlayers = new List<Connection>();
        protected List<Tuple<int,PlayerCharacter>> PendingRemovals = new List<Tuple<int, PlayerCharacter>>();

        public List<Connection> ConnectedPlayers = new List<Connection>();

        protected DateTime LastConnectionTime = DateTime.MinValue;

        public bool Delitable() { lock (ConnectedPlayers) return IsEmpty() && (DateTime.Now - LastConnectionTime).Seconds > 30; }

        public override bool Full() { lock (ConnectedCharacters) return Worker == null ? true: (PendingPlayers.Count + ConnectedCharacters.Count) >= MaxPlayers; }
        public override bool IsEmpty() { lock (ConnectedCharacters) return ( PendingPlayers.Count + ConnectedCharacters.Count) == 0; }

        protected Thread Worker = null;

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

        public void Execute()
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
                ConnectedPlayers.Add(user);

            lock (ConnectedCharacters)
                ConnectedCharacters.Add(user.ActiveCharacter);

            string msg = user.ActiveCharacter.Name + " has entered";

            SendToRoom(msg, user, user.ActiveCharacter.CurrentRoom);

            msg = "You have entered " + HostedZone.Rooms[user.ActiveCharacter.CurrentRoom].Description;
            user.SendOutboundMessage(msg);
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
                ConnectedPlayers.Remove(user);

            // delay any removals until the update thread.
            lock (PendingRemovals)
                PendingRemovals.Add(new Tuple<int,PlayerCharacter>(user.ActiveCharacter.CurrentRoom, user.ActiveCharacter));
        }

        protected void SendToRoom(string msg, Connection except, int room)
        {
            lock (ConnectedPlayers)
            {
                foreach(var c in ConnectedPlayers)
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
                foreach (var c in ConnectedPlayers)
                {
                    if (c == except)
                        continue;

                    c.SendOutboundMessage(msg);
                }
            }
        }

        public override void PlayerSay(Connection con, string text)
        {
            SendToRoom(con.ActiveCharacter.Name + " said \"" + text + "\"",con, con.ActiveCharacter.CurrentRoom);
        }
    }
}
