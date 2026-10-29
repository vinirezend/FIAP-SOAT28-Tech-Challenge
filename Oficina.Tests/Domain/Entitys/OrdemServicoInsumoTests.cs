using Oficina.Domain.Entitys;

namespace Oficina.Tests.Domain.Entitys
{
    public class OrdemServicoInsumoTests
    {
        [Fact]
        public void Criar_DeveCalcularValorTotal()
        {
            var item = OrdemServicoInsumo.Criar(1, 7, 3, 50m);

            Assert.Equal(1, item.IdOs);
            Assert.Equal(7, item.IdInsumo);
            Assert.Equal(3, item.Quantidade);
            Assert.Equal(50m, item.ValorCobrado);
            Assert.Equal(150m, item.ValorTotal);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Criar_ComQuantidadeInvalida_DeveLancarExcecao(int quantidade)
        {
            Assert.Throws<ArgumentException>(() => OrdemServicoInsumo.Criar(1, 7, quantidade, 50m));
        }

        [Fact]
        public void AtualizarQuantidade_DeveRecalcularValorTotal()
        {
            var item = OrdemServicoInsumo.Criar(1, 7, 2, 10m);

            item.AtualizarQuantidade(5);

            Assert.Equal(5, item.Quantidade);
            Assert.Equal(50m, item.ValorTotal);
        }

        [Fact]
        public void AtualizarQuantidade_ComValorInvalido_DeveLancarExcecao()
        {
            var item = OrdemServicoInsumo.Criar(1, 7, 2, 10m);

            Assert.Throws<ArgumentException>(() => item.AtualizarQuantidade(0));
        }

        [Fact]
        public void AtualizarValorCobrado_DeveRecalcularValorTotal()
        {
            var item = OrdemServicoInsumo.Criar(1, 7, 3, 10m);

            item.AtualizarValorCobrado(20m);

            Assert.Equal(20m, item.ValorCobrado);
            Assert.Equal(60m, item.ValorTotal);
        }

        [Fact]
        public void Reconstituir_DeveMontarEntidadeComId()
        {
            var item = OrdemServicoInsumo.Reconstituir(99, 1, 7, 3, 10m);

            Assert.Equal(99, item.IdOsInsumo);
            Assert.Equal(30m, item.ValorTotal);
        }
    }
}
