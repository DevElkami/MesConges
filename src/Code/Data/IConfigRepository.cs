using WebApplicationConges.Model;
using System;
using System.Collections.Generic;

namespace WebApplicationConges.Data
{
    public interface IConfigRepository : IRepository
    {
        Config Get();
        void Update(Config config);
    }

    public class ConfigRepository
    {
        private const String INTERNAL_NAME = "config";
        public static String TABLE_NAME = INTERNAL_NAME;
        public static void RestoreTableName()
        {
            TABLE_NAME = INTERNAL_NAME;
        }

        private static String GetTableName()
        {
            return TABLE_NAME;
        }

        private static String SELECT = @"SELECT appadminlogin AppAdminLogin, appadminpwd AppAdminPwd, appadminemail AppAdminEmail,
                                        ldap Ldap, ldapconnectionstring LdapConnectionString, ldapfilter LdapFilter,
                                        smtp Smtp, smtpserver SmtpServer, smtpport SmtpPort, ssl SSl, smtptargetname SmtpTargetName, smtpdefaultcredentialenabled SmtpDefaultCredentialEnabled,
                                        direxport DirExport, dirbackupbdd DirBackupBdd,
                                        vapidsubject VAPIDSubject, vapidpublickey VAPIDPublicKey, vapidprivatekey VAPIDPrivateKey, futuruse1 FuturUse1";

        public static String GetQuerySelect()
        {
            return SELECT + " FROM " + GetTableName();
        }

        public static String GetQueryInsert()
        {
            return @"INSERT INTO " + GetTableName() + " ( appadminlogin, appadminpwd, appadminemail, "+
                 "ldap, ldapconnectionstring, ldapfilter, " +
                 "smtp, smtpserver, smtpport, ssl, smtptargetname, smtpdefaultcredentialenabled, " + 
                 "direxport, dirbackupbdd, " +
                 "vapidsubject, vapidpublickey, vapidprivatekey, futuruse1)" +
                " VALUES ( @AppAdminLogin, @AppAdminPwd, @AppAdminEmail, " +
                "@Ldap, @LdapConnectionString, @LdapFilter, " +
                "@Smtp, @SmtpServer, @SmtpPort, @SSl, @SmtpTargetName, @SmtpDefaultCredentialEnabled, " + 
                "@DirExport, @DirBackupBdd, " +
                "@VAPIDSubject, @VAPIDPublicKey, @VAPIDPrivateKey, @FuturUse1 )";
        }

        public static String GetQueryUpdate()
        {
            return "UPDATE " + GetTableName() + " set appadminlogin = @AppAdminLogin, appadminpwd = @AppAdminPwd, appadminemail = @AppAdminEmail," +
                "ldap = @Ldap, ldapconnectionstring = @LdapConnectionString, ldapfilter  = @LdapFilter," +
                "smtp = @Smtp, smtpserver = @SmtpServer, smtpport = @SmtpPort, ssl = @SSl, smtptargetname = @SmtpTargetName, smtpdefaultcredentialenabled = @SmtpDefaultCredentialEnabled," +
                "direxport = @DirExport, dirbackupbdd = @DirBackupBdd," +
                "vapidsubject = @VAPIDSubject, vapidpublickey = @VAPIDPublicKey, vapidprivatekey = @VAPIDPrivateKey, futuruse1 = @FuturUse1"
                ;
        }
    }
}