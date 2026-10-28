namespace Oficina.Api.DTO.OrdemServico
{
    public class OrdemServicoStatusResponse
    {
        public int IdOsStatus { get; set; }
        public int IdStatus { get; set; }
        public string Status { get; set; }
        public int IdColaborador { get; set; }
        public DateTime DataInclusao { get; set; }
    }
}
