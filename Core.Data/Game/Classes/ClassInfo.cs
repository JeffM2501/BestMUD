using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Common;

namespace Core.Data.Game.Classes
{
    public class ClassInfo
    {
        public int ClassID = int.MinValue;
        public string Name = string.Empty;

        public List<int> RestrictedRaces = new List<int>();
        public List<int> AllowedRaces = new List<int>();

        public AttributeList DefaultAttributeBonuses = new AttributeList();
        public List<string> DefaultInventory = new List<string>();
        public List<string> DefaultFeatures = new List<string>();
    }
}
