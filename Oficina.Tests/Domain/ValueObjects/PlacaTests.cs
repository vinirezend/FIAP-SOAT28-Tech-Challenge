using System;
using Oficina.Domain.ValueObjects;
using Xunit;

namespace Oficina.Tests.Domain.ValueObjects
{
    public class PlacaTests
    {
        [Theory]
        [InlineData("ABC1234")]
        [InlineData("XYZ9876")]
        public void Criar_DeveRetornarPlacaValida(string placa)
        {
            var resultado = Placa.Criar(placa);
            Assert.Equal(placa, resultado.Numero);
        }
    }
}