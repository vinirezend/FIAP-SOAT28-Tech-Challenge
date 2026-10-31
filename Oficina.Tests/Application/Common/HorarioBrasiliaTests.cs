using Oficina.Application.Common;

namespace Oficina.Tests.Application.Common;

public class HorarioBrasiliaTests
{
    [Fact]
    public void InicioDoDiaEmUtc_DeveRetornarMeiaNoiteDeBrasiliaEmUtc()
    {
        var resultado = HorarioBrasilia.InicioDoDiaEmUtc(new DateOnly(2026, 10, 31));

        Assert.Equal(new DateTime(2026, 10, 31, 3, 0, 0), resultado);
    }

    [Fact]
    public void InicioDoDiaEmUtc_FimDaNoiteEmBrasilia_DevePertencerAoMesmoDia()
    {
        // 31/10 às 22h em Brasília é gravado como 01/11 01h UTC e ainda deve contar como dia 31.
        var finalizadaEmUtc = new DateTime(2026, 11, 1, 1, 0, 0);

        Assert.True(finalizadaEmUtc >= HorarioBrasilia.InicioDoDiaEmUtc(new DateOnly(2026, 10, 31)));
        Assert.True(finalizadaEmUtc < HorarioBrasilia.InicioDoDiaEmUtc(new DateOnly(2026, 11, 1)));
    }

    [Fact]
    public void ConverterDeUtc_DeveRetornarHorarioDeBrasilia()
    {
        var resultado = HorarioBrasilia.ConverterDeUtc(new DateTime(2026, 11, 1, 1, 0, 0));

        Assert.Equal(new DateTime(2026, 10, 31, 22, 0, 0), resultado.DateTime);
        Assert.Equal(TimeSpan.FromHours(-3), resultado.Offset);
    }
}
