using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Utils;

public static class DateUtils
{
    public static string ToAnoMesString(DateTime data)
    {
        return data.ToString("yyyy-MM");
    }

    public static string ToAnoMesString(int ano, int mes)
    {
        return $"{ano:0000}-{mes:00}";
    }

    public static Periodo GetPeriodoFromAnoMes(string anoMes)
    {
        if (!DateTime.TryParseExact($"{anoMes}-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var primeiroDia))
        {
            throw new ArgumentException($"Formato de AnoMes inválido: {anoMes}. Use yyyy-MM");
        }

        var ultimoDia = new DateTime(primeiroDia.Year, primeiroDia.Month, DateTime.DaysInMonth(primeiroDia.Year, primeiroDia.Month));

        return new Periodo(primeiroDia, ultimoDia);
    }
}
