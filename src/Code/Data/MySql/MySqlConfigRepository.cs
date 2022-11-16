using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApplicationConges.Model;

namespace WebApplicationConges.Data
{
    public class MySqlConfigRepository : MySqlBase, IConfigRepository
    {
        public int Order { get { return 5; } }

        public Config Get()
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                Config defaultConfig = cnn.Query<Config>(Data.ConfigRepository.GetQuerySelect(), null).FirstOrDefault();
                if (defaultConfig == null)
                    cnn.Query<dynamic>(Data.ConfigRepository.GetQueryInsert(), new Config());

                return cnn.Query<Config>(Data.ConfigRepository.GetQuerySelect(), null).FirstOrDefault();
            }
        }

        public void Update(Config config)
        {
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Query<dynamic>(Data.ConfigRepository.GetQueryUpdate(), config);
            }
        }

        public void Create()
        {
            Data.ConfigRepository.RestoreTableName();
            using (var cnn = DbConnection())
            {
                cnn.Open();
                cnn.Execute(@"use " + DB_NAME);
                cnn.Execute(@"create TABLE IF NOT EXISTS " + Data.ConfigRepository.TABLE_NAME + @"
                      (
                        appadminlogin varchar(50) not null,
                        appadminpwd varchar(64) not null,
                        appadminemail varchar(50) not null,

                        ldap tinyint (0),
                        ldapconnectionstring varchar(255) null,
                        ldapfilter varchar(64) null,

                        smtp tinyint (0),
                        smtpserver varchar(255) null,
                        smtpport tinyint (25),
                        ssl tinyint (0),
                        smtptargetname varchar(255) null,
                        smtpdefaultcredentialenabled tinyint (0),

                        direxport varchar(50) null,
                        dirbackupbdd varchar(50) null,
                        
                        vapidsubject varchar(255) null,
                        vapidpublickey varchar(255) null,
                        vapidprivatekey varchar(255) null,

                        futuruse1 varchar(255) null,
                        futuruse2 varchar(255) null,
                        futuruse3 varchar(255) null,
                        futuruse4 varchar(255) null
                      )");
            }
        }
    }
}
