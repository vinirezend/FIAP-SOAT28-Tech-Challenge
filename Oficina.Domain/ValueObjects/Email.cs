using System.Net.Mail;

namespace Oficina.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Endereco { get; }

        private Email(string endereco) => Endereco = endereco;
        public static Email Criar(string? valor)
        {
            if (!MailAddress.TryCreate(valor, out _))
                throw new ArgumentException("Email inválido.");

            return new Email(valor!.Trim().ToLowerInvariant());
        }

        public override string ToString() => Endereco;
    }
}