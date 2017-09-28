using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Quest.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // set env. variable ActiveMQ to specify the message q
            var configuration = new ConfigurationBuilder()
              .AddCommandLine(args)
              .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
