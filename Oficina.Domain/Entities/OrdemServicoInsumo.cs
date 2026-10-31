namespace Oficina.Domain.Entities
{
    public class OrdemServicoInsumo
    {
        public int IdOsInsumo { get; private set; }
        public int IdOs { get; private set; }
        public int IdInsumo { get; private set; }
        public int Quantidade { get; private set; }
        public decimal ValorCobrado { get; private set; }
        public decimal ValorTotal => Quantidade * ValorCobrado;

        private OrdemServicoInsumo() { }
        private static int ValidarQuantidade(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.");
            return quantidade;
        }

        public static OrdemServicoInsumo Criar(int IdOs, int IdInsumo, int quantidade, decimal valorCobrado)
        {
            return new OrdemServicoInsumo
            {
                IdOs = IdOs,
                IdInsumo = IdInsumo,
                Quantidade = ValidarQuantidade(quantidade),
                ValorCobrado = valorCobrado

            };
        }
        public void AtualizarQuantidade(int quantidade)
        {
            Quantidade = ValidarQuantidade(quantidade);
        }
        public void AtualizarValorCobrado(decimal valorCobrado)
        {
            ValorCobrado = valorCobrado;
        }
        public static OrdemServicoInsumo Reconstituir(int idOsInsumo, int idOs, int idInsumo, int quantidade, decimal valorCobrado) =>
        new() { IdOsInsumo = idOsInsumo, IdOs = idOs, IdInsumo = idInsumo, Quantidade = ValidarQuantidade(quantidade), ValorCobrado = valorCobrado };
    }
}