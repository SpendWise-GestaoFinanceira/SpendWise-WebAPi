using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Entities;

public class Categoria : BaseEntity
{
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public string? Cor { get; private set; }
    public TipoCategoria Tipo { get; private set; }
    public PrioridadeCategoria Prioridade { get; private set; } = PrioridadeCategoria.Superfluo;
    public Guid UsuarioId { get; private set; }
    public bool IsAtiva { get; private set; } = true;
    public Money? Limite { get; private set; }

    // Relacionamentos
    public virtual Usuario Usuario { get; private set; } = null!;
    public virtual ICollection<Transacao> Transacoes { get; private set; } = new List<Transacao>();

    // Construtor privado para EF Core
    private Categoria() 
    { 
        Nome = string.Empty; 
    }

    public Categoria(string nome, TipoCategoria tipo, Guid usuarioId, string? descricao = null, Money? limite = null, PrioridadeCategoria prioridade = PrioridadeCategoria.Superfluo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da categoria não pode ser vazio", nameof(nome));
        
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId não pode ser vazio", nameof(usuarioId));

        Nome = nome;
        Tipo = tipo;
        Prioridade = prioridade;
        UsuarioId = usuarioId;
        Descricao = descricao;
        Limite = limite;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da categoria não pode ser vazio", nameof(nome));
        
        Nome = nome;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarDescricao(string? descricao)
    {
        Descricao = descricao;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarLimite(Money? limite)
    {
        Limite = limite;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarCor(string? cor)
    {
        Cor = cor;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desativar()
    {
        IsAtiva = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ativar()
    {
        IsAtiva = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // Métodos de negócio para limites
    public decimal CalcularGastoMensal(IEnumerable<Transacao> transacoes, DateTime inicioMes, DateTime fimMes)
    {
        return transacoes
            .Where(t => t.Tipo == Enums.TipoTransacao.Despesa 
                && t.DataTransacao >= inicioMes 
                && t.DataTransacao <= fimMes)
            .Sum(t => t.Valor.Valor);
    }

    public decimal CalcularPercentualUtilizado(decimal gastoAtual)
    {
        if (Limite?.Valor == null || Limite.Valor == 0)
            return 0;
        
        return Math.Round((gastoAtual / Limite.Valor) * 100, 2);
    }

    public StatusLimite VerificarStatusLimite(decimal gastoAtual)
    {
        if (Limite?.Valor == null)
            return StatusLimite.SemLimite;

        var percentual = CalcularPercentualUtilizado(gastoAtual);
        
        return percentual switch
        {
            >= 100 => StatusLimite.Excedido,
            >= 80 => StatusLimite.Alerta,
            _ => StatusLimite.Normal
        };
    }

    public bool PodeAdicionarDespesa(Money valorDespesa, decimal gastoAtual)
    {
        if (Limite?.Valor == null)
            return true; // Sem limite definido, pode adicionar
        
        return (gastoAtual + valorDespesa.Valor) <= Limite.Valor;
    }
}
