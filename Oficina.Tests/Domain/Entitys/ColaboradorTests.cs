using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.Entitys
{
    public class ColaboradorTests
    {
        private static readonly Documento Cpf = Documento.Criar("02994706601");
        private static readonly Email Email = Email.Criar("colaborador@oficina.com");

        [Fact]
        public void CriarColaborador_ComDadosValidos_DeveCriarAtivo()
        {
            var resultado = Colaborador.Criar("  Gerente 1 ", Cpf, Email, Departamento.Gerencia, 1);

            Assert.Equal("Gerente 1", resultado.Nome);
            Assert.True(resultado.Ativo);
            Assert.Equal(Departamento.Gerencia, resultado.IdDepartamento);
            Assert.Equal(1, resultado.IdUsuario);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CriarColaborador_ComNomeInvalido_DeveLancarExcecao(string? nome)
        {
            Assert.Throws<ArgumentException>(() => Colaborador.Criar(nome!, Cpf, Email, Departamento.Oficina, null));
        }

        [Fact]
        public void CriarColaborador_ComCnpj_DeveLancarExcecao()
        {
            var cnpj = Documento.Criar("11319526000155");

            Assert.Throws<ArgumentException>(() => Colaborador.Criar("Colaborador", cnpj, Email, Departamento.Oficina, null));
        }

        [Fact]
        public void CriarColaborador_ComDepartamentoInexistente_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => Colaborador.Criar("Colaborador", Cpf, Email, (Departamento)99, null));
        }

        [Fact]
        public void AtualizarColaborador_DeveAlterarDadosEDesativar()
        {
            var colaborador = Colaborador.Criar("Colaborador", Cpf, Email, Departamento.Oficina, null);
            var novoEmail = Email.Criar("novo@oficina.com");

            colaborador.Atualizar("Novo Nome", novoEmail, Departamento.Almoxarifado, 4, false);

            Assert.Equal("Novo Nome", colaborador.Nome);
            Assert.Equal(novoEmail, colaborador.Email);
            Assert.Equal(Departamento.Almoxarifado, colaborador.IdDepartamento);
            Assert.Equal(4, colaborador.IdUsuario);
            Assert.False(colaborador.Ativo);
            Assert.Equal(Cpf, colaborador.Cpf);
        }
    }
}
