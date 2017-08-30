namespace Unsettling.Dequeue
{
    using EasyNetQ;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Unsettling.Messages;

    public class ConsoleAppRunner : IServer
    {
        public IFeatureCollection Features { get; }
        
        private EasyNetQ.IBus bus;

        public void Dispose()
        {
            bus.Dispose();
        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {
            // docker.local is set in the HOSTS file to the Docker machine's IP.
            var hostString = string.Format("host={0}", "docker.local");
            Console.WriteLine($"host: {hostString}");
            bus = RabbitHutch.CreateBus(hostString);
            bus.Subscribe<RiskCheckMessage>(
                "riskCheck",
                msg =>
                    Console.WriteLine($"appId: {msg.Input.ApplicationId}"));

            // Keep the program running. The Done property is a
            // ManualResetEventSlim instance which gets set if someone terminates
            // the program.
            Program.Done.Wait();
        }

        public Task StartAsync<TContext>(
            IHttpApplication<TContext> application,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
        public static ManualResetEventSlim Done = new ManualResetEventSlim(false);

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder().UseStartup(typeof(Startup)).Build();
            using (var cts = new CancellationTokenSource())
            {
                void Shutdown()
                {
                    if (!cts.IsCancellationRequested)
                    {
                        Console.WriteLine("Application is shutting down...");
                        cts.Cancel();
                    }

                    Done.Wait(cts.Token);
                }

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Shutdown();
                    // Don't terminate the process immediately, wait for the Main
                    // thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                host.Run(cts.Token);
                Done.Set();
            }
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServer, ConsoleAppRunner>();
        }
    }
}
