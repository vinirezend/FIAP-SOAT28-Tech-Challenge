using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Application.Implementations.Services
{
    public class StatusService : IStatusService
    {
        private readonly IStatusRepository _statusRepository;

        public StatusService(IStatusRepository statusRepository)
        {
            _statusRepository = statusRepository;
        }

        public async Task<StatusDTO?> ObterPorIdAsync(int id)
        {
            var status = await _statusRepository.ObterPorIdAsync(id);
            return status is null ? null : TransformaParaDTO(status);
        }

        public async Task<IReadOnlyList<StatusDTO>> ObterTodosAsync()
        {
            var status = await _statusRepository.ObterTodosAsync();
            return status.Select(TransformaParaDTO).ToList();
        }

        private static StatusDTO TransformaParaDTO(Status status)
        {
            return new StatusDTO
            {
                IdStatus = status.IdStatus,
                Nome = status.Nome
            };
        }
    }
}
