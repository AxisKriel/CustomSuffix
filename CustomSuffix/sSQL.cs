using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace CustomSuffix
{
    public class sSQL
    {
        private static IDbConnection db;

        public static void SetupDB()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] host = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                        host[0],
                        host.Length == 1 ? "3306" : host[1],
                        TShock.Config.MySqlDbName,
                        TShock.Config.MySqlUsername,
                        TShock.Config.MySqlPassword)
                    };
                    break;
                case "sqlite":
                    string sql = Path.Combine(TShock.SavePath, "AquaBlitz11", "CustomSuffix", "CustomSuffix.sqlite");
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;
            }
            SqlTableCreator sqlcreator = new SqlTableCreator(db,
                db.GetSqlType() == SqlType.Sqlite
                ? (IQueryBuilder)new SqliteQueryCreator()
                : new MysqlQueryCreator());
            sqlcreator.EnsureExists(new SqlTable("Suffixes",
                new SqlColumn("UserID", MySqlDbType.Int32) { Primary = true },
                new SqlColumn("Suffix", MySqlDbType.String),
                new SqlColumn("Status", MySqlDbType.Int32)));
        }

        public static string GetSuffix(int UserID)
        {
            string query = "SELECT Suffix FROM Suffixes WHERE UserID=@0;";
            using (var reader = db.QueryReader(query, UserID))
            {
                if (reader.Read())
                    return reader.Get<string>("Suffix");
            }
            return null;
        }

        public static bool GetStatus(int UserID)
        {
            string query = "SELECT Status FROM Suffixes WHERE UserID=@0";
            using (var reader = db.QueryReader(query, UserID))
            {
                if (reader.Read())
                    return reader.Get<int>("Status") == 1 ? true : false;
            }
            return false;
        }

        public static bool AddSuffix(int UserID, string Suffix, bool Status)
        {
            String query = "INSERT INTO Suffixes (UserID, Suffix, Status) VALUES (@0, @1, @2);";

            try { db.Query(query, UserID, Suffix, (Status ? 1 : 0)); return true; }
            catch { return false; }
        }

        public static bool UpdateSuffix(int UserID, string Suffix, bool Status)
        {
            String query = "UPDATE Suffixes SET Suffix=@1, Status=@2 WHERE UserID=@0;";

            if (db.Query(query, UserID, Suffix, Status ? 1 : 0) != 1)
                return false;
            return true;
        }

        public static void RemoveSuffix(int UserID)
        {
            String query = "DELETE FROM Suffixes WHERE UserID=@0;";
            db.Query(query, UserID);
        }
    }
}
