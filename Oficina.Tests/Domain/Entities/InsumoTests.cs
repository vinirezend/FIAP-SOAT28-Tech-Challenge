using Xunit;
using Oficina.Domain.Entities;

namespace Oficina.Tests.Domain.Entities
{
    public class InsumoTests
    {
        [Theory]
        [InlineData("Insumo 1", "Descricao 1", 10.5, 100)]
        [InlineData("Insumo 2", "Descricao 2", 20.0, 200)]
        public void CriarInsumo_DeveRetornarInsumoValido(string nome, string descricao, decimal valorUnitario, int quantidadeEstoque)
        {
            var resultado = Insumo.Criar(nome, descricao, valorUnitario, quantidadeEstoque);
            Assert.NotNull(resultado);
        }

        [Theory]
        [InlineData("", "Descricao 1", 10.5, 100)]
        [InlineData(null, "Descricao 2", 20.0, 200)]
        public void CriarInsumo_ComNomeInvalido_DeveLancarExcecao(string? nome, string descricao, decimal valorUnitario, int quantidadeEstoque)
        {
            Assert.Throws<ArgumentException>(() => Insumo.Criar(nome!, descricao, valorUnitario, quantidadeEstoque));
        }
        
        [Theory]
        [InlineData("Insumo 1", "Descricao 1", -10.5, 100)]
        [InlineData("Insumo 2", "Descricao 2", 20.0, -200)]
        public void CriarInsumo_ComValoresInvalidos_DeveLancarExcecao(string nome, string descricao, decimal valorUnitario, int quantidadeEstoque)
        {
            Assert.Throws<ArgumentException>(() => Insumo.Criar(nome, descricao, valorUnitario, quantidadeEstoque));
        }

        [Theory]
        [InlineData(1, "Insumo 1", "Descricao 1", 10.5, true, 100, 0)]
        [InlineData(2, "Insumo 2", "Descricao 2", 20.0, false, 200, 50)]
        public void ReconstituirInsumo_DeveRetornarInsumoValido(int id, string nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada)
        {
            var resultado = Insumo.Reconstituir(id, nome, descricao, valorUnitario, ativo, quantidadeEstoque, quantidadeReservada);
            Assert.NotNull(resultado);
        }
        
        [Theory]
        [InlineData(1, "", "Descricao 1", 10.5, true, 100, 0)]
        [InlineData(2, null, "Descricao 2", 20.0, true, 200, 0)]
        public void ReconstituirInsumo_ComNomeInvalido_DeveLancarExcecao(int id, string? nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada)
        {
            Assert.Throws<ArgumentException>(() => Insumo.Reconstituir(id, nome!, descricao, valorUnitario, ativo, quantidadeEstoque, quantidadeReservada));
        }

        [Theory]
        [InlineData(1, "Insumo 1", "Descricao 1", -10.5, true, 100, 0)]
        [InlineData(2, "Insumo 2", "Descricao 2", 20.0, true, -200, 0)]
        public void ReconstituirInsumo_ComValoresInvalidos_DeveLancarExcecao(int id, string nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada)
        {
            Assert.Throws<ArgumentException>(() => Insumo.Reconstituir(id, nome, descricao, valorUnitario, ativo, quantidadeEstoque, quantidadeReservada));
        }

        [Theory]
        [InlineData(1, "Insumo 1", "Descricao 1", 10.5, true, 100, 0)]
        [InlineData(2, "Insumo 2", "Descricao 2", 20.0, false, 200, 50)]
        public void ReconstituirInsumo_ComValoresValidos_DeveRetornarInsumoValido(int id, string nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada)
        {
            var resultado = Insumo.Reconstituir(id, nome, descricao, valorUnitario, ativo, quantidadeEstoque, quantidadeReservada);
            Assert.NotNull(resultado);
        }
    }
}