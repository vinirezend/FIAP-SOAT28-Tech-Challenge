namespace Oficina.Application.DTOs
{
    public class OrdemServicoStatusDTO
    {
        public int IdOsStatus { get; set; }
        public int IdOs { get; set; }
        public int IdStatus { get; set; }
        public string Status { get; set; }
        public int IdColaborador { get; set; }
        public DateTime DataInclusao { get; set; }
    }
}
