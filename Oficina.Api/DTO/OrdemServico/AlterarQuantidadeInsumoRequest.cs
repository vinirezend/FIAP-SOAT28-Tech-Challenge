using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico
{
    public class AlterarQuantidadeInsumoRequest
    {
        [JsonRequired]
        public int Quantidade { get; set; }
    }
}
