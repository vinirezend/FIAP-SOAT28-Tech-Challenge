namespace Oficina.Application.DTOs
{
    public class TokenDTO
    {
        public TokenDTO(bool authenticated, string created, string expiration, string accessToken)
        {
            Autenticado = authenticated;
            Criado = created;
            Expírado = expiration;
            Token = accessToken;
        }

        public bool Autenticado { get; set; }

        public string Criado { get; set; }

        public string Expírado { get; set; }

        public string Token { get; set; }
    }
}
