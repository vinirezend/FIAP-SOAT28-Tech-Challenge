using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oficina.Tests.Application.Services
{
    public class ClientService
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly ClienteService _service;

        public ClientService()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();

            _service = new ClienteService(
                _clienteRepositoryMock.Object);
        }

        private static Cliente CriarClienteValido()
        {
            return Cliente.Criar(
                "Cliente Teste",
                Documento.Criar("52998224725"),
                Email.Criar("cliente@teste.com"),
                Telefone.Criar("19999999999"));
        }

        private static ClienteDTO CriarClienteDTO()
        {
            return new ClienteDTO
            {
                Nome = "Cliente Teste",
                Documento = "52998224725",
                Email = "cliente@teste.com",
                Telefone = "19999999999"
            };
        }

        [Fact]
        public async Task AdicionarAsync_ClienteValido_DeveRetornarDTO()
        {
            // Arrange
            var clienteDTO = CriarClienteDTO();

            _clienteRepositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Cliente>()))
                .ReturnsAsync((Cliente cliente) => cliente);

            // Act
            var resultado = await _service.AdicionarAsync(clienteDTO);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(clienteDTO.Nome, resultado.Nome);
            Assert.Equal(clienteDTO.Documento, resultado.Documento);
            Assert.Equal(clienteDTO.Email, resultado.Email);
            Assert.Equal(clienteDTO.Telefone, resultado.Telefone);

            _clienteRepositoryMock.Verify(
                r => r.AdicionarAsync(It.IsAny<Cliente>()),
                Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_ClienteInexistente_DeveRetornarNull()
        {
            // Arrange
            var clienteDTO = CriarClienteDTO();
            clienteDTO.IdCliente = 1;

            _clienteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(clienteDTO.IdCliente))
                .ReturnsAsync((Cliente?)null);

            // Act
            var resultado = await _service.AtualizarAsync(clienteDTO);

            // Assert
            Assert.Null(resultado);

            _clienteRepositoryMock.Verify(
                r => r.AtualizarAsync(It.IsAny<Cliente>()),
                Times.Never);
        }

        [Fact]
        public async Task AtualizarAsync_ClienteExistente_DeveAtualizarERetornarDTO()
        {
            // Arrange
            var cliente = CriarClienteValido();

            var clienteDTO = new ClienteDTO
            {
                IdCliente = cliente.IdCliente,
                Nome = "Nome Atualizado",
                Documento = "52998224725",
                Email = "atualizado@teste.com",
                Telefone = "19988887777"
            };

            _clienteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(cliente.IdCliente))
                .ReturnsAsync(cliente);

            _clienteRepositoryMock
                .Setup(r => r.AtualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.AtualizarAsync(clienteDTO);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Nome Atualizado", resultado.Nome);
            Assert.Equal("atualizado@teste.com", resultado.Email);
            Assert.Equal("19988887777", resultado.Telefone);

            _clienteRepositoryMock.Verify(
                r => r.AtualizarAsync(cliente),
                Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_ClienteInexistente_DeveRetornarNull()
        {
            // Arrange
            _clienteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Cliente?)null);

            // Act
            var resultado = await _service.ObterPorIdAsync(1);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_ClienteExistente_DeveRetornarDTO()
        {
            // Arrange
            var cliente = CriarClienteValido();

            _clienteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(cliente.IdCliente))
                .ReturnsAsync(cliente);

            // Act
            var resultado = await _service.ObterPorIdAsync(cliente.IdCliente);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(cliente.Nome, resultado.Nome);
            Assert.Equal(cliente.Documento.Numero, resultado.Documento);
        }

        [Fact]
        public async Task ObterPorDocumentoAsync_ClienteInexistente_DeveRetornarNull()
        {
            // Arrange
            const string documento = "52998224725";

            _clienteRepositoryMock
                .Setup(r => r.ObterPorDocumentoAsync(documento))
                .ReturnsAsync((Cliente?)null);

            // Act
            var resultado = await _service.ObterPorDocumentoAsync(documento);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorDocumentoAsync_ClienteExistente_DeveRetornarDTO()
        {
            // Arrange
            var cliente = CriarClienteValido();

            _clienteRepositoryMock
                .Setup(r => r.ObterPorDocumentoAsync(cliente.Documento.Numero))
                .ReturnsAsync(cliente);

            // Act
            var resultado = await _service.ObterPorDocumentoAsync(
                cliente.Documento.Numero);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(cliente.Documento.Numero, resultado.Documento);
            Assert.Equal(cliente.Nome, resultado.Nome);
        }

        [Fact]
        public async Task ObterTodosAsync_ComClientes_DeveRetornarListaDeDTOs()
        {
            // Arrange
            var cliente = CriarClienteValido();

            _clienteRepositoryMock
                .Setup(r => r.ObterTodosAsync())
                .ReturnsAsync(new List<Cliente> { cliente });

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            Assert.Single(resultado);
            Assert.Equal(cliente.Nome, resultado[0].Nome);
            Assert.Equal(cliente.Documento.Numero, resultado[0].Documento);
        }

        [Fact]
        public async Task ObterTodosAsync_SemClientes_DeveRetornarListaVazia()
        {
            // Arrange
            _clienteRepositoryMock
                .Setup(r => r.ObterTodosAsync())
                .ReturnsAsync(new List<Cliente>());

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task RemoverAsync_DeveChamarRepositorioUmaVez()
        {
            // Arrange
            const int idCliente = 1;

            _clienteRepositoryMock
                .Setup(r => r.RemoverAsync(idCliente))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RemoverAsync(idCliente);

            // Assert
            _clienteRepositoryMock.Verify(
                r => r.RemoverAsync(idCliente),
                Times.Once);
        }
    }
}
