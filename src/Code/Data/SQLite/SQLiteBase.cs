﻿using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace WebApplicationConges.Data
{
    public class SQLiteBase : IDb
    {
        private const String DB_FILE_NAME = "data.sqlite";
        public static String DB_NAME { get; set; }
        public static String ConnectionString { get; set; }

        public System.Data.Common.DbConnection DbConnection()
        {
            return new SqliteConnection("Data Source=" + DB_FILE_NAME);
        }

        public IConfigRepository ConfigRepository { get { return new SQLiteConfigRepository(); } }
        public ILogRepository LogRepository { get { return new SQLiteLogRepository(); } }
        public IUserRepository UserRepository { get { return new SQLiteUserRepository(); } }
        public IServiceRepository ServiceRepository { get { return new SQLiteServiceRepository(); } }
        public IManagerRepository ManagerRepository { get { return new SQLiteManagerRepository(); } }
        public ICongeRepository CongeRepository { get { return new SQLiteCongeRepository(); } }

        public void Reset()
        {
            // Nothing here
        }

        public void Backup(string fullpath)
        {
            fullpath = Path.ChangeExtension(fullpath, "sqlite");
            using (var cnn = DbConnection())
            {
                cnn.Open();

                // Backup in "live"
                using (var backup = new SqliteConnection("Data Source=" + fullpath))
                {
                    (cnn as SqliteConnection).BackupDatabase(backup);
                    backup.Close();
                }

                cnn.Close();

                // To avoid error message " ... used by another process"
                SqliteConnection.ClearAllPools();

                using var archive = ZipFile.Open(Path.ChangeExtension(fullpath, "zip"), ZipArchiveMode.Create);
                {
                    archive.CreateEntryFromFile(fullpath, Path.GetFileName(fullpath), CompressionLevel.Optimal);
                }

                // Keep only zip file
                File.Delete(fullpath);
            }
        }

        public void Load(string fullpath)
        {
            // To avoid error message " ... used by another process"
            SqliteConnection.ClearAllPools();

            // Replace bdd file
            File.Move(fullpath, DB_FILE_NAME, true);

            // Create new table if needed
            Init();
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