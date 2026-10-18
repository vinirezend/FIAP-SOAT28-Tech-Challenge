using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Entitys
{
    public class Veiculo
    {
        public int IdVeiculo { get; private set; }
        public int IdCliente { get; private set; }
        public Placa Placa { get; private set; } = default!;
        public string Modelo { get; private set; } = default!;
        public string Marca { get; private set; } = default!;
        public int Ano { get; private set; }

        private Veiculo() { }
        public static Veiculo Criar(int idCliente, Placa placa, string modelo, string marca, int ano)
        {
            ArgumentNullException.ThrowIfNull(placa);

            return new Veiculo
            {
                IdCliente = idCliente,
                Placa = placa,
                Modelo = ValidarTexto(modelo, "Modelo"),
                Marca = ValidarTexto(marca, "Marca"),
                Ano = ValidarAno(ano)
            };
        }
        public void Atualizar(int idCliente,Placa placa, string modelo, string marca, int ano)
        {
            IdCliente = idCliente;
            Placa = placa;
            Modelo = ValidarTexto(modelo, "Modelo");
            Marca = ValidarTexto(marca, "Marca");
            Ano = ValidarAno(ano);
        }

        private static string ValidarTexto(string? valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"{campo} do veículo é obrigatório.");

            return valor.Trim();
        }

        private static int ValidarAno(int ano)
        {
            if (ano <= 0)
                throw new ArgumentException("Ano do veículo deve ser positivo.");

            return ano;
        }
        public static Veiculo Reconstituir(int idVeiculo, int idCliente, Placa placa, string modelo, string marca, int ano) =>
        new() { IdVeiculo = idVeiculo, IdCliente = idCliente, Placa = placa, Modelo = ValidarTexto(modelo, "Modelo"), Marca = ValidarTexto(marca, "Marca"), Ano = ValidarAno(ano) };
    }
}