using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using WebApplicationConges.Data;

namespace WebApplicationConges
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Toolkit.InitConfiguration();
            Db.Instance.Init(Toolkit.Configuration[Toolkit.ConfigEnum.DbConnectionString.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.DbName.ToString()]);

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
    }
}
