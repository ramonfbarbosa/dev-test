using Application.Common.Interfaces;
using Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Tests.Support;

internal sealed class RecordingEmailProvider : IEmailProvider
{
    private readonly Exception? _exceptionToThrow;

    public RecordingEmailProvider(Exception? exceptionToThrow = null)
    {
        _exceptionToThrow = exceptionToThrow;
    }

    public List<EmailMessage> SentMessages { get; } = [];

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (_exceptionToThrow is not null)
        {
            throw _exceptionToThrow;
        }

        SentMessages.Add(message);
        return Task.CompletedTask;
    }
}
