using System;
using System.Threading;
using Akka.Actor;
using Akka.Cluster.StreamingStatus.Hubs;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.SignalR.AspNetCore;
using Petabridge.Cmd;
using Petabridge.Cmd.Common.Client;
using Petabridge.Cmd.Host;

namespace Akka.Cluster.StreamingStatus.Actors
{
    public interface IWithConnectionId
    {
        string ConnectionId { get; }
    }
    
    public sealed class BeginMonitor : IWithConnectionId
    {
        public BeginMonitor(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }
    }
    
    public sealed class StopMonitor : IWithConnectionId
    {
        public StopMonitor(string connectionId, string? errorMessage)
        {
            ConnectionId = connectionId;
            ErrorMessage = errorMessage;
        }

        public string ConnectionId { get; }
        
        public string? ErrorMessage { get; }
    }

    public sealed class ClusterStatusActor : ReceiveActor
    {
        private readonly string _connectionId;
        
        /// <summary>
        /// Our connection to SignalR
        /// </summary>
        private readonly IStreamingStatusInterface _streamingInterface;

        /// <summary>
        /// Our Petabridge.Cmd client.
        /// </summary>
        private IPbmClient _pbmClient;

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private IKillSwitch _switch;

        public IMaterializer Materializer { get; } = Context.Materializer();

        public ClusterStatusActor(IStreamingStatusInterface streamingInterface, string connectionId)
        {
            _streamingInterface = streamingInterface;
            _connectionId = connectionId;

            ReceiveAsync<IPbmClient>(async client =>
            {
                _pbmClient = client;
                _log.Info("Received connection to local Petabridge.Cmd host...");

                _log.Info("Executing `pbm cluster tail` to begin tracking cluster events...");
                var cmdStream = await client.ExecuteTextCommandAsync("cluster tail");
                _switch = cmdStream.KillSwitch;

                var (completionTask, simpleStream) = cmdStream.Stream.PreMaterialize(Materializer);

                if (!_streamingInterface.HasValue)
                {
                    _log.Info("SignalR not yet ready.");
                }

                _log.Info("Connected to SignalR...");

                var (terminationTask, _) = simpleStream.Via(Flow.Create<CommandResponse>()
                        .Collect(x => !string.IsNullOrEmpty(x.Msg), response => response.Msg)
                        .Select(x => Signals.Send(_connectionId, x)))
                    .ToMaterialized(_streamingInterface.Outbound, Keep.Both)
                    .Run(Materializer);

                _log.Info("Running!");

                var self = Context.Self;
                _ = ExecuteLocals();
                return;

                async Task ExecuteLocals()
                {
                    try
                    {
                        await completionTask;
                        self.Tell(PoisonPill.Instance);
                    }
                    catch (Exception ex)
                    {
                        _log.Warning(ex, "Termination task failed.");
                    }
                }
            });

            Receive<Status.Failure>(f =>
            {
                _log.Warning(f.Cause, "Failed to connect to Petabridge.Cmd. Restarting.");
                throw new InvalidOperationException("Need Petabridge.Cmd connection!");
            });
            
            Receive<BeginMonitor>(m =>
            {
                // do nothing
            });
            
            Receive<StopMonitor>(m =>
            {
                _log.Info("Stopping monitoring for connection [{0}]", m.ConnectionId);
                _switch?.Shutdown();
            });
        }

        protected override void PreStart()
        {
            _log.Info("Connecting to Petabridge.Cmd...");

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var pbm = PetabridgeCmd.Get(Context.System);
            pbm.StartLocalClient(cts.Token).PipeTo(Self);
        }

        protected override void PostStop()
        {
            _log.Info("Terminated");
            // shutdown the stream
            _switch.Shutdown();
            
            base.PostStop();
        }
    }
}