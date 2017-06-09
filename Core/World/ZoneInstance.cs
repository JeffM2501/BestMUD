using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Game;
using Networking;

namespace Core.World
{
    public class ZoneInstance
    {
        public Zone HostedZones = null;

        public List<Connection> ConnectedPlayers = new List<Connection>(); // 

        public int MaxPlayers = 0;

        public bool Full() { lock (ConnectedPlayers) return ConnectedPlayers.Count >= MaxPlayers; }

    }
}
