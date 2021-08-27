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
    public sealed class BeginMonitor
    {
        public BeginMonitor(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }
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

                var stream = simpleStream.Via(Flow.Create<CommandResponse>()
                        .Collect(x => !string.IsNullOrEmpty(x.Msg), response => response.Msg)
                        .Select(x => Signals.Send(_connectionId, x)))
                    .RunWith(_streamingInterface.Outbound, Materializer);

                _log.Info("Running!");

                var self = Context.Self;
#pragma warning disable 4014
                completionTask.ContinueWith(tr => new Status.Failure(new ApplicationException("Upstream terminated.")))
                    .PipeTo(self);
#pragma warning restore 4014
            });

            Receive<Status.Failure>(f =>
            {
                _log.Warning(f.Cause, "Failed to connect to Petabridge.Cmd. Restarting.");
                throw new InvalidOperationException("Need Petabridge.Cmd connection!");
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
            // shutdown the stream
            _switch.Shutdown();
            
            base.PostStop();
        }
    }
}