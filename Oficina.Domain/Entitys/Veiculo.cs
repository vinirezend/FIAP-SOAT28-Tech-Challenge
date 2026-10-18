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
        public int Ano { get; private set; } = default!;

        private Veiculo() { }
        public static Veiculo Criar(int idCliente, Placa placa, string modelo, string marca, int ano)
        {
            return new Veiculo
            {
                IdCliente = idCliente,
                Placa = placa,
                Modelo = modelo,
                Marca = marca,
                Ano = ano
            };
        }
        public static Veiculo Atualizar(int idVeiculo, int idCliente, Placa placa, string modelo, string marca, int ano)
        {
            return new Veiculo
            {
                IdVeiculo = idVeiculo,
                IdCliente = idCliente,
                Placa = placa,
                Modelo = modelo,
                Marca = marca,
                Ano = ano
            };
        }

        public static Veiculo Reconstituir(int idVeiculo, int idCliente, Placa placa, string modelo, string marca, int ano) =>
        new() { IdVeiculo = idVeiculo, IdCliente = idCliente, Placa = placa, Modelo = modelo, Marca = marca, Ano = ano };
    }
}