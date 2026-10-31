using Oficina.Application.DTOs;

namespace Oficina.IntegrationTests.Support;

/// <summary>
/// Dados criados ao longo de um cenário (ids e e-mails), compartilhados entre as classes de steps.
/// </summary>
public class OficinaContext
{
    public int IdColaborador { get; set; }

    public Dictionary<string, int> Veiculos { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> Servicos { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> Insumos { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string? ClienteEmail { get; set; }

    private int? _idOs;
    public int IdOs
    {
        get => _idOs ?? throw new InvalidOperationException("Nenhuma ordem de serviço foi aberta neste cenário.");
        set => _idOs = value;
    }

    private EmailDTO? _email;
    public EmailDTO Email
    {
        get => _email ?? throw new InvalidOperationException("Nenhum e-mail foi conferido neste cenário.");
        set => _email = value;
    }

    private string? _link;
    public string Link
    {
        get => _link ?? throw new InvalidOperationException("Nenhum link do e-mail foi aberto neste cenário.");
        set => _link = value;
    }
}
