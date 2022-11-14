using System;
using WebApplicationConges.Data;

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
                            if(Db.Instance.DataBase.ConfigRepository.Get().Ldap)
                                instance = new LdapConnection();
                            else
                                instance = new DbConnection();
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
