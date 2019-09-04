using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Totem
{
    public class Program
    {
        public static readonly string ApplicationName = typeof(Program).Assembly.GetName().Name;

        public static void Main(string[] args)
        {
            Console.Title = ApplicationName;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration(x => x.AddEnvironmentVariables(ApplicationName + ":"))
                .UseStartup<Startup>();
    }
}
