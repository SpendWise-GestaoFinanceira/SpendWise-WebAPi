using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using SpendWise.Infrastructure.Data;

namespace SpendWise.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> BuscarPorIdAsync(Guid id)
    {
        return await _context.Usuarios
            .Include(u => u.Categorias)
            .Include(u => u.Transacoes)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByIdAsync(Guid id)
    {
        return await BuscarPorIdAsync(id);
    }

    public async Task<Usuario?> BuscarPorEmailAsync(Email email)
    {
        var emailValor = email.Valor.ToLowerInvariant();
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == emailValor);
    }

    public async Task<IEnumerable<Usuario>> BuscarTodosAsync()
    {
        return await _context.Usuarios
            .Where(u => u.IsAtivo)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await Task.CompletedTask;
    }

    public async Task ExcluirAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            usuario.Desativar();
            _context.Usuarios.Update(usuario);
        }
    }

    public async Task<bool> EmailExisteAsync(Email email)
    {
        var emailValor = email.Valor.ToLowerInvariant();
        return await _context.Usuarios
            .AnyAsync(u => u.Email == emailValor);
    }

    public async Task SalvarAsync()
    {
        await _context.SaveChangesAsync();
    }

    // Métodos adicionais para testes
    public async Task AddAsync(Usuario usuario)
    {
        await AdicionarAsync(usuario);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        var emailValor = email.ToLowerInvariant();
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == emailValor);
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        await AtualizarAsync(usuario);
    }

    public async Task DeleteAsync(Guid id)
    {
        await ExcluirAsync(id);
    }
}
