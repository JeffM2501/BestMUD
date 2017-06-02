using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Common;

namespace Core.Data.Game.Characters
{
    public class PlayerCharacter
    {
        public bool ReadOnly { get; protected set; }

        protected object DirtyLocker = new object();
        protected bool InternalDirty = false;

        public bool Dirty { get { lock (DirtyLocker) return InternalDirty && !ReadOnly; } set { lock (DirtyLocker) InternalDirty = value; } }

        public PlayerCharacter(bool readOnly)
        {
            ReadOnly = readOnly;
        }

        public void FinalizeCreate(int uid)
        {
            UID = uid;
            ReadOnly = false;
        }

        public int UID = int.MinValue;

        public int UserID = int.MinValue;

        public string Name = string.Empty;
        public int Level = 0;
        public int Experience = 0;

        public int RaceID = int.MinValue;
        public int ClassID = int.MinValue;

        public AttributeList Attributes = new AttributeList();
        public List<string> Equipment = new List<string>();
        public List<string> Inventory = new List<string>();

        public KeyValueList ExtraAttributes = new KeyValueList();

        public Dictionary<Tuple<int, string>, string> QuestData = new Dictionary<Tuple<int, string>, string>();
    }
}
