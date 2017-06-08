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
        public static RaceDB Instance = new RaceDB();

        public Dictionary<int, Room> RoomCache = new Dictionary<int, Room>();

        protected override void ValidateDatabase()
        {
            base.ValidateDatabase();

            string sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='rooms';";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            var results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE rooms (roomID INTEGER PRIMARY KEY AUTOINCREMENT,name TEXT,attributes TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }

            sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='exits';";
            command = new SQLiteCommand(sql, DB);
            results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE exits(exitID INTEGER PRIMARY KEY AUTOINCREMENT,roomID INTEGER REFERENCES rooms(roomID),direction INTEGER,destinationZoneID INTEGER REFERENCES rooms(roomID), destinationExitID INTEGER, attributes TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }
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

            string sql = "INSERT into rooms (Name) VALUES (@name);";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", tempName));
            command.ExecuteNonQuery();

            sql = "SELECT roomID from rooms where name=@name;";

            command.Parameters.Add(new SQLiteParameter("@name", tempName));
            var results = command.ExecuteReader();
            if (!results.HasRows || !results.Read())
                return -1;

            room.UID = results.GetInt32(0);
            WriteRoomData(room.UID, room);

            return room.UID;
        }

        protected void WriteRoomData(int id, Room room)
        {
            string sql = "UPDATE rooms SET (Name=@name,attributes=@att) where roomID=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", room.Name));
            command.Parameters.Add(new SQLiteParameter("@att", room.Attributes.SerializeToText()));
            command.Parameters.Add(new SQLiteParameter("@id", id));
            command.ExecuteNonQuery();

            List<int> existingExits = new List<int>();
            sql = "SELECT exitID FROM exits where roomID=@id;";
            command.Parameters.Add(new SQLiteParameter("@id", id));

            var results = command.ExecuteReader();
        }

        protected bool ReadRoomData(int id, Room room)
        {
            room.UID = id;

            string sql = "SELECT * FROM rooms WHERE UID=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@id", id));

            var results = command.ExecuteReader();
            if (!results.HasRows || !results.Read())
                return false;

            room.Name = results.GetString(1);
            room.Attributes = KeyValueList.DeserlizeFromString(results.GetString(2));

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
                exit.Direction = (Directions)results.GetInt32(2);
                exit.Destination = results.GetInt32(3);
                exit.DesinationExit = results.GetInt32(4);
                exit.Attributes = KeyValueList.DeserlizeFromString(results.GetString(5));

                exit.Description = FileTools.GetFileContents(Paths.DataPath, "zone", room.UID.ToString(), "exit_" + exit.ID.ToString() + ".data", true);

                room.Exits.Add(exit);
            }

            return true;

        }
    }
}
