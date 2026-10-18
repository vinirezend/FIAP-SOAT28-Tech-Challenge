using Xunit;
using Oficina.Domain.Entitys;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.Entitys
{
    public class VeiculoTests
    {
        [Theory]
        [InlineData(1, "ABC1234", "Modelo 1", "Marca 1", 2020)]
        [InlineData(2, "XYZ5678", "Modelo 2", "Marca 2", 2021)]
        public void CriarVeiculo_DeveRetornarVeiculoValido(int idCliente, string placa, string modelo, string marca, int ano)
        {
            var placaObj = Placa.Criar(placa);

            var resultado = Veiculo.Criar(idCliente, placaObj, modelo, marca, ano);

            Assert.NotNull(resultado);
            Assert.Equal(idCliente, resultado.IdCliente);
            Assert.Equal(placaObj, resultado.Placa);
            Assert.Equal(modelo, resultado.Modelo);
            Assert.Equal(marca, resultado.Marca);
            Assert.Equal(ano, resultado.Ano);
        }

        [Theory]
        [InlineData("", "Marca 1", 2020)]
        [InlineData(null, "Marca 2", 2021)]
        public void CriarVeiculo_ComModeloInvalido_DeveLancarExcecao(string modelo, string marca, int ano)
        {
            var placaObj = Placa.Criar("ABC1234");

            Assert.Throws<ArgumentException>(() => Veiculo.Criar(1, placaObj, modelo, marca, ano));
        }

        [Theory]
        [InlineData("Modelo 1", "", 2020)]
        [InlineData("Modelo 2", null, 2021)]
        public void CriarVeiculo_ComMarcaInvalida_DeveLancarExcecao(string modelo, string marca, int ano)
        {
            var placaObj = Placa.Criar("ABC1234");

            Assert.Throws<ArgumentException>(() => Veiculo.Criar(1, placaObj, modelo, marca, ano));
        }

        [Theory]
        [InlineData("Modelo 1", "Marca 1", 0)]
        [InlineData("Modelo 2", "Marca 2", -2021)]
        public void CriarVeiculo_ComAnoInvalido_DeveLancarExcecao(string modelo, string marca, int ano)
        {
            var placaObj = Placa.Criar("ABC1234");

            Assert.Throws<ArgumentException>(() => Veiculo.Criar(1, placaObj, modelo, marca, ano));
        }

        [Fact]
        public void CriarVeiculo_ComPlacaNula_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => Veiculo.Criar(1, null!, "Modelo 1", "Marca 1", 2020));
        }

        [Theory]
        [InlineData(1, "ABC1234", "Modelo 1", "Marca 1", 2020)]
        [InlineData(2, "XYZ5678", "Modelo 2", "Marca 2", 2021)]
        public void AtualizarVeiculo_DeveAtualizarCorretamente(int idCliente, string placa, string modelo, string marca, int ano)
        {
            var placaInicial = Placa.Criar("ABC1234");
            var veiculo = Veiculo.Criar(1, placaInicial, "Modelo Inicial", "Marca Inicial", 2010);

            var placaObj = Placa.Criar(placa);
            veiculo.Atualizar(idCliente, placaObj, modelo, marca, ano);

            Assert.Equal(idCliente, veiculo.IdCliente);
            Assert.Equal(placaObj, veiculo.Placa);
            Assert.Equal(modelo, veiculo.Modelo);
            Assert.Equal(marca, veiculo.Marca);
            Assert.Equal(ano, veiculo.Ano);
        }

        [Theory]
        [InlineData("", "Marca 1", 2020)]
        [InlineData(null, "Marca 2", 2021)]
        public void AtualizarVeiculo_ComModeloInvalido_DeveLancarExcecao(string modelo, string marca, int ano)
        {
            var placaObj = Placa.Criar("ABC1234");
            var veiculo = Veiculo.Criar(1, placaObj, "Modelo Inicial", "Marca Inicial", 2010);

            Assert.Throws<ArgumentException>(() => veiculo.Atualizar(1, placaObj, modelo, marca, ano));
        }

        [Theory]
        [InlineData(1, 1, "ABC1234", "Modelo 1", "Marca 1", 2020)]
        [InlineData(2, 2, "XYZ5678", "Modelo 2", "Marca 2", 2021)]
        public void ReconstituirVeiculo_DeveRetornarVeiculoValido(int idVeiculo, int idCliente, string placa, string modelo, string marca, int ano)
        {
            var placaObj = Placa.Criar(placa);

            var resultado = Veiculo.Reconstituir(idVeiculo, idCliente, placaObj, modelo, marca, ano);

            Assert.NotNull(resultado);
            Assert.Equal(idVeiculo, resultado.IdVeiculo);
            Assert.Equal(idCliente, resultado.IdCliente);
            Assert.Equal(placaObj, resultado.Placa);
            Assert.Equal(modelo, resultado.Modelo);
            Assert.Equal(marca, resultado.Marca);
            Assert.Equal(ano, resultado.Ano);
        }

        [Theory]
        [InlineData(1, "", "Marca 1", 2020)]
        [InlineData(2, null, "Marca 2", 2021)]
        public void ReconstituirVeiculo_ComModeloInvalido_DeveLancarExcecao(int idVeiculo, string modelo, string marca, int ano)
        {
            var placaObj = Placa.Criar("ABC1234");

            Assert.Throws<ArgumentException>(() => Veiculo.Reconstituir(idVeiculo, 1, placaObj, modelo, marca, ano));
        }
    }
}
