using System;
using System.Collections.Generic;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Connect
{
    public class DbConnection : IConnection
    {
        public User Connect(String login, String password)
        {            
            return Db.Instance.DataBase.UserRepository.Get(login, Toolkit.CreateSHAHash(password));
        }

        public List<User> GetUsers()
        {         
            return Db.Instance.DataBase.UserRepository.GetAll();
        }        
    }
}
