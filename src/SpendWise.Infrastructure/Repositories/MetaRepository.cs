using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Infrastructure.Data;

namespace SpendWise.Infrastructure.Repositories;

public class MetaRepository : IMetaRepository
{
    private readonly ApplicationDbContext _context;

    public MetaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Meta?> GetByIdAsync(Guid id)
    {
        return await _context.Metas
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Meta>> GetAllAsync()
    {
        return await _context.Metas
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Meta>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Metas
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Meta>> GetAtivasByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Metas
            .Where(m => m.UsuarioId == usuarioId && m.IsAtiva)
            .OrderBy(m => m.Prazo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Meta>> GetVencidasByUsuarioIdAsync(Guid usuarioId)
    {
        var hoje = DateTime.UtcNow.Date;
        return await _context.Metas
            .Where(m => m.UsuarioId == usuarioId && 
                       m.IsAtiva && 
                       m.DataAlcancada == null && 
                       m.Prazo.Date < hoje)
            .OrderBy(m => m.Prazo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Meta>> GetAlcancadasByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Metas
            .Where(m => m.UsuarioId == usuarioId && m.DataAlcancada != null)
            .OrderByDescending(m => m.DataAlcancada)
            .ToListAsync();
    }

    public async Task<Meta> AddAsync(Meta meta)
    {
        var result = await _context.Metas.AddAsync(meta);
        return result.Entity;
    }

    public async Task<Meta> UpdateAsync(Meta meta)
    {
        _context.Metas.Update(meta);
        return await Task.FromResult(meta);
    }

    public async Task DeleteAsync(Guid id)
    {
        var meta = await _context.Metas.FindAsync(id);
        if (meta != null)
        {
            _context.Metas.Remove(meta);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Metas.AnyAsync(m => m.Id == id);
    }
}
