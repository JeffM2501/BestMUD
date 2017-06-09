using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Common;
using Core.Data.Game;
using Utilities;

namespace Core.Databases.GameData
{
    public class ZoneDB : SQLiteDB
    {
        public static ZoneDB Instance = new ZoneDB();

        public Dictionary<int, Room> RoomCache = new Dictionary<int, Room>();

        protected override void ValidateDatabase()
        {
            base.ValidateDatabase();

            string sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='zones';";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            var results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE zones (zoneID INTEGER PRIMARY KEY AUTOINCREMENT,name TEXT,attributes TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }

            sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='rooms';";
            command = new SQLiteCommand(sql, DB);
            results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE rooms (roomID INTEGER PRIMARY KEY AUTOINCREMENT,name TEXT,attributes TEXT, zoneID INTEGER REFERENCES zones(zoneID));";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }

            sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='exits';";
            command = new SQLiteCommand(sql, DB);
            results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE exits(exitID INTEGER PRIMARY KEY AUTOINCREMENT,roomID INTEGER REFERENCES rooms(roomID),direction INTEGER,destinationRoomID INTEGER REFERENCES rooms(roomID), destinationExitID INTEGER, attributes TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }
        }

        public int GetRoomZone(int roomID)
        {
            List<int> l = new List<int>();

            string sql = "SELECT zoneID FROM rooms WHERE roomID=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@id", roomID));

            var results = command.ExecuteReader();
            if (results.HasRows && results.Read())
                return results.GetFieldInt(0);

