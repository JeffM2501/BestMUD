﻿using System;

using System.Data.SQLite;
using Utilities;

namespace Core.Databases.Authentication
{
    public class AuthenticaitonDB : SQLiteDB
    {
        public static AuthenticaitonDB Instance = new AuthenticaitonDB();

        internal class UserInfo
        {
            public string UserGUID = string.Empty;
            public string UserName = string.Empty;
            public string CryptoPass = string.Empty;
            public string AccessFlags = string.Empty;
        }

        protected override void ValidateDatabase()
        {
            base.ValidateDatabase();

            string sql = "SELECT name FROM "+ DB.Database+".sqlite_master  WHERE type='table' AND name='users';";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            var results = command.ExecuteReader();
            if (!results.HasRows)
            {
                sql = "CREATE TABLE users (userID INTEGER PRIMARY KEY AUTOINCREMENT, username TEXT, passhash TEXT, authFlags TEXT, lastAuth TEXT, enabled INTEGER);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();

                sql = "CREATE INDEX IF NOT EXISTS idx_users ON users (username ASC);";
                command = new SQLiteCommand(sql, DB);
                command.ExecuteNonQuery();
            }
        }

        public bool UserExists(string name)
        {
            if (DB == null)
                return false;

            if (name.Trim() == string.Empty)
                return true;

            string sql = "SELECT userID FROM users WHERE username=@name AND enabled=1;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", name));

            var results = command.ExecuteReader();
            if (!results.HasRows)
                return false;

            return true;
        }

        public bool CreateUser(string name, string password, string accessFlags)
        {
            if (UserExists(name))
                return false;

            if (DB == null)
                return false;

            string sql = "INSERT INTO  users (username, passhash, authFlags, enabled) VALUES(@name, @hash, @flags,1);";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", name));
            command.Parameters.Add(new SQLiteParameter("@hash", CryptoTools.LocalCryptString(password)));
            command.Parameters.Add(new SQLiteParameter("@flags", accessFlags));

            command.ExecuteNonQuery();

            return UserExists(name);
        }

        public bool AuthenticateUser(string name, string password, out string accessFlags, out int userID)
        {
            accessFlags = string.Empty;
            userID = -1;

            if (DB == null || !UserExists(name))
                return false;

            string sql = "SELECT userID, passhash, authFlags FROM users WHERE username=@name AND enabled=1;";
            SQLiteCommand command = new SQLiteCommand(sql, DB);
            command.Parameters.Add(new SQLiteParameter("@name", name));

            var results = command.ExecuteReader();
            if (!results.HasRows || !results.Read())
                return false;

            accessFlags = results.GetString(2);
            bool valid = CryptoTools.LocalDecryptString(results.GetString(1)) == password;
            if (!valid)
                LogCache.Log(LogCache.BasicLog, "Invalid authentication for " + name);
            else
            {
                userID = results.GetInt32(0);
                sql = "UPDATE users SET lastAuth=@now WHERE userID=@uid;";
                command = new SQLiteCommand(sql, DB);
                command.Parameters.Add(new SQLiteParameter("@now", DateTime.Now.ToString()));
                command.Parameters.Add(new SQLiteParameter("@uid", userID));
                command.ExecuteNonQuery();
            }

            return valid;
        }
    }
}
