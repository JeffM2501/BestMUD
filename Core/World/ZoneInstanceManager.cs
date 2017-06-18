using Core.Databases.GameData;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.World
{
    public static class ZoneInstanceManager
    {
        private static Dictionary<int, List<RuntimeZoneInstance>> ActiveZones = new Dictionary<int, List<RuntimeZoneInstance>>();
        public static RuntimeZoneInstance GetInstance(Connection user, int zoneID, int roomID)
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
                zone = ZoneList.Find(x => !x.Full());
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

        public static void SetUserToZone(Connection user, int zoneID, int roomID)
        {
            if (user.CurrentZoneProcessor as RuntimeZoneInstance != null)
                (user.CurrentZoneProcessor as RuntimeZoneInstance).RemoveUser(user);

            user.CurrentZoneProcessor = null;
            if (zoneID >= 0)
                user.CurrentZoneProcessor = GetInstance(user, zoneID, roomID);
        }

        public static void Update()
        {
            List<int> zoneKeys = null;
            lock (ActiveZones)
                zoneKeys = new List<int>(ActiveZones.Keys);

            foreach (var key in zoneKeys)
            {
                List<RuntimeZoneInstance> zoneList = null;
                lock (ActiveZones)
                    zoneList = ActiveZones[key];

                List<RuntimeZoneInstance> deadZones = new List<RuntimeZoneInstance>();
                lock (zoneList)
                {
                    foreach(var zone in zoneList)
                    {
                        if (zone.Delitable())
                        {
                            zone.Kill();
                            deadZones.Add(zone);
                        }
                    }

                    zoneList.RemoveAll(x => deadZones.Contains(x));
                }

                foreach(var zone in deadZones)
                {
                    if (zone.Primary)
                    {
                        foreach (var r in zone.HostedZone.Rooms.Values)
                        {
                            // write room contents before flush...
                        }
                    }
                }
            }
        }

        public static void KillAll()
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
