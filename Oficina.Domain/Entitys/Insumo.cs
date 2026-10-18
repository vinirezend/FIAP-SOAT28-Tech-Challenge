namespace Oficina.Domain.Entitys
{
    public class Insumo
    {
        public int IdInsumo { get; private set; }
        public string Nome { get; private set; } = default!;
        public string Descricao { get; private set; } = default!;
        public decimal ValorUnitario { get; private set; }
        public bool Ativo { get; private set; } 
        public int QuantidadeEstoque { get; private set; }
        public int QuantidadeReservada { get; private set; }
        

        private Insumo() { }

        public static Insumo Criar(string nome, string descricao, decimal valorUnitario, int quantidadeEstoque)
        {
            return new Insumo
            {
                Nome = ValidarNome(nome),
                Descricao = descricao,
                ValorUnitario = ValidarValorUnitario(valorUnitario),
                Ativo = true,
                QuantidadeEstoque = ValidarQuantidade(quantidadeEstoque),
                QuantidadeReservada = 0
            };
        }

        public void Atualizar(string nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada)
        {
            Nome = ValidarNome(nome);
            Descricao = descricao;
            ValorUnitario = ValidarValorUnitario(valorUnitario);
            Ativo = ativo;
            QuantidadeEstoque = ValidarQuantidade(quantidadeEstoque);
            QuantidadeReservada = ValidarQuantidade(quantidadeReservada);
        }
        
        private static string ValidarNome(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"Nome do insumo é obrigatório.");

            return valor.Trim();
        }
        private static decimal ValidarValorUnitario(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException($"Valor unitário do insumo deve ser maior que zero.");

            return valor;
        }
        private static int ValidarQuantidade(int valor)
        {
            if (valor < 0)
                throw new ArgumentException($"Quantidade do insumo não pode ser negativa.");

            return valor;
        }

        public static Insumo Reconstituir(int idInsumo, string nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada) =>
        new() { IdInsumo = idInsumo, Nome = ValidarNome(nome), Descricao = descricao, ValorUnitario = ValidarValorUnitario(valorUnitario), Ativo = ativo, QuantidadeEstoque = ValidarQuantidade(quantidadeEstoque), QuantidadeReservada = ValidarQuantidade(quantidadeReservada) };
    }
}