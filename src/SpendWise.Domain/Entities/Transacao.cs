using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Entities;

public class Transacao : BaseEntity
{
    public string Descricao { get; private set; }
    public Money Valor { get; private set; }
    public DateTime DataTransacao { get; private set; }
    public TipoTransacao Tipo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid CategoriaId { get; private set; }
    public string? Observacoes { get; private set; }

    // Relacionamentos
    public virtual Usuario Usuario { get; private set; }
    public virtual Categoria Categoria { get; private set; }

    // Construtor privado para EF Core
    private Transacao() { }

    public Transacao(
        string descricao, 
        Money valor, 
        DateTime dataTransacao, 
        TipoTransacao tipo, 
        Guid usuarioId, 
        Guid categoriaId, 
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição não pode ser vazia", nameof(descricao));
        
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId não pode ser vazio", nameof(usuarioId));
        
        if (categoriaId == Guid.Empty)
            throw new ArgumentException("CategoriaId não pode ser vazio", nameof(categoriaId));

        Descricao = descricao;
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        DataTransacao = dataTransacao;
        Tipo = tipo;
        UsuarioId = usuarioId;
        CategoriaId = categoriaId;
        Observacoes = observacoes;
    }

    public void AtualizarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição não pode ser vazia", nameof(descricao));
        
        Descricao = descricao;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarValor(Money valor)
    {
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarDataTransacao(DateTime dataTransacao)
    {
        DataTransacao = dataTransacao;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarObservacoes(string? observacoes)
    {
        Observacoes = observacoes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarCategoria(Guid categoriaId)
    {
        if (categoriaId == Guid.Empty)
            throw new ArgumentException("CategoriaId não pode ser vazio", nameof(categoriaId));
        
        CategoriaId = categoriaId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarTipo(TipoTransacao tipo)
    {
        Tipo = tipo;
        UpdatedAt = DateTime.UtcNow;
    }
}
