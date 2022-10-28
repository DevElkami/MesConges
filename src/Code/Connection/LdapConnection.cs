using System;
using System.Collections.Generic;
using System.DirectoryServices;
using WebApplicationConges.Model;

namespace WebApplicationConges.Connect
{
    public class LdapConnection : IConnection
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Valider la compatibilité de la plateforme", Justification = "<En attente>")]
        public User Connect(String login, String password)
        {
            DirectoryEntry de = new
                     (Toolkit.Configuration[Toolkit.ConfigEnum.LdapConnectionString.ToString()], login, password, AuthenticationTypes.Secure);

            DirectorySearcher search = new(de)
            {
                Filter = "(sAMAccountName=*" + login + ")"
            };
            search.SearchScope = SearchScope.Subtree;

            SearchResult result = search.FindOne();

            User user = GetUser(result);
            if (user != null)
                return user;

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Valider la compatibilité de la plateforme", Justification = "<En attente>")]
        public List<User> GetUsers()
        {
            DirectoryEntry de = new(Toolkit.Configuration[Toolkit.ConfigEnum.LdapConnectionString.ToString()]);
            DirectorySearcher search = new(de)
            {
                Filter = Toolkit.Configuration[Toolkit.ConfigEnum.LdapFilter.ToString()]
            };
            search.SearchScope = SearchScope.Subtree;

            List<User> users = new();
            foreach (SearchResult result in search.FindAll())
            {
                User user = GetUser(result);
                if (user != null)
                    users.Add(user);
            }

            return users;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Valider la compatibilité de la plateforme", Justification = "<En attente>")]
        private User GetUser(SearchResult result)
        {
            if ((result != null)
                && (result.Properties["mail"].Count > 0)
                && (result.Properties["sn"].Count > 0)
                && (result.Properties["displayname"].Count > 0)
                && (result.Properties["givenname"].Count > 0)
                && (result.Properties["samaccountname"].Count > 0))
            {
                User user = new()
                {
                    Email = result.Properties["mail"][0].ToString(),
                    FamilyName = result.Properties["sn"][0].ToString(),
                    Name = result.Properties["displayname"][0].ToString(),
                    Surname = result.Properties["givenname"][0].ToString(),
                    Login = result.Properties["samaccountname"][0].ToString()
                };
                return user;
            }

            return null;
        }
    }
}
