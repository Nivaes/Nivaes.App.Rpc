using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace Nivaes.App.Rpc.Client.RpcSyncData
{
    internal class RpcSyncDataSignal
    {
        private readonly Channel<bool> _channel = Channel.CreateUnbounded<bool>();

        public void Signal()
        {
            _channel.Writer.TryWrite(true);
        }

        public ValueTask<bool> WaitAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
