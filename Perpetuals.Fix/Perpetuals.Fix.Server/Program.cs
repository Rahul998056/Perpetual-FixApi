using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Perpetuals.Fix.Core.Configuration;
using Perpetuals.Fix.Core.Services;
using Perpetuals.Fix.Core.Services.Interface;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace Perpetuals.Fix.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {           
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {                    
                    services.Configure<UpstreamApiOptions>(context.Configuration.GetSection("UpstreamApi"));
                  
                    services.AddHttpClient<UpstreamService>();
                    services.AddSingleton<FixServerApp>();

                    services.AddScoped<IUpstreamService, UpstreamService>();
                    services.AddScoped<IFixServices,FixServices>();

                    services.AddSingleton<FixServerApp>();
                })
                .Build();

            var app = host.Services.GetRequiredService<FixServerApp>();
           
            var settings = new SessionSettings("server.cfg");
            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);

            var acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory);
            acceptor.Start();

            Console.WriteLine("FIX Server started. Press Enter to quit.");
            Console.ReadLine();

            acceptor.Stop();
        }
    }
}
