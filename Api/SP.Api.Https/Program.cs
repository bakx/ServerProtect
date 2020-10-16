using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SP.Api.Https
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			IConfigurationRoot config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
				.AddJsonFile("config/logSettings.json", false, true)
				.Build();

			ILogger log = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.CreateLogger();

			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
				.UseSerilog(log);
		}
	}
}