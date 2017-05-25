using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Game.Classes
{
    public class ClassInfo
    {
        public int ClassID = int.MinValue;
        public string Name = string.Empty;

        public List<int> RestrictedRaces = new List<int>();
        public List<int> AllowedRaces = new List<int>();

        public List<string> DefaultAttributeBonuses = new List<string>();
        public List<string> DefaultInventory = new List<string>();
        public List<string> DefaultFeatures = new List<string>();
    }
}
