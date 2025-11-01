using SpendWise.Domain.Enums;

namespace SpendWise.Domain.Entities;

public class FechamentoMensal : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public string AnoMes { get; private set; } // Formato: YYYY-MM
    public DateTime DataFechamento { get; private set; }
    public StatusFechamento Status { get; private set; }
    public decimal TotalReceitas { get; private set; }
    public decimal TotalDespesas { get; private set; }
    public decimal SaldoFinal { get; private set; }
    public string? Observacoes { get; private set; }

    // Navigation Properties
    public Usuario Usuario { get; set; } = null!;

    // Construtor protegido para EF
    protected FechamentoMensal()
    {
        AnoMes = string.Empty;
    }

    public FechamentoMensal(
        Guid usuarioId,
        string anoMes,
        decimal totalReceitas,
        decimal totalDespesas,
        string? observacoes = null)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId não pode ser vazio");

        if (string.IsNullOrWhiteSpace(anoMes))
            throw new ArgumentException("AnoMes é obrigatório");

        if (!System.Text.RegularExpressions.Regex.IsMatch(anoMes, @"^\d{4}-\d{2}$"))
            throw new ArgumentException("AnoMes deve estar no formato YYYY-MM");

        if (totalReceitas < 0)
            throw new ArgumentException("Total de receitas não pode ser negativo");

        if (totalDespesas < 0)
            throw new ArgumentException("Total de despesas não pode ser negativo");

        UsuarioId = usuarioId;
        AnoMes = anoMes;
        TotalReceitas = totalReceitas;
        TotalDespesas = totalDespesas;
        SaldoFinal = totalReceitas - totalDespesas;
        DataFechamento = DateTime.UtcNow;
        Status = StatusFechamento.Fechado;
        Observacoes = observacoes;
    }

    public void Reabrir(string? motivoReabertura = null)
    {
        if (Status != StatusFechamento.Fechado)
            throw new InvalidOperationException("Só é possível reabrir um mês fechado");

        Status = StatusFechamento.Aberto;
        Observacoes = $"{Observacoes} | Reaberto em {DateTime.UtcNow:dd/MM/yyyy HH:mm} - {motivoReabertura}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarTotais(decimal totalReceitas, decimal totalDespesas)
    {
        if (Status == StatusFechamento.Fechado)
            throw new InvalidOperationException("Não é possível atualizar totais de um mês fechado");

        TotalReceitas = totalReceitas;
        TotalDespesas = totalDespesas;
        SaldoFinal = totalReceitas - totalDespesas;
    }

    public void FecharNovamente()
    {
        if (Status != StatusFechamento.Aberto)
            throw new InvalidOperationException("Só é possível fechar um mês aberto");

        Status = StatusFechamento.Fechado;
        DataFechamento = DateTime.UtcNow;
    }
}
