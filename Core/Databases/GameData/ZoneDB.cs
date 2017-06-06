using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Databases.GameData
{
    public class ZoneDB : SQLiteDB
    {
        public static RaceDB Instance = new RaceDB();

        protected override void ValidateDatabase()
        {
            base.ValidateDatabase();

            string sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='rooms';";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            var results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE room (roomID INTEGER PRIMARY KEY AUTOINCREMENT,name TEXT,attributes TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }

            sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='exits';";
            command = new SQLiteCommand(sql, DB);
            results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE exits(exitID INTEGER PRIMARY KEY AUTOINCREMENT,roomID INTEGER REFERENCES roooms(roomID),direction INTEGER,destinationZoneID INTEGER REFERENCES rooms(roomID), destinationExitID INTEGER, attributes TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }
        }
    }
}
