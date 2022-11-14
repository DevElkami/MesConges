using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace WebApplicationConges.Code.SignalR
{
    public class CongesHub : Hub
    {        
        public async Task Notify(String aMessage)
        {
            await this.Clients.All.SendAsync("Notification", aMessage);
        }
    }
}
