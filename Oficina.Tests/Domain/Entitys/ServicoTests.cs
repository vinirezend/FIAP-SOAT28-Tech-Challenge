using Xunit;
using Oficina.Domain.Entitys;

namespace Oficina.Tests.Domain.Entitys
{
    public class ServicoTests
    {
       [Theory]
       [InlineData("Servico 1", "Descricao 1", 100.0)]
       [InlineData("Servico 2", "Descricao 2", 200.0)] 
       public void CriarServico_DeveRetornarServicoValido(string nome, string descricao, decimal valor)
       {

           // Act
           var resultado = Servico.Criar(nome, descricao, valor);

           // Assert
           Assert.NotNull(resultado);
       }

       [Theory]
       [InlineData("", "Descricao 1", 100.0)]
       [InlineData(null, "Descricao 2", 200.0)]
       public void CriarServico_ComNomeInvalido_DeveLancarExcecao(string nome, string descricao, decimal valor)
       {
           Assert.Throws<ArgumentException>(() => Servico.Criar(nome, descricao, valor));
       }
       
       [Theory]
       [InlineData("Servico 1", "Descricao 1", -100.0)]
       [InlineData("Servico 2", "Descricao 2", 0)]
       public void CriarServico_ComValoresInvalidos_DeveLancarExcecao(string nome, string descricao, decimal valor)
       {
           Assert.Throws<ArgumentException>(() => Servico.Criar(nome, descricao, valor));
       }
    }
}