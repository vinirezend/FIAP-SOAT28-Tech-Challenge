using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Resources;

public interface IEmailSender
{
    public Task EnviarAsync(EmailDTO email, CancellationToken cancellationToken);
}
