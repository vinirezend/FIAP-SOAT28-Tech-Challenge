using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Entitys
{
    public class Cliente
    {
        public int IdCliente { get; private set; }
        public string Nome { get; private set; } = default!;
        public Documento Documento { get; private set; } = default!;
        public Email Email { get; private set; } = default!;
        public Telefone Telefone { get; private set; } = default!;

        private Cliente() { }

        public static Cliente Criar(string nome, Documento documento, Email email, Telefone telefone)
        {
            return new Cliente
            {
                Nome = ValidarNome(nome),
                Documento = documento,
                Email = email,
                Telefone = telefone
            };
        }
        public void Atualizar(string nome, Email email, Telefone telefone)
        {
            Nome = ValidarNome(nome);
            Email = email;
            Telefone = telefone;
        }

        private static string ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do cliente é obrigatório.");

            return nome.Trim();
        }

        public static Cliente Reconstituir(int idCliente, string nome, Documento documento, Email email, Telefone telefone) =>
        new() { IdCliente = idCliente, Nome = ValidarNome(nome), Documento = documento, Email = email, Telefone = telefone };
    }
}