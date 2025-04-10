using System;
using Akka.Actor;
using Akka.DependencyInjection;

namespace Akka.Cluster.StreamingStatus.Actors
{
    public sealed class ClusterStatusManager : ReceiveActor
    {
        public IDependencyResolver Resolver { get; } = DependencyResolver.For(Context.System).Resolver;

        public ClusterStatusManager()
        {
            Receive<BeginMonitor>(m =>
            {
                var childName = Uri.EscapeDataString(m.ConnectionId);
                Context.Child(childName).GetOrElse(() =>
                {
                    var props = Resolver.Props<ClusterStatusActor>(m.ConnectionId);
                    return Context.ActorOf(props, childName);
                });
            });
        }
    }
}