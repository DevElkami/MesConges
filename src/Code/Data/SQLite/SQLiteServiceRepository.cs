using Dapper;
using System.Collections.Generic;
using System.Linq;
using WebApplicationConges.Model;

namespace WebApplicationConges.Data
{
    public class SQLiteServiceRepository : SQLiteBase, IServiceRepository
    {
        public int Order { get { return 4; } }

        public List<Service> GetAll()
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<Service>(cnn.Query<Service>(Data.ServiceRepository.GetQueryAll(), null));
            }
        }

        public Service Get(int id)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return cnn.Query<Service>(Data.ServiceRepository.GetQuerySelectById(), new { id }).FirstOrDefault();
            }
        }

        public void Insert(Service service)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.ServiceRepository.GetQueryInsert(), service);
            }
        }

        public void Update(Service service)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.ServiceRepository.GetQueryUpdate(), service);
            }
        }

        public void Delete(Service service)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.ServiceRepository.GetQueryDelete(), service);
            }
        }

        public void Create()
        {
            Data.ServiceRepository.RestoreTableName();
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"create TABLE IF NOT EXISTS " + Data.ServiceRepository.TABLE_NAME + @"
                      (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name varchar(50) not null,
                        description varchar(255) null
                      )");
            }
        }
    }
}
