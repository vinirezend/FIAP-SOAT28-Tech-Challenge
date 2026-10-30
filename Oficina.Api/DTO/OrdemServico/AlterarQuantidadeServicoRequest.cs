using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico
{
    public class AlterarQuantidadeServicoRequest
    {
        [JsonRequired]
        public int Quantidade { get; set; }
    }
}
