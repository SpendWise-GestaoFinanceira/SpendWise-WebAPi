using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Infrastructure.Data;

namespace SpendWise.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly ApplicationDbContext _context;

    public CategoriaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Categoria?> GetByIdAsync(Guid id)
    {
        return await _context.Categorias
            .Include(c => c.Usuario)
            .Include(c => c.Transacoes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Categoria>> GetAllAsync()
    {
        return await _context.Categorias
            .Include(c => c.Usuario)
            .Where(c => c.IsAtiva)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Categoria>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Categorias
            .Where(c => c.UsuarioId == usuarioId && c.IsAtiva)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Categoria>> GetByTipoAsync(Guid usuarioId, TipoCategoria tipo)
    {
        return await _context.Categorias
            .Where(c => c.UsuarioId == usuarioId && c.Tipo == tipo && c.IsAtiva)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<Categoria> AddAsync(Categoria categoria)
    {
        var result = await _context.Categorias.AddAsync(categoria);
        return result.Entity;
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        _context.Categorias.Update(categoria);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            categoria.Desativar();
            _context.Categorias.Update(categoria);
        }
    }

    public async Task<bool> ExistsByNomeAsync(Guid usuarioId, string nome)
    {
        return await _context.Categorias
            .AnyAsync(c => c.UsuarioId == usuarioId && c.Nome == nome && c.IsAtiva);
    }
}
