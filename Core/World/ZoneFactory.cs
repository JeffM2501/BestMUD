using Core.Databases.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.World
{
    public static class ZoneFactory
    {
        public static RuntimeZoneInstance SpawnZone(int zoneID)
        {
            RuntimeZoneInstance inst = new RuntimeZoneInstance(ZoneDB.Instance.GetZone(zoneID));
            inst.Run();
            return inst;
        }
    }
}
