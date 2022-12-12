using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public interface ILogRepository : IRepository
    {
        List<Log> GetAll();       
        void Insert(Log log);
    }

    public class LogRepository
    {
        private const String INTERNAL_NAME = "logs";
        public static String TABLE_NAME = INTERNAL_NAME;
        public static void RestoreTableName()
        {
            TABLE_NAME = INTERNAL_NAME;
        }

        private static String GetTableName()
        {
            return TABLE_NAME;
        }

        private static String SELECT = "select id Id, user_id UserId, description Description";

        public static String GetQueryAll()
        {
            return SELECT + " FROM " + GetTableName();
        }

        public static String GetQueryInsert()
        {
            return @"INSERT INTO " + GetTableName() + " ( user_id, description ) VALUES ( @UserId, @Description )";
        }
    }
}