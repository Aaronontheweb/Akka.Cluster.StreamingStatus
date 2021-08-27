using Akka.Streams.SignalR.AspNetCore;
using Akka.Streams.SignalR.AspNetCore.Internals;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Akka.Cluster.StreamingStatus.Hubs
{
    public class ClusterStream : StreamConnector
    {
        public ClusterStream(IStreamingStatusService service, IHubClients clients, ConnectionSourceSettings sourceSettings = null, ConnectionSinkSettings sinkSettings = null) 
            : base(clients, sourceSettings, sinkSettings)
        {
            // populate the data so it's available to Akka.NET
            service.Connect(Source, Sink);
        }
    }
    
    public class ClusterStatusHub : StreamHub<ClusterStream>
    {
        public ClusterStatusHub(IStreamDispatcher dispatcher) : base(dispatcher)
        {
        }

    }

    public static class ClusterStatusHubExtensions
    {
        public static IServiceCollection AddClusterStatusServices(this IServiceCollection services)
        {
            services.AddSingleton<SignalRStatusService>();
            services.AddSingleton<IStreamingStatusService, SignalRStatusService>();
            services.AddSingleton<IStreamingStatusInterface, SignalRStatusService>();
            
            return services;
        }
    }
}