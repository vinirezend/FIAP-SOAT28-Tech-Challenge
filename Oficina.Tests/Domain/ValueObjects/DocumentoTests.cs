using Oficina.Domain.ValueObjects;
using Xunit;

namespace Oficina.Tests.Domain.ValueObjects
{
    public class DocumentoTests
    {
        [Theory]
        [InlineData("418.055.700-21", TipoDocumento.Cpf)]
        [InlineData("48681404008", TipoDocumento.Cpf)]
        [InlineData("14.640.091/0001-34", TipoDocumento.Cnpj)]
        [InlineData("14640091000134", TipoDocumento.Cnpj)]
        [InlineData("5J.8BW.BXK/0001-54", TipoDocumento.Cnpj)]
        public void Criar_DeveRetornarDocumentoValido(string numero, TipoDocumento tipo)
        {

            var resultado = Documento.Criar(numero);
            var numeroSemMascara = new string(numero.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            Assert.Equal(numeroSemMascara, resultado.Numero);
            Assert.Equal(tipo, resultado.Tipo);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-document")]
        [InlineData("123")]
        public void Criar_DeveLancarExcecaoParaDocumentoInvalido(string numero)
        {
            Assert.Throws<ArgumentException>(() => Documento.Criar(numero));
        }
        
    }
}