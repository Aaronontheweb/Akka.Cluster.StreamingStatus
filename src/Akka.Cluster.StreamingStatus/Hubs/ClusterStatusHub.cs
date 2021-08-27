using System.Threading.Tasks;
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
        private readonly IClusterMonitor _monitor;
        
        public ClusterStatusHub(IStreamDispatcher dispatcher, IClusterMonitor monitor) : base(dispatcher)
        {
            _monitor = monitor;
        }

        public override Task OnConnectedAsync()
        {
            _monitor.StartMonitor(Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }

    public static class ClusterStatusHubExtensions
    {
        public static IServiceCollection AddClusterStatusServices(this IServiceCollection services)
        {
            services.AddSingleton<SignalRStatusService>();
            services.AddSingleton<IStreamingStatusService>(sp => sp.GetRequiredService<SignalRStatusService>());
            services.AddSingleton<IStreamingStatusInterface>(sp => sp.GetRequiredService<SignalRStatusService>());
            
            return services;
        }
    }
}