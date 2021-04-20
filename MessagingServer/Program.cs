using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessagingServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    ConfigureServices(services);
                });

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CommandHandler>();
            services.AddSingleton<ICommandExecutor, MessageExecutor>();
        }
    }
}