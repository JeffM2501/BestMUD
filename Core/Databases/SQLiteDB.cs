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
}
