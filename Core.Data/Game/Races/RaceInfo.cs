using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Common;

namespace Core.Data.Game.Races
{
    public class RaceInfo
    {
        public int RaceID = int.MinValue;
        public string Name = string.Empty;

        public AttributeList DefaultAttributeBonuses = new AttributeList();
        public List<string> DefaultInventory = new List<string>();
        public List<string> DefaultFeatures = new List<string>();
    }
}
