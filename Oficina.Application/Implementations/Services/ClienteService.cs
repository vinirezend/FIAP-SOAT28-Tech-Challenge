using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.Entitys;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Application.Implementations.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IEmailSender _emailSender;

        public ClienteService(IClienteRepository clienteRepository, IEmailSender emailSender)
        {
            _clienteRepository = clienteRepository;
            _emailSender = emailSender;
        }
        public async Task<ClienteDTO> AdicionarAsync(ClienteDTO cliente)
        {
            var clienteEntity = Cliente.Criar(
                cliente.Nome,
                Documento.Criar(cliente.Documento),
                Email.Criar(cliente.Email),
                Telefone.Criar(cliente.Telefone)
            );
            clienteEntity = await _clienteRepository.AdicionarAsync(clienteEntity);
            return TransformaParaDTO(clienteEntity);
        }
        public async Task<ClienteDTO?> AtualizarAsync(ClienteDTO cliente)
        {
            var clienteRecord = await _clienteRepository.ObterPorIdAsync(cliente.IdCliente);
            if (clienteRecord is null)
                return null;
            clienteRecord.Atualizar(
                cliente.Nome,
                Email.Criar(cliente.Email),
                Telefone.Criar(cliente.Telefone)
            );
            await _clienteRepository.AtualizarAsync(clienteRecord);
            return TransformaParaDTO(clienteRecord);
        }
        public async Task<ClienteDTO?> ObterPorIdAsync(int id)
        {
            var clienteEntity = await _clienteRepository.ObterPorIdAsync(id);

            var email = new EmailDTO
            {
                Assunto = "Primeiro Teste",
                CorpoHtml = "Teste qlqr coisa aaaaaaaaaaaaaaa",
                //TODO alterar email que deseja testar
                EnderecoDestinatario = clienteEntity?.Email?.Endereco ?? "",
            };

            await _emailSender.Enviar(email);

            return clienteEntity is null ? null : TransformaParaDTO(clienteEntity);
        }
        public async Task<ClienteDTO?> ObterPorDocumentoAsync(string documento)
        {
            var clienteEntity = await _clienteRepository.ObterPorDocumentoAsync(documento);
            return clienteEntity is null ? null : TransformaParaDTO(clienteEntity);
        }
        public async Task<IReadOnlyList<ClienteDTO>> ObterTodosAsync()
        {
            var clientes = await _clienteRepository.ObterTodosAsync();
            return clientes.Select(TransformaParaDTO).ToList();
        }
        public Task RemoverAsync(int id)
        {
            return _clienteRepository.RemoverAsync(id);
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
}