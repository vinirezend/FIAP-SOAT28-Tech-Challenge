using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico
{
    public class AdicionarInsumoRequest
    {
        [JsonRequired]
        public int IdInsumo { get; set; }

        [JsonRequired]
        public int Quantidade { get; set; }
    }
}
