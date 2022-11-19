using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace WebApplicationConges.Code.SignalR
{
    public class CongesHub : Hub
    {        
        public async Task Notify(int type, String mailFrom, String mailTo, String subject, String body)
        {
            await this.Clients.All.SendAsync("Notification", type, mailFrom, mailTo, subject, body);
        }
    }
}
