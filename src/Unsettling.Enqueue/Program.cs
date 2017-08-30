namespace Unsettling.Enqueue
{
    using System;
    using EasyNetQ;
    using Unsettling.Messages;
    using Unsettling.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            // docker.local is set in the HOSTS file to the Docker machine's IP.
            var endpoint = "docker.local";
            var host = $"host={endpoint}";
            var appId = Guid.NewGuid().ToString();
            var model =
                new RiskCheckRequest
                {
                    ApplicationId = appId
                };
            var msg = new RiskCheckMessage { Input = model };
            using (var bus = RabbitHutch.CreateBus(host))
            {
                bus.Publish(msg);
            }
        }
    }
}