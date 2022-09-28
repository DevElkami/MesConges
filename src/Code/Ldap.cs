using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplicationConges.Model;

namespace WebApplicationConges
{
    public class Ldap
    {
        public static async Task<User> ConnectToLdap(String login, String password)
        {
            using (var cn = new LdapConnection())
            {
                cn.Connect(Toolkit.Configuration[Toolkit.ConfigEnum.LdapServer.ToString()], int.Parse(Toolkit.Configuration[Toolkit.ConfigEnum.LdapPort.ToString()]));
                cn.Bind(Toolkit.Configuration[Toolkit.ConfigEnum.Domain.ToString()] + login, password);

                LdapSearchQueue searchQueue = cn.Search(Toolkit.Configuration[Toolkit.ConfigEnum.LdapBase.ToString()], LdapConnection.SCOPE_SUB, $"(sAMAccountName={login})",
                    new string[] { "*" }, false, null as LdapSearchQueue);

                LdapMessage message;
                while ((message = searchQueue.getResponse()) != null)
                {
                    if (message is LdapSearchResult searchResult)
                    {
                        LdapEntry entry = searchResult.Entry;

                        User ldapUser = Convert(entry);
                        if (ldapUser != null)
                            return ldapUser;
                    }
                }
            }

            return null;
        }

        public static List<User> GetLdapUsers(String login, String password)
        {
            List<User> users = new List<User>();
            using (var cn = new LdapConnection())
            {
                cn.Connect(Toolkit.Configuration[Toolkit.ConfigEnum.LdapServer.ToString()], int.Parse(Toolkit.Configuration[Toolkit.ConfigEnum.LdapPort.ToString()]));
                cn.Bind(Toolkit.Configuration[Toolkit.ConfigEnum.Domain.ToString()] + login, password);

                LdapSearchQueue searchQueue = cn.Search(Toolkit.Configuration[Toolkit.ConfigEnum.LdapBase.ToString()], LdapConnection.SCOPE_SUB, "(sAMAccountName=*)",
                    new string[] { "*" }, false, null as LdapSearchQueue);

                LdapMessage message;
                while ((message = searchQueue.getResponse()) != null)
                {
                    if (message is LdapSearchResult searchResult)
                    {
                        LdapEntry entry = searchResult.Entry;

                        // On filtre pour ne récupérer que les comptes actifs
                        if (!String.IsNullOrEmpty(Toolkit.Configuration[Toolkit.ConfigEnum.LdapFilterName.ToString()]))
                        {
                            if ((entry.getAttribute(Toolkit.Configuration[Toolkit.ConfigEnum.LdapFilterName.ToString()]) == null) || (entry.getAttribute(Toolkit.Configuration[Toolkit.ConfigEnum.LdapFilterName.ToString()]).StringValue != Toolkit.Configuration[Toolkit.ConfigEnum.LdapFilterValue.ToString()]))
                                continue;
                        }

                        User ldapUser = Convert(entry);
                        if (ldapUser != null)
                            users.Add(ldapUser);
                    }
                }

                cn.Disconnect();
            }

            return users;
        }

        private static User Convert(LdapEntry entry)
        {
            User ldapUser = new User();
            if ((entry.getAttribute("MAIL") != null) && (entry.getAttribute("SN") != null) && (entry.getAttribute("DISPLAYNAME") != null) && (entry.getAttribute("GIVENNAME") != null))
            {
                ldapUser.Email = entry.getAttribute("MAIL").StringValue;
                ldapUser.FamilyName = entry.getAttribute("SN").StringValue;
                ldapUser.Name = entry.getAttribute("DISPLAYNAME").StringValue;
                ldapUser.Surname = entry.getAttribute("GIVENNAME").StringValue;
                return ldapUser;
            }

            return null;
        }
    }
}