            return -1;
        }

        public List<int> GetRoomIndexList()
        {
            List<int> l = new List<int>();

            string sql = "SELECT roomID FROM rooms;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);

            var results = command.ExecuteReader();
            if (results.HasRows)
            {
                while (results.Read())
                    l.Add(results.GetInt32(0));
            }

            return l;
        }

        public List<int> RoomIndexesWithAttribute(string attribute)
        {
            List<int> l = new List<int>();

            string sql = "SELECT roomID FROM rooms WHERE attributes LIKE @att;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@att", "%" + attribute + "%"));

            var results = command.ExecuteReader();
            if (results.HasRows)
            {
                while (results.Read())
                    l.Add(results.GetInt32(0));
            }

            return l;
        }

        public List<Zone> GetAllZones()
        {
            Dictionary<int, Zone> zones = new Dictionary<int, Zone>();

            foreach(int id in GetRoomIndexList())
            {
                Room r = new Room();
                ReadRoomData(id, r);

                if (!zones.ContainsKey(id))
                {
                    Zone z = new Zone();
                    z.ID = r.ZoneID;
                    string sql = "SELECT name, attributes FROM zones WHERE zoneID=@zID;";
                    SQLiteCommand command = new SQLiteCommand(sql, DB);
                    command.Parameters.Add(new SQLiteParameter("@zID", z.ID));

                    var results = command.ExecuteReader();
                    if (results.HasRows && results.Read())
                    {
                        z.Name = results.GetFieldString(0);
                        z.Attributes = KeyValueList.DeserlizeFromString(results.GetFieldString(1));
                    }

                    zones.Add(z.ID, z);
                }

                zones[r.ZoneID].Rooms.Add(r);
            }

            return new List<Zone>(zones.Values.ToArray());
        }

        public Room GetRoom(int id )
        {
            if (RoomCache.ContainsKey(id))
                return RoomCache[id];

            Room r = new Room();
            ReadRoomData(id, r);
            RoomCache.Add(id, r);
            return r;
        }

        public void RefreshRoom(int id)
        {
            if (!RoomCache.ContainsKey(id))
                return;

            ReadRoomData(id, RoomCache[id]);
        }

        public int AddRoom(Room room)
        {
            string tempName = RNG.PsudoGUID();

            string sql = "INSERT into rooms (name) VALUES (@name);";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", tempName));
            command.ExecuteNonQuery();

            sql = "SELECT roomID from rooms where name=@name;";

            command.Parameters.Add(new SQLiteParameter("@name", tempName));
            var results = command.ExecuteReader();
            if (!results.HasRows || !results.Read())
                return -1;

            room.UID = results.GetInt32(0);
            WriteRoomData(room);

            return room.UID;
        }

        public int AddZone(Zone zone)
        {
            string tempName = RNG.PsudoGUID();

            string sql = "INSERT into zones (name) VALUES (@name);";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", tempName));
            command.ExecuteNonQuery();

            sql = "SELECT zoneID from zones where name=@name;";
            command.Parameters.Add(new SQLiteParameter("@name", tempName));

            var results = command.ExecuteReader();
            if (!results.HasRows || !results.Read())
                return -1;

            zone.ID = results.GetInt32(0);

            sql = "UPDATE zones SET (name=@name, attributes=@att) where zoneID=@id;";
            command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", zone.Name));
            command.Parameters.Add(new SQLiteParameter("@att", zone.Attributes.SerializeToText()));
            command.Parameters.Add(new SQLiteParameter("@id", zone.ID));
            command.ExecuteNonQuery();

            foreach (var r in zone.Rooms)
            {
                r.ZoneID = zone.ID;
                WriteRoomData(r);
            }
            return zone.ID;
        }

        public int AddExit(Room room, Room.Exit exit)
        {
            string tempName = RNG.PsudoGUID();

            string sql = "INSERT into exits (roomID, attributes) VALUES (@id, @name);";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@roomID", room.UID));
            command.Parameters.Add(new SQLiteParameter("@name", tempName));
            command.ExecuteNonQuery();

            sql = "SELECT exitID from exits where roomID=@id AND attributes=@name;";
            command.Parameters.Add(new SQLiteParameter("@id", room.UID));
            command.Parameters.Add(new SQLiteParameter("@name", tempName));

            var results = command.ExecuteReader();
            if (!results.HasRows || !results.Read())
                return -1;

            exit.ID = results.GetInt32(0);
            return exit.ID;
        }

        public void DeleteExit(Room room, Room.Exit exit)
        {
            string sql = "DELETE FROM exits WHERE exitID=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@id", exit.ID));
            command.ExecuteNonQuery();

            FileTools.DeleteFile(Paths.DataPath, "zone", room.UID.ToString(), "exit_" + exit.ID.ToString() + ".data");

            room.Exits.Remove(exit);
        }

        protected void WriteRoomData(Room room)
        {
            string sql = "UPDATE rooms SET (Name=@name, zoneID=@zID, attributes=@att) where roomID=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", room.Name));
            command.Parameters.Add(new SQLiteParameter("@att", room.Attributes.SerializeToText()));
            command.Parameters.Add(new SQLiteParameter("@zID", room.ZoneID));
            command.Parameters.Add(new SQLiteParameter("@id", room.UID));
            command.ExecuteNonQuery();

            FileTools.SetFileContents(Paths.DataPath, "zone", room.UID.ToString(), "desc.data", room.Description);

            foreach(var exit in room.Exits)// set the exits to current data
            {
                sql = "UPDATE exits Set (direction=@dir, destinationZoneID=@destRID, destinationExitID =@destEID, attributes=@att) WHERE exitID=@eID AND roomID=@rID;";
                command = new SQLiteCommand(sql, DB);
                command.Parameters.Add(new SQLiteParameter("@eID", exit.ID));
                command.Parameters.Add(new SQLiteParameter("@rID", room.UID));
                command.Parameters.Add(new SQLiteParameter("@dir", (int)exit.Direction));
                command.Parameters.Add(new SQLiteParameter("@destRID", exit.Destination));
                command.Parameters.Add(new SQLiteParameter("@destEID", exit.DesinationExit));
                command.Parameters.Add(new SQLiteParameter("@att", exit.Attributes.SerializeToText()));
                command.ExecuteNonQuery();

                FileTools.SetFileContents(Paths.DataPath, "zone", room.UID.ToString(), "exit_" + exit.ID.ToString() + ".data", exit.Description);
            }
        }

        protected bool ReadRoomData(int id, Room room)
        {
            room.UID = id;

            string sql = "SELECT name, zoneID, attributes FROM rooms WHERE roomID=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@id", id));

            var results = command.ExecuteReader();
            if (!results.HasRows || !results.Read())
                return false;

            room.Name = results.GetFieldString(0);
            room.ZoneID = results.GetFieldInt(1);
            room.Attributes = KeyValueList.DeserlizeFromString(results.GetFieldString(2));

            room.Description = FileTools.GetFileContents(Paths.DataPath, "zone", room.UID.ToString(), "desc.data", true);

            sql = "SELECT * FROM exits WHERE roomID=@id;";
            command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@id", id));

            results = command.ExecuteReader();
            if (!results.HasRows)
                return true;

            room.Exits.Clear();

            while (results.Read())
            {
                Room.Exit exit = new Room.Exit();
                exit.ID = results.GetInt32(0);
                //var roomID = results.GetInt32(1); 
                exit.Direction = (Directions)results.GetFieldInt(2);
                exit.Destination = results.GetFieldInt(3);
                exit.DesinationExit = results.GetFieldInt(4);
                exit.Attributes = KeyValueList.DeserlizeFromString(results.GetFieldString(5));

                exit.Description = FileTools.GetFileContents(Paths.DataPath, "zone", room.UID.ToString(), "exit_" + exit.ID.ToString() + ".data", true);

                room.Exits.Add(exit);
            }

            return true;
        }
    }
}
