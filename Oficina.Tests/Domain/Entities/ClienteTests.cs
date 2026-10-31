using Xunit;
using Oficina.Domain.Entities;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.Entities
{
    public class ClienteTests
    {
        [Theory]
        [InlineData("Thiago Zoldan", "teste@teste.com", "40831018810", "11944993399")]
        [InlineData("Adalberto LTDA", "outro@teste.com", "5J.8BW.BXK/0001-54", "11944993399")]
        public void CriarCliente_DeveRetornarClienteValido(string nome, string email, string cpfV, string telefone)
        {
            var emailObj = Email.Criar(email);
            var documentoObj = Documento.Criar(cpfV);
            var telefoneObj = Telefone.Criar(telefone);

            var resultado = Cliente.Criar(nome, documentoObj, emailObj, telefoneObj);
            Assert.NotNull(resultado);
        }

        [Theory]
        [InlineData("", "teste@teste.com", "40831018810", "11944993399")]
        [InlineData(null, "outro@teste.com", "5J.8BW.BXK/0001-54", "11944993399")]
        public void CriarCliente_ComNomeInvalido_DeveLancarExcecao(string? nome, string email, string cpfV, string telefone)
        {
            var emailObj = Email.Criar(email);
            var documentoObj = Documento.Criar(cpfV);
            var telefoneObj = Telefone.Criar(telefone);

            Assert.Throws<ArgumentException>(() => Cliente.Criar(nome!, documentoObj, emailObj, telefoneObj));
        }

        [Theory]
        [InlineData("Thiago Zoldan", "teste@teste.com", "40831018810", "11944993399")]
        [InlineData("Adalberto LTDA", "outro@teste.com", "5J.8BW.BXK/0001-54", "11944993399")]
        public void ReconstituirCliente_DeveRetornarClienteValido(string nome, string email, string cpfV, string telefone)
        {
            var emailObj = Email.Criar(email);
            var documentoObj = Documento.Criar(cpfV);
            var telefoneObj = Telefone.Criar(telefone);

            var resultado = Cliente.Reconstituir(1, nome, documentoObj, emailObj, telefoneObj);
            Assert.NotNull(resultado);
        }

        [Theory]
        [InlineData("", "teste@teste.com", "40831018810", "11944993399")]
        [InlineData(null, "outro@teste.com", "5J.8BW.BXK/0001-54", "11944993399")]
        public void ReconstituirCliente_ComNomeInvalido_DeveLancarExcecao(string? nome, string email, string cpfV, string telefone)
        {
            var emailObj = Email.Criar(email);
            var documentoObj = Documento.Criar(cpfV);
            var telefoneObj = Telefone.Criar(telefone);

            Assert.Throws<ArgumentException>(() => Cliente.Reconstituir(1, nome!, documentoObj, emailObj, telefoneObj));
        }
        [Theory]
        [InlineData("", "teste@teste.com", "40831018810", "11944993399")]
        [InlineData(null, "outro@teste.com", "5J.8BW.BXK/0001-54", "11944993399")]
        public void AtualizarCliente_ComNomeInvalido_DeveLancarExcecao(string? nome, string email, string cpfV, string telefone)
        {
            var emailObj = Email.Criar(email);
            var documentoObj = Documento.Criar(cpfV);
            var telefoneObj = Telefone.Criar(telefone);

            var cliente = Cliente.Criar("Nome Valido", documentoObj, emailObj, telefoneObj);

            Assert.Throws<ArgumentException>(() => cliente.Atualizar(nome!, emailObj, telefoneObj));
        }

        [Theory]
        [InlineData("Nome Valido", "teste@teste.com", "40831018810", "11944993399")]
        [InlineData("Outro Nome Valido", "outro@teste.com", "5J.8BW.BXK/0001-54", "11944993399")]
        public void AtualizarCliente_DeveAtualizarCorretamente(string nome, string email, string cpfV, string telefone)
        {
            var emailObj = Email.Criar(email);
            var documentoObj = Documento.Criar(cpfV);
            var telefoneObj = Telefone.Criar(telefone);

            var cliente = Cliente.Criar("Nome Inicial", documentoObj, emailObj, telefoneObj);
            cliente.Atualizar(nome, emailObj, telefoneObj);

            Assert.Equal(nome, cliente.Nome);
            Assert.Equal(emailObj, cliente.Email);
            Assert.Equal(telefoneObj, cliente.Telefone);
        }

    }
}