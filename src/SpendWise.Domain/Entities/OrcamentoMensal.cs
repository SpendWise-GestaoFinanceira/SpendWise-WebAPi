using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Entities;

public class OrcamentoMensal : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public string AnoMes { get; private set; } // Formato: "2025-08"
    public Money Valor { get; private set; }

    // Relacionamentos
    public virtual Usuario Usuario { get; private set; }

    // Construtor privado para EF Core
    private OrcamentoMensal() { }

    public OrcamentoMensal(Guid usuarioId, string anoMes, Money valor)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId não pode ser vazio", nameof(usuarioId));
        
        if (string.IsNullOrWhiteSpace(anoMes))
            throw new ArgumentException("AnoMes não pode ser vazio", nameof(anoMes));
        
        if (!IsValidAnoMesFormat(anoMes))
            throw new ArgumentException("AnoMes deve estar no formato YYYY-MM", nameof(anoMes));

        UsuarioId = usuarioId;
        AnoMes = anoMes;
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
    }

    public void AtualizarValor(Money novoValor)
    {
        Valor = novoValor ?? throw new ArgumentNullException(nameof(novoValor));
        UpdatedAt = DateTime.UtcNow;
    }

    public Money CalcularSaldoDisponivel(Money totalDespesasMes)
    {
        return Valor.Subtract(totalDespesasMes);
    }

    public decimal CalcularPercentualUtilizado(Money totalDespesasMes)
    {
        if (Valor.Valor == 0) return 0;
        return (totalDespesasMes.Valor / Valor.Valor) * 100;
    }

    public bool PodeAdicionarDespesa(Money valorDespesa)
    {
        return valorDespesa.Valor <= Valor.Valor;
    }

    public static string ObterAnoMesAtual()
    {
        return DateTime.Now.ToString("yyyy-MM");
    }

    private static bool IsValidAnoMesFormat(string anoMes)
    {
        return DateTime.TryParseExact(anoMes + "-01", "yyyy-MM-dd", null, 
            System.Globalization.DateTimeStyles.None, out _);
    }
}