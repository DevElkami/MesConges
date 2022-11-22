using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationConges.Model
{
    public class Config
    {
        // Admin part
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        [Required]
        [MaxLength(50)]
        [RegularExpression("[^<>:='\"]*")]
        public String AppAdminLogin { get; set; } = "administrator";

        // Default password SHA512 hash in base 64: holidays
        [MaxLength(64)]
        [Required]
        public String AppAdminPwd { get; set; } = "EFeFBj5tLUZhjyUBFgQex1INLcDYy67nQGhnjVEwsS4o0zqSDLjRofkDZkBtG6Ftib8gPrq4UOipLqwAR1rJVw==";

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        [RegularExpression("[^<>:='\"]*")]
        public String AppAdminEmail { get; set; } = "adminmail@yourdomain.com";

        // LDAP part
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool Ldap { get; set; } = false;

        [MaxLength(255)]
        [RegularExpression("[^<>'\"]*")]
        public String LdapConnectionString { get; set; } = "LDAP://192.168.7.1:389/dc=yourdc,dc=local";

        [MaxLength(64)]
        [RegularExpression("[^<>]*")]
        public String LdapFilter { get; set; } = "(sAMAccountName=*)";

        // SMTP part
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool Smtp { get; set; } = false;

        [MaxLength(255)]
        [RegularExpression("[^<>:='\"]*")]
        public String SmtpServer { get; set; } = "masociete-fr.mail.protection.outlook.com";

        public int SmtpPort { get; set; } = 25;
        public bool SSl { get; set; } = false; // Not implemented

        [MaxLength(255)]
        [RegularExpression("[^<>:='\"]*")]
        public String SmtpTargetName { get; set; } = "STARTTLS/smtp.office365.com"; // Not implemented

        public bool SmtpDefaultCredentialEnabled { get; set; } = false; // Not implemented

        // Export / backup directory part
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        [MaxLength(50)]
        [RegularExpression("[^<>:='\"]*")]
        public String DirExport { get; set; } = "ExportDir";

        [MaxLength(50)]
        [RegularExpression("[^<>:='\"]*")]
        public String DirBackupBdd { get; set; } = "BackupDir";

        // Notification part
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(254)]
        public String VAPIDSubject { get; set; }

        [RegularExpression("[^<>'\"]*")]
        [MaxLength(254)]
        public String VAPIDPublicKey { get; set; }

        [RegularExpression("[^<>'\"]*")]
        [MaxLength(254)]
        public String VAPIDPrivateKey { get; set; }

        // Extra holidays
        [MaxLength(254)]
        public String FuturUse1 { get; set; }

        public List<String> ExtraDaysOff 
        { 
            get 
            { 
                return new List<String>(FuturUse1.Split(','));
            } 
            set 
            {
                FuturUse1 = "";
                foreach (String date in value)                
                    FuturUse1 += (date + ",");

                FuturUse1 = FuturUse1.TrimEnd(trimChar: ',');
            } 
        }

        // Future use part
        ///////////////////////////////////////////////////////////////////////////////////////////////////////        

        [MaxLength(254)]
        public String FuturUse2 { get; set; }

        [MaxLength(254)]
        public String FuturUse3 { get; set; }

        [MaxLength(254)]
        public String FuturUse4 { get; set; }
    }
}