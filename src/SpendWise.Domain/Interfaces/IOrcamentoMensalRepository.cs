using SpendWise.Domain.Entities;

namespace SpendWise.Domain.Interfaces;

public interface IOrcamentoMensalRepository
{
    Task<OrcamentoMensal?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrcamentoMensal>> GetAllAsync();
    Task<IEnumerable<OrcamentoMensal>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<OrcamentoMensal?> GetByUsuarioEAnoMesAsync(Guid usuarioId, string anoMes);
    Task<IEnumerable<OrcamentoMensal>> GetByAnoMesAsync(string anoMes);
    Task AddAsync(OrcamentoMensal orcamento);
    void Update(OrcamentoMensal orcamento);
    void Delete(OrcamentoMensal orcamento);
    
    // Métodos para relatórios
    Task<IEnumerable<OrcamentoMensal>> BuscarPorPeriodoAsync(
        Guid usuarioId,
        int anoInicio,
        int mesInicio,
        int anoFim,
        int mesFim,
        CancellationToken cancellationToken = default);
}