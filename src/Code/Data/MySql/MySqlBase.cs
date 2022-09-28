using Dapper;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public class MySqlBase : IDb
    {
        public static String DB_NAME { get; set; }
        public static String ConnectionString { get; set; }

        public System.Data.Common.DbConnection DbConnection()
        {
            return new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
        }

        public IUserRepository UserRepository { get { return new MySqlUserRepository(); } }
        public IServiceRepository ServiceRepository { get { return new MySqlServiceRepository(); } }
        public IManagerRepository ManagerRepository { get { return new MySqlManagerRepository(); } }
        public ICongeRepository CongeRepository { get { return new MySqlCongeRepository(); } }

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
                cnn.Execute(@"CREATE DATABASE IF NOT EXISTS " + DB_NAME + " DEFAULT CHARACTER SET utf8 DEFAULT COLLATE utf8_general_ci;");
                cnn.Close();
            }

            // Création des tables et des colonnes, migration
            IEnumerable<IRepository> repositories = Db.GetRepositories(this);
            foreach (IRepository repository in repositories)
                repository.Create();
        }
    }
}