using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IEnvioLinkAssinadoService
    {
        Task<LinkAssinadoDTO> EnviarAsync(EnvioLinkAssinadoDTO envio, CancellationToken cancellationToken = default);
    }
}
