using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public interface IManagerRepository : IRepository
    {
        List<Manager> GetAll();
        Manager Get(String userId);
        Manager GetByServiceId(long serviceId);
        void Insert(Manager manager);
        void Delete(Manager manager);
        void Update(Manager manager);
    }

    public class ManagerRepository
    {
        private const String INTERNAL_NAME = "manager";
        public static String TABLE_NAME = INTERNAL_NAME;
        public static void RestoreTableName()
        {
            TABLE_NAME = INTERNAL_NAME;
        }

        private static String GetTableName()
        {
            return TABLE_NAME;
        }

        private static String SELECT = "SELECT id Id, service_id ServiceId";

        public static String GetQueryAll()
        {
            return SELECT + " FROM " + GetTableName();
        }

        public static String GetQuerySelectById()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE id = @userId";
        }

        public static String GetQuerySelectByServiceId()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE service_id = @serviceId";
        }

        public static String GetQueryInsert()
        {
            return @"INSERT INTO " + GetTableName() + @" ( id, service_id ) VALUES ( @Id, @ServiceId )";
        }

        public static String GetQueryUpdate()
        {
            return @"UPDATE " + GetTableName() + @" set service_id = @ServiceId WHERE id = @Id";
        }

        public static String GetQueryDelete()
        {
            return @"DELETE FROM " + GetTableName() + " WHERE id = @Id";
        }
    }
}