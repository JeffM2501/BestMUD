using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.World
{
    public class ZoneInstanceManager
    {
        public Dictionary<int, List<RuntimeZoneInstance>> ActiveZones = new Dictionary<int, List<RuntimeZoneInstance>>();
        public RuntimeZoneInstance GetInstance(Connection user, int zoneID, int roomID)
        {
            List<RuntimeZoneInstance> ZoneList = null;
            lock(ActiveZones)
            {
                if (!ActiveZones.ContainsKey(zoneID))
                    ActiveZones.Add(zoneID, new List<RuntimeZoneInstance>());

                ZoneList = ActiveZones[zoneID];
            }

            RuntimeZoneInstance zone = null;
            lock (ZoneList)
            {
                zone = ZoneList.Find(x => !x.IsEmpty());
                if (zone == null)
                {
                    // gotta add a new one
                    zone = ZoneFactory.SpawnZone(zoneID);
                    zone.Primary = ZoneList.Count == 0;

                    ZoneList.Add(zone);
                }
            }

            zone.AddUser(user);
            return zone;
        }

        public void Update()
        {

        }

        public void KillAll()
        {
            lock (ActiveZones)
            {
                foreach (var z in ActiveZones.Values)
                {
                    lock (z)
                    {
                        foreach (var i in z)
                        {
                            lock (i)
                                i.Kill();
                        }
                    }

                    z.Clear();
                }

                ActiveZones.Clear();
            }
        }
    }
}
