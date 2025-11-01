using SpendWise.Domain.Entities;

namespace SpendWise.Domain.Interfaces;

public interface IMetaRepository
{
    Task<Meta?> GetByIdAsync(Guid id);
    Task<IEnumerable<Meta>> GetAllAsync();
    Task<IEnumerable<Meta>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<IEnumerable<Meta>> GetAtivasByUsuarioIdAsync(Guid usuarioId);
    Task<IEnumerable<Meta>> GetVencidasByUsuarioIdAsync(Guid usuarioId);
    Task<IEnumerable<Meta>> GetAlcancadasByUsuarioIdAsync(Guid usuarioId);
    Task<Meta> AddAsync(Meta meta);
    Task<Meta> UpdateAsync(Meta meta);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
