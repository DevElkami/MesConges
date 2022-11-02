using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using WebApplicationConges.Model;

namespace WebApplicationConges.Data
{
    public interface IDb
    {
        DbConnection DbConnection();
        void Init();
        void Backup(string fullpath);
        void Reset();
        IUserRepository UserRepository { get; }
        IManagerRepository ManagerRepository { get; }
        IServiceRepository ServiceRepository { get; }
        ICongeRepository CongeRepository { get; }
    }

    public class Db
    {
        static private Db instance = null;
        static private Object objectlock = new Object();

        public static IEnumerable<IRepository> GetRepositories(Object caller)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type commandType = typeof(IRepository);

            List<IRepository> repoCollection = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => (i == commandType)) && t.IsClass && (t.BaseType == caller.GetType()))
                .Select(t =>
                {
                    return (IRepository)Activator.CreateInstance(t);
                })
                .ToList();

            return repoCollection.OrderByDescending(a => a.Order);
        }

        #region Instance
        public static Db Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (objectlock)
                    {
                        if (instance == null)
                            instance = new Db();
                    }
                }
                return instance;
            }
        }

        private Db()
        {
        }
        #endregion

        public void Init(String connectionString, String dataBaseName)
        {
            if (Toolkit.Configuration[Toolkit.ConfigEnum.DbType.ToString()] == "mysql")
            {
                MySqlBase.ConnectionString = connectionString;
                MySqlBase.DB_NAME = dataBaseName;
                DataBase = new MySqlBase();
            }
            else
                DataBase = new SQLiteBase();

            DataBase.Init();
        }

        public IDb DataBase
        {
            get;
            set;
        }

        public User BeautifyUser(User user)
        {
            user.IsManager = Db.Instance.DataBase.ManagerRepository.Get(user.Email) != null;
            user.Service = Db.Instance.DataBase.ServiceRepository.Get(user.ServiceId);
            user.Manager = Db.Instance.DataBase.ManagerRepository.GetByServiceId(user.ServiceId);

            return user;
        }

        public List<User> GetDrh()
        {
            return Db.Instance.DataBase.UserRepository.GetDrh();
        }

        public List<User> GetAdmin()
        {
            return Db.Instance.DataBase.UserRepository.GetAdmin();
        }
    }
}