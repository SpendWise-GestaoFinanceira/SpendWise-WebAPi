using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorIdAsync(Guid id);
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<Usuario?> BuscarPorEmailAsync(Email email);
    Task<IEnumerable<Usuario>> BuscarTodosAsync();
    Task AdicionarAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
    Task ExcluirAsync(Guid id);
    Task<bool> EmailExisteAsync(Email email);
    Task SalvarAsync();
}
