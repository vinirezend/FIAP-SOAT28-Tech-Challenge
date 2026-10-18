namespace Oficina.Domain.Entitys
{
    public class Servico
    {
        public int IdServico { get; private set; }
        public string Nome { get; private set; } = default!;
        public string Descricao { get; private set; } = default!;
        public decimal ValorEstipulado { get; private set; }
        public bool Ativo { get; private set; }

        private Servico() { }

        public static Servico Criar(string Nome, string Descricao, decimal ValorEstipulado)
        {
            return new Servico
            {
                Nome = ValidarNome(Nome),
                Descricao = Descricao,
                ValorEstipulado = ValidarValorEstipulado(ValorEstipulado),
                Ativo = true
            };
        }
        public void Atualizar(string nome, string descricao, decimal valorEstipulado, bool ativo)
        {
            Nome = ValidarNome(nome);
            Descricao = descricao;
            ValorEstipulado = ValidarValorEstipulado(valorEstipulado);
            Ativo = ativo;
        }

        private static string ValidarNome(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"Nome do serviço é obrigatório.");

            return valor.Trim();
        }

        private static decimal ValidarValorEstipulado(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException($"Valor estipulado do serviço deve ser maior que zero.");

            return valor;
        }
        public static Servico Reconstituir(int idServico, string nome, string descricao, decimal valorEstipulado, bool ativo) =>
        new() { IdServico = idServico, Nome = ValidarNome(nome), Descricao = descricao, ValorEstipulado = ValidarValorEstipulado(valorEstipulado), Ativo = ativo };
    }
}