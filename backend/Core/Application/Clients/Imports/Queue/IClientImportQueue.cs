using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Imports.Queue;

public interface IClientImportQueue
{
    ValueTask QueueAsync(string filePath, CancellationToken cancellationToken);
    ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
}
