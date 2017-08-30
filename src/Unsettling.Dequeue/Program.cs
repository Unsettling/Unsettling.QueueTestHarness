namespace Unsettling.Dequeue
{
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Threading;

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
}
