using Core.Data.Game.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Game.World
{
    public class ZoneInstance : EventArgs
    {
        public Zone HostedZone = null;
        public List<PlayerCharacter> ConnectedCharacters= new List<PlayerCharacter>();

        public int MaxPlayers = 0;

        protected bool IsPrimary = false;

        public bool Primary { get { lock (ConnectedCharacters) return IsPrimary; } set { lock (ConnectedCharacters) IsPrimary = value; } }

        public virtual bool Full() { lock (ConnectedCharacters) return ConnectedCharacters.Count >= MaxPlayers; }
        public virtual bool IsEmpty() { lock (ConnectedCharacters) return ConnectedCharacters.Count == 0; }

        public ZoneInstance (Zone z)
        {
            HostedZone = z;z
        }

        // virtualized methods for doing actions
        public abstract void PlayerSay(Connection con, string text);
    }
}
