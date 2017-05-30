﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Game.Classes;

namespace Core.Databases.GameData
{
    public class ClassDB : SQLiteDB
    {
        public static ClassDB Instance = new ClassDB();

        protected Dictionary<int, ClassInfo> ClassCache = new Dictionary<int, ClassInfo>();

        protected override void ValidateDatabase()
        {
            base.ValidateDatabase();

            string sql = "SELECT name FROM " + DB.Database + ".sqlite_master WHERE type='table' AND name='classes';";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            var results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE classes ( classID INTEGER PRIMARY KEY AUTOINCREMENT,name TEXT, allowedRaces TEXT, restrictedRaces TEXT, attributes TEXT,equipment TEXT,features TEXT);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }

            Reload();
        }
        
        protected ClassInfo ReadClassData(SQLiteDataReader results)
        {
            ClassInfo c = new ClassInfo();

            if (results != null && DB != null)
            {
                c.ClassID = results.GetInt32(0);
                c.Name = results.GetString(1);

                try
                {
                    foreach (string s in results.GetString(2).Split(";".ToCharArray()))
                        c.AllowedRaces.Add(int.Parse(s));
                }
                catch (Exception /*ex*/) { }

                try
                {
                    foreach (string s in results.GetString(3).Split(";".ToCharArray()))
                        c.RestrictedRaces.Add(int.Parse(s));
                }
                catch (Exception /*ex*/) { }

                c.DefaultAttributeBonuses.AddRange(results.GetString(4).Split(";".ToCharArray()));
                c.DefaultInventory.AddRange(results.GetString(5).Split(";".ToCharArray()));
                c.DefaultFeatures.AddRange(results.GetString(6).Split(";".ToCharArray()));
            }
            return c;
        }

        protected void Reload()
        {
            if (DB != null)
            {
                lock (ClassCache)
                {
                    ClassCache.Clear();

                    string sql = "SELECT * FROM classes;";
                    SQLiteCommand command = new SQLiteCommand(sql, DB);

                    var results = command.ExecuteReader();
                    if (results.HasRows)
                    {
                        while (results.Read())
                            ClassCache.Add(results.GetInt32(0), ReadClassData(results));
                    }
                }
                
            }
        }

        protected ClassInfo FindClass(int id)
        {
            lock (ClassCache)
            {
                if (ClassCache.ContainsKey(id))
                    return ClassCache[id];
            }
            return null;
        }

        protected ClassInfo[] GetClassList()
        {
            lock (ClassCache)
                return ClassCache.Values.ToArray();
        }
    }
}
