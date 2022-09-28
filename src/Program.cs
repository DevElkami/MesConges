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
#if DEBUG
            Db.Instance.Init("Server=localhost;Port=3306;Uid=root;Pwd=acxbn45;Character Set=utf8;", Toolkit.Configuration[Toolkit.ConfigEnum.DbName.ToString()]);
#else
            Db.Instance.Init(Toolkit.Configuration[Toolkit.ConfigEnum.ConnectionString.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.DbName.ToString()]);
#endif
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
    }
}
