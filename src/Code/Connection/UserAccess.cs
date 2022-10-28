using System;

namespace WebApplicationConges.Connect
{
    public class UserAccess
    {
        static private IConnection instance = null;
        static private Object objectlock = new Object();

        #region Instance
        public static IConnection Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (objectlock)
                    {
                        if (instance == null)
                        {
                            // Select LDAP connection or database connection
                            String ldapStr = Toolkit.Configuration[Toolkit.ConfigEnum.Ldap.ToString()];
                            if(String.IsNullOrEmpty(ldapStr) || !bool.TryParse(ldapStr, out _) || !bool.Parse(ldapStr))
                                instance = new DbConnection();
                            else
                                instance = new LdapConnection();
                        }
                    }
                }
                return instance;
            }
        }

        private UserAccess()
        {
        }
        #endregion
    }
}
