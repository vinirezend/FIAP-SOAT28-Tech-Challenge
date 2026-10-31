namespace Oficina.Application.Templates;

/// <summary>
/// Conversão para o horário de Brasília, usado nos textos exibidos ao cliente.
/// </summary>
public static class HorarioBrasilia
{
    public static DateTimeOffset Converter(DateTimeOffset data) =>
        TimeZoneInfo.TryFindSystemTimeZoneById("America/Sao_Paulo", out var fusoHorario)
            ? TimeZoneInfo.ConvertTime(data, fusoHorario)
            : data.ToOffset(TimeSpan.FromHours(-3));

    /// <summary>
    /// As colunas de data são "timestamp without time zone" gravadas em UTC.
    /// </summary>
    public static DateTimeOffset ConverterDeUtc(DateTime dataUtc) =>
        Converter(new DateTimeOffset(DateTime.SpecifyKind(dataUtc, DateTimeKind.Utc)));
}
