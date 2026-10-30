using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico
{
    public class AdicionarServicoRequest
    {
        [JsonRequired]
        public int IdServico { get; set; }

        [JsonRequired]
        public int Quantidade { get; set; }
    }
}
