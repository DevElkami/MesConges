using System.Linq;
using Dapper;
using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public class MySqlManagerRepository : MySqlBase, IManagerRepository
    {
        public int Order { get { return 3; } }

        public List<Manager> GetAll()
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                return new List<Manager>(cnn.Query<Manager>(Data.ManagerRepository.GetQueryAll(), null));
            }
        }

        public Manager Get(String userId)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                return cnn.Query<Manager>(Data.ManagerRepository.GetQuerySelectById(), new { userId }).FirstOrDefault();
            }
        }

        public Manager GetByServiceId(long serviceId)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                return cnn.Query<Manager>(Data.ManagerRepository.GetQuerySelectByServiceId(), new { serviceId }).FirstOrDefault();
            }
        }

        public void Insert(Manager manager)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                cnn.Query<dynamic>(Data.ManagerRepository.GetQueryInsert(), manager);
            }
        }

        public void Update(Manager manager)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                cnn.Query<dynamic>(Data.ManagerRepository.GetQueryUpdate(), manager);
            }
        }

        public void Delete(Manager manager)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                cnn.Query<dynamic>(Data.ManagerRepository.GetQueryDelete(), manager);
            }
        }

        public void Create()
        {
            Data.ManagerRepository.RestoreTableName();
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                cnn.Execute(@"create TABLE IF NOT EXISTS " + Data.ManagerRepository.TABLE_NAME + @"
                      (
                        id varchar(50) not null primary key,
                        service_id int not null,
                        FOREIGN KEY (service_id)
                            REFERENCES " + Data.ServiceRepository.TABLE_NAME + @"(id)
                            ON DELETE CASCADE
                      )");
            }
        }
    }
}
