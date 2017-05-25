using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Game.Characters
{
    public class PlayerCharacter
    {
        public bool ReadOnly { get; protected set; }

        public PlayerCharacter(bool readOnly)
        {
            ReadOnly = readOnly;
        }

        public int UID = int.MinValue;

        public int UserID = int.MinValue;

        public string Name = string.Empty;
        public int Level = 0;
        public int Experience = 0;

        public int RaceID = int.MinValue;
        public int ClassID = int.MinValue;

        public List<string> Attributes = new List<string>();
        public List<string> Equipment = new List<string>();
        public List<string> Inventory = new List<string>();

        public Dictionary<Tuple<int,string>, string> QuestData = new Dictionary<Tuple<int, string>, string>();
    }
}
