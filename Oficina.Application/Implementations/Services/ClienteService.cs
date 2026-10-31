using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Implementations.Services;

public class ClienteService(IClienteRepository clienteRepository) : IClienteService
{
    public async Task<ClienteDTO> AdicionarAsync(ClienteDTO cliente, CancellationToken cancellationToken = default)
    {
        var clienteEntity = Cliente.Criar(
            cliente.Nome,
            Documento.Criar(cliente.Documento),
            Email.Criar(cliente.Email),
            Telefone.Criar(cliente.Telefone)
        );
        clienteEntity = await clienteRepository.AdicionarAsync(clienteEntity, cancellationToken);
        return TransformaParaDTO(clienteEntity);
    }
    public async Task<ClienteDTO?> AtualizarAsync(ClienteDTO cliente, CancellationToken cancellationToken = default)
    {
        var clienteRecord = await clienteRepository.ObterPorIdAsync(cliente.IdCliente, cancellationToken);
        if (clienteRecord is null)
            return null;
        clienteRecord.Atualizar(
            cliente.Nome,
            Email.Criar(cliente.Email),
            Telefone.Criar(cliente.Telefone)
        );
        await clienteRepository.AtualizarAsync(clienteRecord, cancellationToken);
        return TransformaParaDTO(clienteRecord);
    }
    public async Task<ClienteDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var clienteEntity = await clienteRepository.ObterPorIdAsync(id, cancellationToken);
        return clienteEntity is null ? null : TransformaParaDTO(clienteEntity);
    }
    public async Task<ClienteDTO?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        var clienteEntity = await clienteRepository.ObterPorDocumentoAsync(documento, cancellationToken);
        return clienteEntity is null ? null : TransformaParaDTO(clienteEntity);
    }
    public async Task<IReadOnlyList<ClienteDTO>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await clienteRepository.ObterTodosAsync(cancellationToken);
        return clientes.Select(TransformaParaDTO).ToList();
    }
    public Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return clienteRepository.RemoverAsync(id, cancellationToken);
    }
    private static ClienteDTO TransformaParaDTO(Cliente cliente)
    {
        return new ClienteDTO
        {
            IdCliente = cliente.IdCliente,
            Nome = cliente.Nome,
            Documento = cliente.Documento.Numero,
            TipoDocumento = cliente.Documento.Tipo.ToString(),
            Email = cliente.Email.Endereco,
            Telefone = cliente.Telefone.Numero
        };
    }
}