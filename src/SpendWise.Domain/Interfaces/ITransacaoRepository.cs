using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Interfaces;

public interface ITransacaoRepository
{
    Task<Transacao?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transacao>> GetAllAsync();
    Task<IEnumerable<Transacao>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<IEnumerable<Transacao>> GetByPeriodoAsync(Guid usuarioId, Periodo periodo);
    Task<IEnumerable<Transacao>> GetByTipoAsync(Guid usuarioId, TipoTransacao tipo);
    Task<IEnumerable<Transacao>> GetByCategoriaAsync(Guid categoriaId);
    Task<Transacao> AddAsync(Transacao transacao);
    Task UpdateAsync(Transacao transacao);
    Task DeleteAsync(Guid id);
    Task<decimal> GetTotalByTipoAsync(Guid usuarioId, TipoTransacao tipo, Periodo periodo);
    Task<decimal> GetSaldoAsync(Guid usuarioId, Periodo periodo);
    
    // Filtros Avançados
    Task<IEnumerable<Transacao>> GetAdvancedFilteredAsync(
        Guid usuarioId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        decimal? valorMinimo = null,
        decimal? valorMaximo = null,
        Guid? categoriaId = null,
        TipoTransacao? tipo = null,
        string? descricao = null,
        string? observacoes = null,
        string orderBy = "DataTransacao",
        bool ascending = false,
        int skip = 0,
        int take = 10);
        
    Task<int> CountAdvancedFilteredAsync(
        Guid usuarioId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        decimal? valorMinimo = null,
        decimal? valorMaximo = null,
        Guid? categoriaId = null,
        TipoTransacao? tipo = null,
        string? descricao = null,
        string? observacoes = null);
    
    // Métodos para relatórios
    Task<IEnumerable<Transacao>> BuscarPorPeriodoComCategoriasAsync(
        Guid usuarioId,
        DateTime dataInicio,
        DateTime dataFim,
        List<Guid>? categoriaIds = null,
        CancellationToken cancellationToken = default);
}
