using WebApplicationConges.Model;
using System;
using System.Collections.Generic;
using Org.BouncyCastle.Utilities;

namespace WebApplicationConges.Data
{
    public interface ILogRepository : IRepository
    {
        List<Log> GetAll();
        void DeleteOld();
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

        private static String SELECT = "select id Id, user_id UserId, actiondate ActionDate, description Description";

        public static String GetQueryAll()
        {
            return $"{SELECT} FROM {GetTableName()} ORDER BY actiondate DESC";
        }

        public static String GetQueryDeleteOld()
        {
            return $"DELETE FROM {GetTableName()} WHERE id IN (SELECT id FROM {GetTableName()} ORDER BY actiondate ASC LIMIT 100)";
        }

        public static String GetQueryInsert()
        {
            return @"INSERT INTO " + GetTableName() + " ( user_id, actiondate, description ) VALUES ( @UserId, @ActionDate, @Description )";
        }
    }
}