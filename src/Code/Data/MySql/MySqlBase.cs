using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

namespace WebApplicationConges.Data
{
    public class MySqlBase : IDb
    {
        public static String DB_NAME { get; set; }
        public static String ConnectionString { get; set; }

        public System.Data.Common.DbConnection DbConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public IConfigRepository ConfigRepository { get { return new MySqlConfigRepository(); } }
        public ILogRepository LogRepository { get { return new MySqlLogRepository(); } }
        public IUserRepository UserRepository { get { return new MySqlUserRepository(); } }
        public IServiceRepository ServiceRepository { get { return new MySqlServiceRepository(); } }
        public IManagerRepository ManagerRepository { get { return new MySqlManagerRepository(); } }
        public ICongeRepository CongeRepository { get { return new MySqlCongeRepository(); } }

        public void Reset()
        {
            // Nothing here
        }

        public void Backup(string fullpath)
        {
            fullpath = Path.ChangeExtension(fullpath, "sql");
            using (var cnn = DbConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = cnn as MySqlConnection;
                        cnn.Open();
                        mb.ExportToFile(fullpath);
                        cnn.Close();
                    }
                }
            }

            using var archive = ZipFile.Open(Path.ChangeExtension(fullpath, "zip"), ZipArchiveMode.Create);
            {
                archive.CreateEntryFromFile(fullpath, Path.GetFileName(fullpath), CompressionLevel.Optimal);
            }

            // Keep only zip file
            File.Delete(fullpath);
        }

        public void Load(string fullpath)
        {
            // Not implemented
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