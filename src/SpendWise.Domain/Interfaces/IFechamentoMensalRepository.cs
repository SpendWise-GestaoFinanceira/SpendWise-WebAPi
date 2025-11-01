using SpendWise.Domain.Entities;

namespace SpendWise.Domain.Interfaces;

public interface IFechamentoMensalRepository
{
    Task<FechamentoMensal?> GetByIdAsync(Guid id);
    Task<FechamentoMensal?> GetByUsuarioEAnoMesAsync(Guid usuarioId, string anoMes);
    Task<IEnumerable<FechamentoMensal>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<IEnumerable<FechamentoMensal>> GetAllAsync();
    Task<FechamentoMensal> AddAsync(FechamentoMensal fechamentoMensal);
    Task UpdateAsync(FechamentoMensal fechamentoMensal);
    Task DeleteAsync(Guid id);
    Task<bool> ExisteAsync(Guid usuarioId, string anoMes);
    Task<bool> MesEstaFechadoAsync(Guid usuarioId, string anoMes);
}
