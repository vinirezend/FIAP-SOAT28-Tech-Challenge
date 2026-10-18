namespace Oficina.Domain.Entitys
{
    public class Insumo
    {
        public int IdInsumo { get; private set; }
        public string Nome { get; private set; } = default!;
        public string Descricao { get; private set; } = default!;
        public decimal ValorUnitario { get; private set; } = default!;
        public bool Ativo { get; private set; } = default!;
        public int QuantidadeEstoque { get; private set; } = default!;
        public int QuantidadeReservada { get; private set; } = default!;
        

        private Insumo() { }

        public static Insumo Criar(string nome, string descricao, decimal valorUnitario, int quantidadeEstoque)
        {
            return new Insumo
            {
                Nome = nome,
                Descricao = descricao,
                ValorUnitario = valorUnitario,
                Ativo = true,
                QuantidadeEstoque = quantidadeEstoque,
                QuantidadeReservada = 0
            };
        }

        public void Atualizar(string nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada)
        {
            Nome = nome;
            Descricao = descricao;
            ValorUnitario = valorUnitario  ;
            Ativo = ativo;
            QuantidadeEstoque = quantidadeEstoque;
            QuantidadeReservada = quantidadeReservada;
        }

        public static Insumo Reconstituir(int idInsumo, string nome, string descricao, decimal valorUnitario, bool ativo, int quantidadeEstoque, int quantidadeReservada) =>
        new() { IdInsumo = idInsumo, Nome = nome, Descricao = descricao, ValorUnitario = valorUnitario, Ativo = ativo, QuantidadeEstoque = quantidadeEstoque, QuantidadeReservada = quantidadeReservada };
    }
}