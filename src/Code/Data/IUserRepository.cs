using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public interface IUserRepository : IRepository
    {
        List<User> GetAll();
        User Get(String email);
        User Get(String login, String hash);
        List<User> GetAll(long serviceId);
        List<User> GetDrh();
        List<User> GetAdmin();
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
        
        private static String SELECT = @"SELECT email Email, login Login, hash HashPwd, isdrh IsDrh, isadmin IsAdmin, service_id ServiceId, name Name, date_create DateCreate, surname Surname, 
                                         family_name FamilyName, phone_number PhoneNumber, description Description, last_connection LastConnection, matricule Matricule";

        public static String GetQueryAll()
        {
            return SELECT + " FROM " + GetTableName();
        }

        public static String GetQuerySelectByEmail()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE email = @email";
        }

        public static String GetQuerySelectByLoginAndHash()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE login = @login and hash = @hash";
        }

        public static String GetQuerySelectByService()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE service_id = @serviceId";
        }

        public static String GetQuerySelectDrh()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE isdrh = true";
        }

        public static String GetQuerySelectAdmin()
        {
            return SELECT + " FROM " + GetTableName() + " WHERE isadmin = true";
        }

        public static String GetQueryInsert()
        {
            return @"INSERT INTO " +
                    GetTableName() +
                    @" ( name, date_create, email, login, hash, isdrh, isadmin, service_id, surname, family_name, phone_number, description, last_connection, matricule ) VALUES 
                    ( @Name, @DateCreate, @Email, @Login, @HashPwd, @IsDrh, @IsAdmin, @ServiceId, @Surname, @FamilyName, @PhoneNumber, @Description, @LastConnection, @Matricule )";
        }

        public static String GetQueryUpdate()
        {
            return @"update " + GetTableName() +
                    @" set login = @Login, name = @Name, email = @Email, hash = @HashPwd, isdrh = @IsDrh, isadmin = @IsAdmin, service_id = @ServiceId, surname = @Surname, 
                    family_name = @FamilyName, phone_number = @PhoneNumber, description = @Description, last_connection = @LastConnection, matricule = @Matricule
                    WHERE email = @Email";
        }

        public static String GetQueryDelete()
        {
            return @"delete from " + GetTableName() + @" WHERE email = @Email";
        }
    }
}