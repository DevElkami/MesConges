using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplicationConges.Model;
using static WebApplicationConges.Model.Conge;

namespace WebApplicationConges.Data
{
    public class SQLiteCongeRepository : SQLiteBase, ICongeRepository
    {
        public int Order { get { return 1; } }

        public List<Conge> GetAll()
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<Conge>(cnn.Query<Conge>(Data.CongeRepository.GetQueryAll(), null));
            }
        }

        public List<Conge> Get(String userId, StateEnum state)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<Conge>(cnn.Query<Conge>(Data.CongeRepository.GetQuerySelectByUserIdAndState(), new { userId, state }));
            }
        }

        public List<Conge> Get(String userId, StateEnum state, bool isExported)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<Conge>(cnn.Query<Conge>(Data.CongeRepository.GetQuerySelectByUserIdStateIsExported(), new { userId, state, isExported }));
            }
        }

        public List<Conge> Get(String userId, bool canDeleted, StateEnum state)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<Conge>(cnn.Query<Conge>(Data.CongeRepository.GetQuerySelectByUserIdStateCanDeleted(), new { userId, state, canDeleted }));
            }
        }

        public List<Conge> Get(String userId, StateEnum state, CGTypeEnum cgType, DateTime beginPeriode, DateTime endPeriode)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return new List<Conge>(cnn.Query<Conge>(Data.CongeRepository.GetQuerySelectByUserIdStateTypeAndDate(), new { userId, state, cgType, beginPeriode, endPeriode }));
            }
        }

        public Conge Get(long id)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                return cnn.Query<Conge>(Data.CongeRepository.GetQuerySelectById(), new { id }).FirstOrDefault();
            }
        }

        public void Insert(Conge conge)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.CongeRepository.GetQueryInsert(), conge);
            }
        }

        public void Update(Conge conge)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.CongeRepository.GetQueryUpdate(), conge);
            }
        }

        public void Delete(Conge conge)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.CongeRepository.GetQueryDelete(), conge);
            }
        }

        public void Create()
        {
            Data.CongeRepository.RestoreTableName();
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"create TABLE IF NOT EXISTS " + Data.CongeRepository.TABLE_NAME + @"
                      (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id varchar(50) not null,                        
                        motif varchar(255) null,
                        state int not null,
                        cgtype int not null,
                        dbegin datetime not null,
                        dend datetime not null,                       
                        dcreate datetime not null,
                        dmodify datetime not null,        
                        isExported tinyint (0),
                        canDeleted tinyint (0),
                        FOREIGN KEY (user_id)
                            REFERENCES " + Data.UserRepository.TABLE_NAME + @"(email)
                            ON DELETE CASCADE
                      )");
            }
        }
    }
}
