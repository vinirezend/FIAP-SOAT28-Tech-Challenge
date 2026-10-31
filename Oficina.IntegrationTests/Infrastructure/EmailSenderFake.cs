using System.Collections.Concurrent;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.IntegrationTests.Infrastructure;

/// <summary>
/// Substitui o envio SMTP: guarda os e-mails em memória para os cenários conferirem destinatário, assunto e links.
/// </summary>
public sealed class EmailSenderFake : IEmailSender
{
    private readonly ConcurrentQueue<EmailDTO> _enviados = new();

    public IReadOnlyList<EmailDTO> Enviados => [.. _enviados];

    public Task EnviarAsync(EmailDTO email, CancellationToken cancellationToken)
    {
        _enviados.Enqueue(email);
        return Task.CompletedTask;
    }

    public void Limpar() => _enviados.Clear();
}
