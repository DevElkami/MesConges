using System;
using System.Collections.Generic;
using WebApplicationConges.Model;

namespace WebApplicationConges.Connect
{
    public interface IConnection
    {
        User Connect(String login, String password);
        List<User> GetUsers();
    }
}
