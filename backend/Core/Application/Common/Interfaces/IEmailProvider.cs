using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IEmailProvider
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
