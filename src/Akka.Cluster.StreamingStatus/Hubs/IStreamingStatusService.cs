using Akka.Streams.Dsl;
using Akka.Streams.SignalR.AspNetCore;

namespace Akka.Cluster.StreamingStatus.Hubs
{
    /// <summary>
    /// Pass this service in via DI so Akka.Streams can access it.
    /// </summary>
    public interface IStreamingStatusService
    {
        void Connect(Source<ISignalREvent, NotUsed> source, Sink<ISignalRResult, NotUsed> sink);
    }

    public interface IStreamingStatusInterface
    {
        /// <summary>
        /// Indicates that SignalR has populated the <see cref="Inbound"/>
        /// and <see cref="Outbound"/> values.
        /// </summary>
        bool HasValue { get; }
        
        Source<ISignalREvent, NotUsed> Inbound { get; }
        
        Sink<ISignalRResult, NotUsed> Outbound { get; }
    }
    
    public sealed class SignalRStatusService : IStreamingStatusInterface, IStreamingStatusService
    {
        public bool HasValue { get; private set; }
        
        public Source<ISignalREvent, NotUsed> Inbound { get; private set; }
        public Sink<ISignalRResult, NotUsed> Outbound { get; private set; }
        
        public void Connect(Source<ISignalREvent, NotUsed> source, Sink<ISignalRResult, NotUsed> sink)
        {
            if (!HasValue)
            {
                Inbound = source;
                Outbound = sink;
                HasValue = true;
            }
        }
    }
}