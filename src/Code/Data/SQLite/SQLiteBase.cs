using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public class SQLiteBase : IDb
    {
        public static String DB_NAME { get; set; }
        public static String ConnectionString { get; set; }

        public System.Data.Common.DbConnection DbConnection()
        {
            return new SqliteConnection("Data Source=data.sqlite");
        }

        public IUserRepository UserRepository { get { return new SQLiteUserRepository(); } }
        public IServiceRepository ServiceRepository { get { return new SQLiteServiceRepository(); } }
        public IManagerRepository ManagerRepository { get { return new SQLiteManagerRepository(); } }
        public ICongeRepository CongeRepository { get { return new SQLiteCongeRepository(); } }

        public void Reset()
        {
            // Nothing here
        }

        public void Init()
        {
            // Création de la db
            using (var cnn = DbConnection())
            {
                cnn.Open();                
                cnn.Close();
            }

            // Création des tables et des colonnes, migration
            IEnumerable<IRepository> repositories = Db.GetRepositories(this);
            foreach (IRepository repository in repositories)
                repository.Create();
        }
    }
}