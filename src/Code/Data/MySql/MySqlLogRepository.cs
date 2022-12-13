using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplicationConges.Model;

namespace WebApplicationConges.Data
{
    public class MySqlLogRepository : MySqlBase, ILogRepository
    {
        public int Order { get { return 6; } }

        public List<Log> GetAll()
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<Log>(cnn.Query<Log>(Data.LogRepository.GetQueryAll(), null));
            }
        }

        public void DeleteOld()
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.LogRepository.GetQueryDeleteOld(), null);
            }
        }

        public void Insert(Log log)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.LogRepository.GetQueryInsert(), log);
            }
        }

        public void Create()
        {
            Data.LogRepository.RestoreTableName();
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                cnn.Execute(@"create TABLE IF NOT EXISTS " + Data.LogRepository.TABLE_NAME + @"
                      (
                        id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
                        user_id varchar(50) not null,
                        actiondate datetime not null,
                        description varchar(1024) not null                       
                      )");
            }
        }
    }
}
