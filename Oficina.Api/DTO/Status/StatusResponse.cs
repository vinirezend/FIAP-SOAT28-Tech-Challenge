namespace Oficina.Api.DTO.Status;

public sealed record StatusResponse
{
    public int IdStatus { get; init; }
    public required string Nome { get; init; }
}
