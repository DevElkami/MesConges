using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public interface IServiceRepository : IRepository
    {
        Service Get(int serviceId);
        List<Service> GetAll();
        void Insert(Service service);
        void Delete(Service service);
        void Update(Service service);
    }

    public class ServiceRepository
    {
        private const String INTERNAL_NAME = "service";
        public static String TABLE_NAME = INTERNAL_NAME;
        public static void RestoreTableName()
        {
            TABLE_NAME = INTERNAL_NAME;
        }

        private static String GetTableName()
        {
            return TABLE_NAME;
        }

        private static String SELECT = @"SELECT id Id, name Name, description Description";

        public static String GetQueryAll()
        {
            return SELECT + " FROM " + GetTableName();
        }

        public static String GetQuerySelectById()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE id = @serviceId";
        }

        public static String GetQueryInsert()
        {
            return "INSERT INTO " + GetTableName() + " ( name, description ) VALUES  ( @Name, @Description )";
        }

        public static String GetQueryUpdate()
        {
            return "UPDATE " + GetTableName() + " set name = @Name, description = @Description WHERE id = @Id";
        }

        public static String GetQueryDelete()
        {
            return "delete from " + GetTableName() + " WHERE id = @Id";
        }
    }
}