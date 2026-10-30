namespace Oficina.Domain.Entitys
{
    public class OrdemServicoServico
    {
        public int IdOsServico { get; private set; }
        public int IdOs { get; private set; }
        public int IdServico { get; private set; }
        public int Quantidade { get; private set; }
        public decimal ValorCobrado { get; private set; }
        public decimal ValorTotal => Quantidade * ValorCobrado;

        private OrdemServicoServico() { }
        private static int ValidarQuantidade(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.");
            return quantidade;
        }

        public static OrdemServicoServico Criar(int IdOs, int IdServico, int quantidade, decimal valorCobrado)
        {
            return new OrdemServicoServico
            {
                IdOs = IdOs,
                IdServico = IdServico,
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
        public static OrdemServicoServico Reconstituir(int idOsServico, int idOs, int idServico, int quantidade, decimal valorCobrado) =>
        new() { IdOsServico = idOsServico, IdOs = idOs, IdServico = idServico, Quantidade = ValidarQuantidade(quantidade), ValorCobrado = valorCobrado };
    }
}