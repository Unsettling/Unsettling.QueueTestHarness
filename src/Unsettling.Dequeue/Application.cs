namespace Unsettling.Dequeue
{
    using EasyNetQ;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http.Features;
    using System;
    using Unsettling.Messages;
    
    public class Application : IServer
    {
        public IFeatureCollection Features { get; }
        
        private IBus bus;

        public void Dispose()
        {
            bus.Dispose();
        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {
            // The Docker machine's IP.
            var hostString = string.Format("host={0}", "192.168.99.100");
            Console.WriteLine($"host: {hostString}");
            bus = RabbitHutch.CreateBus(hostString);
            bus.Subscribe<RiskCheckMessage>(
                "RiskCheck",
                msg =>
                    Console.WriteLine($"appId: {msg.Input.ApplicationId}"));

            // Keep the program running. The Done property is a
            // ManualResetEventSlim instance which gets set if someone terminates
            // the program.
            Program.Done.Wait();
        }
    }
}
