using System;
using System.IO;
using System.Data.SQLite;

namespace Core.Databases
{
    public class SQLiteDB
    {
        protected FileInfo DBFile = null;
        protected SQLiteConnection DB = null;

        public void Setup(string connectString)
        {
            DBFile = new FileInfo(connectString);

            if (!DBFile.Directory.Exists)
                DBFile.Directory.Create();

            if (!DBFile.Exists)
            {
                SQLiteConnection.CreateFile(DBFile.FullName);
                if (!File.Exists(DBFile.FullName))
                    return;
            }

            if (DB != null)
                DB.Close();

            DB = new SQLiteConnection("Data Source=" + DBFile.FullName);
            if (DB != null)
                DB.Open();

            ValidateDatabase();
        }

        protected virtual void ValidateDatabase()
        {
            if (DB == null)
                return;
        }

     
    }

    public static class ReaderUtils
    {
        public static string GetFieldString(this SQLiteDataReader results, int index)
        {
            if (results.IsDBNull(index) || index < 0 || index >= results.FieldCount)
                return string.Empty;

            return results.GetString(index);
        }

        public static int GetFieldInt(this SQLiteDataReader results, int index)
        {
            if (results.IsDBNull(index) || index < 0 || index >= results.FieldCount)
                return int.MinValue;

            return results.GetInt32(index);
        }
    }
}
