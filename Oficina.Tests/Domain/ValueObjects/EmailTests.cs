
using Oficina.Domain.ValueObjects;
using Xunit;

namespace Oficina.Tests.Domain.ValueObjects
{
    public class EmailTests
    {
        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user@domain.com")]
        [InlineData("name.surname@company.org")]
        public void Criar_DeveRetornarEmailValido(string email)
        {
            var resultado = Email.Criar(email);
            Assert.Equal(email, resultado.Endereco);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("user@")]
        [InlineData("@domain.com")]
        [InlineData("user@.com")]
        public void Criar_DeveLancarExcecaoParaEmailInvalido(string email)
        {
            Assert.Throws<ArgumentException>(() => Email.Criar(email));
        }

    }
}