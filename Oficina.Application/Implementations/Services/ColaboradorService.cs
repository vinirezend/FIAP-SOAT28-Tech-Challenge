using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Implementations.Services
{
    public class ColaboradorService : IColaboradorService
    {
        private readonly IColaboradorRepository _colaboradorRepository;

        public ColaboradorService(IColaboradorRepository colaboradorRepository)
        {
            _colaboradorRepository = colaboradorRepository;
        }

        public async Task<ColaboradorDTO> AdicionarAsync(ColaboradorDTO colaborador, CancellationToken cancellationToken = default)
        {
            var colaboradorEntity = Colaborador.Criar(
                colaborador.Nome,
                Documento.Criar(colaborador.Cpf),
                Email.Criar(colaborador.Email),
                (Departamento)colaborador.IdDepartamento,
                colaborador.IdUsuario
            );
            colaboradorEntity = await _colaboradorRepository.AdicionarAsync(colaboradorEntity, cancellationToken);
            return TransformaParaDTO(colaboradorEntity);
        }

        public async Task<ColaboradorDTO?> AtualizarAsync(ColaboradorDTO colaborador, CancellationToken cancellationToken = default)
        {
            var colaboradorEntity = await _colaboradorRepository.ObterPorIdAsync(colaborador.IdColaborador, cancellationToken);
            if (colaboradorEntity is null) return null;

            colaboradorEntity.Atualizar(
                colaborador.Nome,
                Email.Criar(colaborador.Email),
                (Departamento)colaborador.IdDepartamento,
                colaborador.IdUsuario,
                colaborador.Ativo
            );

            await _colaboradorRepository.AtualizarAsync(colaboradorEntity, cancellationToken);
            return TransformaParaDTO(colaboradorEntity);
        }

        public async Task<ColaboradorDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var colaborador = await _colaboradorRepository.ObterPorIdAsync(id, cancellationToken);
            return colaborador is null ? null : TransformaParaDTO(colaborador);
        }

        public async Task<IReadOnlyList<ColaboradorDTO>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var colaboradores = await _colaboradorRepository.ObterTodosAsync(cancellationToken);
            return colaboradores.Select(TransformaParaDTO).ToList();
        }

        public Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            return _colaboradorRepository.RemoverAsync(id, cancellationToken);
        }

        private static ColaboradorDTO TransformaParaDTO(Colaborador colaborador)
        {
            return new ColaboradorDTO
            {
                IdColaborador = colaborador.IdColaborador,
                Nome = colaborador.Nome,
                Cpf = colaborador.Cpf.Numero,
                Email = colaborador.Email.Endereco,
                Ativo = colaborador.Ativo,
                IdDepartamento = (int)colaborador.IdDepartamento,
                IdUsuario = colaborador.IdUsuario
            };
        }
    }
}
