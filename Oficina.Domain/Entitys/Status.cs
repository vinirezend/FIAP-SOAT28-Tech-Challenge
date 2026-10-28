namespace Oficina.Domain.Entitys
{
    public class Status
    {
        public int IdStatus { get; private set; }
        public string Nome { get; private set; } = default!;

        private Status() { }

        public static Status Reconstituir(int idStatus, string nome) =>
        new() { IdStatus = idStatus, Nome = nome };
    }
}
