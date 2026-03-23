using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Application.Clients.Imports.Models;

namespace Application.Clients.Imports.Queue;

public class ClientImportQueue : IClientImportQueue
{
    private readonly Channel<string> _channel;

    public ClientImportQueue(IOptions<ClientImportOptions> options)
    {
        var queueCapacity = Math.Max(1, options.Value.QueueCapacity);
        _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(queueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask QueueAsync(string filePath, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(filePath, cancellationToken);
    }

    public ValueTask<string> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
