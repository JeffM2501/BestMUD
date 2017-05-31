using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

using Utilities;
using Core.Data.Game.Characters;
using Core.Data.Common;

namespace Core.Databases.PlayerData
{
    public class PlayerCharacterDB : SQLiteDB
    {
        public static PlayerCharacterDB Instance = new PlayerCharacterDB();

        protected List<PlayerCharacter> ActiveCharacters = new List<PlayerCharacter>();

        protected override void ValidateDatabase()
        {
            base.ValidateDatabase();

            string sql = "SELECT name FROM " + DB.Database + ".sqlite_master  WHERE type='table' AND name='characters';";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            var results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE characters ( characterID INTEGER PRIMARY KEY AUTOINCREMENT,userID INTEGER,enabled INTEGER,name TEXT,raceID INTEGER,level INTEGER,experience INTEGER,classID INTEGER, attributeData TEXT,equipmentData TEXT,inventoryData TEXT );";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }

            sql = "SELECT name FROM " + DB.Database + ".sqlite_master  WHERE type='table' AND name='questData';";
            command = new SQLiteCommand(sql, DB);
            results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE questData ( ID INTEGER PRIMARY KEY AUTOINCREMENT,chracterID INTEGER, valueName TEXT, valuedata TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }
        }

        public List<PlayerCharacter> GetUserCharacters(int userID)
        {
            List<PlayerCharacter> pcs = new List<PlayerCharacter>();

            if (DB != null)
            {
                string sql = "SELECT * FROM characters WHERE userID=@uid AND enabled=1;";
                SQLiteCommand command = new SQLiteCommand(sql, DB);
                command.Parameters.Add(new SQLiteParameter("@uid", userID));

                var results = command.ExecuteReader();
                if (results.HasRows)
                {
                    while (results.Read())
                        pcs.Add(ReadPCData(results, false));
                }
            }
            return pcs;
        }

        protected PlayerCharacter ReadPCData(SQLiteDataReader results, bool getQuestData)
        {
            PlayerCharacter pc = new PlayerCharacter(!getQuestData);

            if (results != null && DB != null)
            {
                pc.UID = results.GetInt32(0);
                pc.UserID = results.GetInt32(1);
                //int enabled = results.GetInt32(2);
                pc.Name = results.GetString(3);
                pc.RaceID = results.GetInt32(4);
                pc.Level = results.GetInt32(5);
                pc.Experience = results.GetInt32(6);
                pc.ClassID = results.GetInt32(7);

                pc.Attributes = AttributeList.DeserlizeFromString(results.GetString(8));
                pc.Equipment.AddRange(results.GetString(9).Split(";".ToCharArray()));
                pc.Inventory.AddRange(results.GetString(10).Split(";".ToCharArray()));

                if (getQuestData && DB != null)
                {
                    string sql = "SELECT * FROM qyestData WHERE characterID=@cid;";
                    SQLiteCommand command = new SQLiteCommand(sql, DB);
                    command.Parameters.Add(new SQLiteParameter("@cid", pc.UID));

                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                            pc.QuestData.Add(new Tuple<int, string>(reader.GetInt32(0), reader.GetString(1)), reader.GetString(2));
                    }
                }
            }

            return pc;
        }

        public PlayerCharacter CheckOutChracter(int userID, int characterID)
        {
            if (DB != null)
            {
                string sql = "SELECT * FROM characters WHERE userID=@uid AND characterID=@cid AND enabled=1;";
                SQLiteCommand command = new SQLiteCommand(sql, DB);
                command.Parameters.Add(new SQLiteParameter("@uid", userID));
                command.Parameters.Add(new SQLiteParameter("@cid", characterID));

                var results = command.ExecuteReader();
                if (results.HasRows)
                {
                    results.Read();
                    var pc = ReadPCData(results, true);
                    lock (ActiveCharacters)
                        ActiveCharacters.Add(pc);
                    return pc;
                }
            }

            return null;
        }

        public void CheckInCharacter(PlayerCharacter pc)
        {
            lock (ActiveCharacters)
                ActiveCharacters.Remove(pc);

            if (pc.Dirty)
                SavePlayerCharacter(pc);
        }

        public void SavePlayerCharacter(PlayerCharacter pc)
        {
            lock(pc)
            {
                if (pc.ReadOnly)
                    return;

                string sql = "UPDATE characters  Set name=@name, raceID=@raceID, level=@level, experience=@exp, classID=@classID, attributeData=@att, equipmentData=@equip, inventoryData=@inv WHERE characterID=@cid AND userID=@uid AND enabled=1);";
                SQLiteCommand command = new SQLiteCommand(sql, DB);

                command.Parameters.Add(new SQLiteParameter("@name", pc.Name));
                command.Parameters.Add(new SQLiteParameter("@faceID", pc.RaceID));
                command.Parameters.Add(new SQLiteParameter("@classID", pc.ClassID));
                command.Parameters.Add(new SQLiteParameter("@level", pc.Level));
                command.Parameters.Add(new SQLiteParameter("@exp", pc.Experience));

                command.Parameters.Add(new SQLiteParameter("@att", pc.Attributes.SerializeToText());
                command.Parameters.Add(new SQLiteParameter("@equip", string.Join(";", pc.Equipment.ToArray())));
                command.Parameters.Add(new SQLiteParameter("@inv", string.Join(";", pc.Inventory.ToArray())));

                command.Parameters.Add(new SQLiteParameter("@uid", pc.UserID));
                command.Parameters.Add(new SQLiteParameter("@cid", pc.UID));
                command.ExecuteNonQuery();

                pc.Dirty = false;
            }
        }

        public bool PCNameExists(string name)
        {
            if (DB == null)
                return false;

            if (name.Trim() == string.Empty)
                return true;

            string sql = "SELECT characterID FROM characters WHERE name=@name AND enabled=1;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", name));

            var results = command.ExecuteReader();
            if (!results.HasRows)
                return false;

            return true;
        }

        public PlayerCharacter GetCharacterCloneByID(int characterID)
        {
            if (DB != null)
            {
                string sql = "SELECT * FROM characters WHERE characterID=@cid AND enabled=1;";
                SQLiteCommand command = new SQLiteCommand(sql, DB);
                command.Parameters.Add(new SQLiteParameter("@cid", characterID));

                var results = command.ExecuteReader();
                if (results.HasRows)
                {
                    results.Read();
                    return ReadPCData(results, false);
                }
            }

            return null;
        }

        public PlayerCharacter CreatePlayerCharacter(PlayerCharacter pc)
        {
            if (PCNameExists(pc.Name))
                return null;

            lock(pc)
            {
                string sql = "INSERT INTO characters (userID, enabled, name) VALUES(@uid,1, @name);";
                SQLiteCommand command = new SQLiteCommand(sql, DB);
                command.Parameters.Add(new SQLiteParameter("@uid", pc.UserID));
                command.Parameters.Add(new SQLiteParameter("@name", pc.Name));
                command.ExecuteNonQuery();

                sql = "SELECT characterID FROM characters WHERE name=@name AND userID=@uid AND enabled=1;";
                command = new SQLiteCommand(sql, DB);
                command.Parameters.Add(new SQLiteParameter("@name", pc.Name));
                command.Parameters.Add(new SQLiteParameter("@uid", pc.UserID));

                var results = command.ExecuteReader();
                if (!results.HasRows)
                    return null;
                results.Read(); ;
                pc.FinalizeCreate(results.GetInt32(0));
                SavePlayerCharacter(pc);

                return pc;
            }
        }
    }
}
