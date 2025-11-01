using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;

namespace SpendWise.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> GetByIdAsync(Guid id);
    Task<IEnumerable<Categoria>> GetAllAsync();
    Task<IEnumerable<Categoria>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<IEnumerable<Categoria>> GetByTipoAsync(Guid usuarioId, TipoCategoria tipo);
    Task<Categoria> AddAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByNomeAsync(Guid usuarioId, string nome);
}
