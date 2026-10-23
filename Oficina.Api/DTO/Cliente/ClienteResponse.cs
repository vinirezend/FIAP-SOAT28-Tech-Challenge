namespace Oficina.Api.DTO.Cliente
{
    public class ClienteResponse
    {
        public int IdCliente { get; set; }
        public string Nome { get; set; }
        public string Documento { get; set; }
        public string TipoDocumento { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
    }
}