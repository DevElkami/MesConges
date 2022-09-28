using WebApplicationConges.Model;
using System;
using System.Collections.Generic;
using static WebApplicationConges.Model.Conge;

namespace WebApplicationConges.Data
{
    public interface ICongeRepository : IRepository
    {
        List<Conge> GetAll();
        List<Conge> Get(String userId, StateEnum state);
        List<Conge> Get(String userId, StateEnum state, bool isExported);
        List<Conge> Get(String userId, bool canDeleted, StateEnum state);
        List<Conge> Get(String userId, StateEnum state, CGTypeEnum cgType, DateTime beginPeriode, DateTime endPeriode);
        Conge Get(long id);
        void Insert(Conge conge);
        void Delete(Conge conge);
        void Update(Conge conge);
    }

    public class CongeRepository
    {
        private const String INTERNAL_NAME = "conge";
        public static String TABLE_NAME = INTERNAL_NAME;
        public static void RestoreTableName()
        {
            TABLE_NAME = INTERNAL_NAME;
        }

        private static String GetTableName()
        {
            return TABLE_NAME;
        }

        private static String SELECT = "select id Id, user_id UserId, motif Motif, state State, cgtype CGType, dbegin BeginDate, dend EndDate, dcreate CreateDate, dmodify ModifyDate, isExported IsExported, canDeleted CanDeleted";

        public static String GetQueryAll()
        {
            return SELECT + " FROM " + GetTableName();
        }

        public static String GetQuerySelectByUserIdAndState()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE user_id = @UserId and state = @state";
        }

        public static String GetQuerySelectByUserIdStateIsExported()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE user_id = @UserId and state = @state and isExported = @isExported";
        }

        public static String GetQuerySelectByUserIdStateCanDeleted()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE user_id = @UserId and state = @state and canDeleted = @canDeleted";
        }

        public static String GetQuerySelectByUserIdStateTypeAndDate()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE user_id = @UserId and state = @state and cgtype = @cgtype and dbegin BETWEEN @beginPeriode AND @endPeriode";
        }

        public static String GetQuerySelectById()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE id = @id";
        }

        public static String GetQueryInsert()
        {
            return @"INSERT INTO " + GetTableName() + " ( user_id, motif, state, cgtype, dbegin, dend, dcreate, dmodify, isExported, canDeleted ) VALUES ( @UserId, @Motif, @State, @CGType, @BeginDate, @EndDate, @CreateDate, @ModifyDate, @IsExported, @CanDeleted )";
        }

        public static String GetQueryUpdate()
        {
            return @"update " + GetTableName() + " set motif = @Motif, state = @State, cgtype = @CGType, dbegin = @BeginDate, dend = @EndDate, dcreate = @CreateDate, dmodify = @ModifyDate, isExported = @IsExported, canDeleted = @CanDeleted WHERE id = @Id";
        }

        public static String GetQueryDelete()
        {
            return @"delete from " + GetTableName() + " WHERE id = @Id";
        }
    }
}