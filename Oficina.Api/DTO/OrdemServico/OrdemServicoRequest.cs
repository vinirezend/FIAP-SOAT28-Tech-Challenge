using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico
{
    public class OrdemServicoRequest
    {
        [JsonRequired]
        public int IdVeiculo { get; set; }
    }
}
