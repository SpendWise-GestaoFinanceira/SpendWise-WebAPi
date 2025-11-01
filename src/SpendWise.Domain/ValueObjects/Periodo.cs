namespace SpendWise.Domain.ValueObjects;

public class Periodo
{
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }

    private Periodo() { } // Para EF Core

    public Periodo(DateTime dataInicio, DateTime dataFim)
    {
        if (dataInicio > dataFim)
            throw new ArgumentException("Data de início não pode ser maior que data de fim");

        DataInicio = dataInicio.Date;
        DataFim = dataFim.Date;
    }

    public int QuantidadeDias => (DataFim - DataInicio).Days + 1;

    public bool ContemData(DateTime data)
    {
        var dataDate = data.Date;
        return dataDate >= DataInicio && dataDate <= DataFim;
    }

    public bool SobrepoeA(Periodo outro)
    {
        return DataInicio <= outro.DataFim && DataFim >= outro.DataInicio;
    }

    public static Periodo MesAtual()
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);
        return new Periodo(inicioMes, fimMes);
    }

    public static Periodo AnoAtual()
    {
        var hoje = DateTime.Today;
        var inicioAno = new DateTime(hoje.Year, 1, 1);
        var fimAno = new DateTime(hoje.Year, 12, 31);
        return new Periodo(inicioAno, fimAno);
    }

    public override string ToString() => $"{DataInicio:dd/MM/yyyy} - {DataFim:dd/MM/yyyy}";

    public override bool Equals(object? obj)
    {
        if (obj is not Periodo other) return false;
        return DataInicio == other.DataInicio && DataFim == other.DataFim;
    }

    public override int GetHashCode() => HashCode.Combine(DataInicio, DataFim);
}
