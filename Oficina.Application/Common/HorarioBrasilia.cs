namespace Oficina.Application.Common;

/// <summary>
/// Fuso horário da oficina. As datas são gravadas em UTC e convertidas para o horário de Brasília
/// ao exibir para o cliente e ao interpretar filtros por dia.
/// </summary>
public static class HorarioBrasilia
{
    private static readonly TimeSpan OffsetPadrao = TimeSpan.FromHours(-3);

    public static DateTimeOffset Converter(DateTimeOffset data) =>
        TimeZoneInfo.TryFindSystemTimeZoneById("America/Sao_Paulo", out var fusoHorario)
            ? TimeZoneInfo.ConvertTime(data, fusoHorario)
            : data.ToOffset(OffsetPadrao);

    /// <summary>
    /// As colunas de data são "timestamp without time zone" gravadas em UTC.
    /// </summary>
    public static DateTimeOffset ConverterDeUtc(DateTime dataUtc) =>
        Converter(new DateTimeOffset(DateTime.SpecifyKind(dataUtc, DateTimeKind.Utc)));

    /// <summary>
    /// Meia-noite do dia no horário de Brasília, em UTC, para comparar com as colunas de data gravadas em UTC.
    /// </summary>
    public static DateTime InicioDoDiaEmUtc(DateOnly dia)
    {
        var inicioDoDia = dia.ToDateTime(TimeOnly.MinValue);
        var offset = TimeZoneInfo.TryFindSystemTimeZoneById("America/Sao_Paulo", out var fusoHorario)
            ? fusoHorario.GetUtcOffset(inicioDoDia)
            : OffsetPadrao;

        return inicioDoDia - offset;
    }
}
