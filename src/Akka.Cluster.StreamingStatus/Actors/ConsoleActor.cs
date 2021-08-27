using System.Reflection.Emit;
using Akka.Actor;
using Akka.Event;
using Microsoft.AspNetCore.Diagnostics;

namespace Akka.Cluster.StreamingStatus.Actors
{
    public sealed class ConsoleActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public ConsoleActor()
        {
            ReceiveAny(_ => _log.Info("Received: {0}", _));
        }
    }
}