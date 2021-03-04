using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FabricMQ.Broker.Identity.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using FabricMQ.Broker.SignalR;

// Documentation at https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-communication
namespace FabricMQ.Broker
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class Broker : StatefulService
    {
        public IWebHost WebHost { get; private set; }

        public Broker(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "KestrelServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        WebHost = new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureAppConfiguration((builderContext, config) =>
                                    {
                                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                                    })
                                    .ConfigureLogging(logging =>  
                                    {  
                                        logging.ClearProviders();  
                                        logging.AddConsole();   
                                        logging.AddEventSourceLogger();
                                        logging.SetMinimumLevel(LogLevel.Debug);
                                    })
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatefulServiceContext>(serviceContext)
                                            .AddSingleton<IReliableStateManager>(this.StateManager))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                        return WebHost;
                    }))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            using (var scope = WebHost.Services.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<SeedAuth>().SeedAsync();
                await scope.ServiceProvider.GetRequiredService<SeedDefaultMessageTypes>().SeedAsync();
            }

            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
