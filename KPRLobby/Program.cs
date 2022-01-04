using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace KPRLobby
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .UseUrls("http://localhost:4000")
                              .UseUrls("https://localhost:4001");

                });
    }
}