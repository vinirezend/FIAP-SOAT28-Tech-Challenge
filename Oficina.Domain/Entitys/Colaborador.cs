using Oficina.Domain.Enum;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Entitys
{
    public class Colaborador
    {
        public int IdColaborador { get; private set; }
        public string Nome { get; private set; } = default!;
        public Documento Cpf { get; private set; } = default!;
        public Email Email { get; private set; } = default!;
        public bool Ativo { get; private set; }
        public Departamento IdDepartamento { get; private set; }
        public int? IdUsuario { get; private set; }

        private Colaborador() { }

        public static Colaborador Criar(string nome, Documento cpf, Email email, Departamento idDepartamento, int? idUsuario)
        {
            ArgumentNullException.ThrowIfNull(email);

            return new Colaborador
            {
                Nome = ValidarNome(nome),
                Cpf = ValidarCpf(cpf),
                Email = email,
                Ativo = true,
                IdDepartamento = ValidarDepartamento(idDepartamento),
                IdUsuario = idUsuario
            };
        }

        public void Atualizar(string nome, Email email, Departamento idDepartamento, int? idUsuario, bool ativo)
        {
            ArgumentNullException.ThrowIfNull(email);

            Nome = ValidarNome(nome);
            Email = email;
            IdDepartamento = ValidarDepartamento(idDepartamento);
            IdUsuario = idUsuario;
            Ativo = ativo;
        }

        private static string ValidarNome(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do colaborador é obrigatório.");

            return nome.Trim();
        }

        private static Documento ValidarCpf(Documento cpf)
        {
            ArgumentNullException.ThrowIfNull(cpf);

            if (cpf.Tipo != TipoDocumento.CPF)
                throw new ArgumentException("Documento do colaborador deve ser um CPF.");

            return cpf;
        }

        private static Departamento ValidarDepartamento(Departamento idDepartamento)
        {
            if (!System.Enum.IsDefined(idDepartamento))
                throw new ArgumentException("Departamento do colaborador inválido.");

            return idDepartamento;
        }

        public static Colaborador Reconstituir(int idColaborador, string nome, Documento cpf, Email email, bool ativo, Departamento idDepartamento, int? idUsuario) =>
            new()
            {
                IdColaborador = idColaborador,
                Nome = ValidarNome(nome),
                Cpf = cpf,
                Email = email,
                Ativo = ativo,
                IdDepartamento = idDepartamento,
                IdUsuario = idUsuario
            };
    }
}
