using Core.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Game
{
    public class Room
    {
        public class Exit
        {
            public int ID = int.MinValue;
            public int Destination = int.MinValue;
            public int DesinationExit = int.MinValue;   // optional for "go back" verbs

            public Directions Direction = Directions.Unknown;
            public string Description = string.Empty;
            public List<string> Attributes = new List<string>();
        }

        public int UID = int.MinValue;
        public List<string> Attributes = new List<string>();
        public string Name = string.Empty;

        public string Description = string.Empty;
        public List<Exit> Exits = new List<Exit>();
    }

    public class Zone
    {
        public int ID = int.MinValue;
        public string Name = string.Empty;
        public List<Room> Rooms = new List<Room>();
    }
}
