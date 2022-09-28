using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public interface IUserRepository : IRepository
    {
        List<User> GetAll();
        User Get(String email);
        List<User> GetAll(long serviceId);
        void Insert(User user);
        void Update(User user);
        void Delete(User user);
    }

    public class UserRepository
    {
        private const String INTERNAL_NAME = "user";
        public static String TABLE_NAME = INTERNAL_NAME;
        public static void RestoreTableName()
        {
            TABLE_NAME = INTERNAL_NAME;
        }

        private static String GetTableName()
        {
            return TABLE_NAME;
        }

        private static String SELECT = @"SELECT email Email, service_id ServiceId, name Name, date_create DateCreate, surname Surname, 
                                         family_name FamilyName, phone_number PhoneNumber, description Description, last_connection LastConnection, matricule Matricule";

        public static String GetQueryAll()
        {
            return SELECT + " FROM " + GetTableName();
        }

        public static String GetQuerySelectByEmail()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE email = @email";
        }

        public static String GetQuerySelectByService()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE service_id = @serviceId";
        }

        public static String GetQueryInsert()
        {
            return @"INSERT INTO " +
                    GetTableName() +
                    @" ( name, date_create, email, service_id, surname, family_name, phone_number, description, last_connection, matricule ) VALUES 
                    ( @Name, @DateCreate, @Email, @ServiceId, @Surname, @FamilyName, @PhoneNumber, @Description, @LastConnection, @Matricule )";
        }

        public static String GetQueryUpdate()
        {
            return @"update " + GetTableName() +
                    @" set name = @Name, email = @Email, service_id = @ServiceId, surname = @Surname, 
                    family_name = @FamilyName, phone_number = @PhoneNumber, description = @Description, last_connection = @LastConnection, matricule = @Matricule
                    WHERE email = @Email";
        }

        public static String GetQueryDelete()
        {
            return @"delete from " + GetTableName() + @" WHERE email = @Email";
        }
    }
}