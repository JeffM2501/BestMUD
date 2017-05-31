using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Common;
using Core.Data.Game.Races;

namespace Core.Databases.GameData
{
    public class RaceDB : SQLiteDB
    {
        public static RaceDB Instance = new RaceDB();

        protected Dictionary<int, RaceInfo> RaceCache = new Dictionary<int, RaceInfo>();

        protected override void ValidateDatabase()
        {
            base.ValidateDatabase();

            string sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='races';";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            var results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE races (raceID INTEGER PRIMARY KEY AUTOINCREMENT,name TEXT, attributes TEXT,equipment TEXT,features TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }

            Reload();
        }

        protected RaceInfo ReadRaceData(SQLiteDataReader results)
        {
            RaceInfo race = new RaceInfo();

            if (results != null && DB != null)
            {
                race.RaceID = results.GetInt32(0);
                race.Name = results.GetString(1);
                race.DefaultAttributeBonuses = AttributeList.DeserlizeFromString(results.GetString(2));
                race.DefaultInventory.AddRange(results.GetString(3).Split(";".ToCharArray()));
                race.DefaultFeatures.AddRange(results.GetString(4).Split(";".ToCharArray()));
            }
            return race;
        }

        public void Reload()
        {
            if (DB != null)
            {
                lock (RaceCache)
                {
                    RaceCache.Clear();

                    string sql = "SELECT * FROM races;";
                    SQLiteCommand command = new SQLiteCommand(sql, DB);

                    var results = command.ExecuteReader();
                    if (results.HasRows)
                    {
                        while (results.Read())
                            RaceCache.Add(results.GetInt32(0), ReadRaceData(results));
                    }
                }
            }
        }

        public RaceInfo FindRace(int id)
        {
            lock(RaceCache)
            {
                if (RaceCache.ContainsKey(id))
                    return RaceCache[id];
            }
            return null;
        }

        public RaceInfo[] GetRaceList()
        {
            lock (RaceCache)
                return RaceCache.Values.ToArray();
        }

        public int[] GetRaceIndexList()
        {
            List<int> i = new List<int>();
            lock (RaceCache)
            {
                foreach(var r in RaceCache)
                {
                    i.Add(r.Key);
                }
            }

            return i.ToArray();
        }
    }
}
