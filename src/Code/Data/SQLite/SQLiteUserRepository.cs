using System.Linq;
using Dapper;
using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public class SQLiteUserRepository : SQLiteBase, IUserRepository
    {
        public int Order { get { return 2; } }

        public List<User> GetAll()
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<User>(cnn.Query<User>(Data.UserRepository.GetQueryAll(), null));
            }
        }

        public List<User> GetAll(long serviceId)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<User>(cnn.Query<User>(Data.UserRepository.GetQuerySelectByService(), new { serviceId }));
            }
        }

        public User Get(String email)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return cnn.Query<User>(Data.UserRepository.GetQuerySelectByEmail(), new { email }).FirstOrDefault();
            }
        }

        public User Get(String login, String hash)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return cnn.Query<User>(Data.UserRepository.GetQuerySelectByLoginAndHash(), new { login, hash }).FirstOrDefault();
            }
        }

        public List<User> GetDrh()
        {
            {
                using (var cnn = DbConnection())
                {
                    cnn.Open();
                    return new List<User>(cnn.Query<User>(Data.UserRepository.GetQuerySelectDrh(), null));
                }
            }
        }

        public List<User> GetAdmin()
        {
            {
                using (var cnn = DbConnection())
                {
                    cnn.Open();
                    return new List<User>(cnn.Query<User>(Data.UserRepository.GetQuerySelectAdmin(), null));
                }
            }
        }

        public void Insert(User user)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.UserRepository.GetQueryInsert(), user);
            }
        }

        public void Update(User user)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.UserRepository.GetQueryUpdate(), user);
            }
        }

        public void Delete(User user)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.UserRepository.GetQueryDelete(), user);
            }
        }

        public void Create()
        {
            Data.UserRepository.RestoreTableName();
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"create TABLE IF NOT EXISTS " + Data.UserRepository.TABLE_NAME + @"
                      (
                        email varchar(50) not null primary key,
                        login varchar(50) null,
                        hash varchar(100) null,
                        isdrh tinyint (0), 
                        isadmin tinyint (0),
                        service_id int not null,
                        name varchar(50) not null,
                        date_create datetime null,                        
                        surname varchar(50) null,
                        family_name varchar(50) null,
                        phone_number varchar(20) null,
                        description varchar(50) null,
                        last_connection datetime null,
                        matricule varchar(20) null,
                        FOREIGN KEY (service_id)
                            REFERENCES " + Data.ServiceRepository.TABLE_NAME + @"(id)
                      )");
            }
        }
    }
}
