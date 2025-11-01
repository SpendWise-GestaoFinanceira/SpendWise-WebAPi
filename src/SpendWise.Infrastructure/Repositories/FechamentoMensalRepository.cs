using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Infrastructure.Data;

namespace SpendWise.Infrastructure.Repositories;

public class FechamentoMensalRepository : IFechamentoMensalRepository
{
    private readonly ApplicationDbContext _context;

    public FechamentoMensalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FechamentoMensal?> GetByIdAsync(Guid id)
    {
        return await _context.FechamentosMensais
            .Include(f => f.Usuario)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FechamentoMensal?> GetByUsuarioEAnoMesAsync(Guid usuarioId, string anoMes)
    {
        return await _context.FechamentosMensais
            .Include(f => f.Usuario)
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.AnoMes == anoMes);
    }

    public async Task<IEnumerable<FechamentoMensal>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.FechamentosMensais
            .Include(f => f.Usuario)
            .Where(f => f.UsuarioId == usuarioId)
            .OrderByDescending(f => f.AnoMes)
            .ToListAsync();
    }

    public async Task<IEnumerable<FechamentoMensal>> GetAllAsync()
    {
        return await _context.FechamentosMensais
            .Include(f => f.Usuario)
            .OrderByDescending(f => f.AnoMes)
            .ToListAsync();
    }

    public async Task<FechamentoMensal> AddAsync(FechamentoMensal fechamentoMensal)
    {
        _context.FechamentosMensais.Add(fechamentoMensal);
        await _context.SaveChangesAsync();
        return fechamentoMensal;
    }

    public async Task UpdateAsync(FechamentoMensal fechamentoMensal)
    {
        _context.FechamentosMensais.Update(fechamentoMensal);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var fechamento = await GetByIdAsync(id);
        if (fechamento != null)
        {
            _context.FechamentosMensais.Remove(fechamento);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteAsync(Guid usuarioId, string anoMes)
    {
        return await _context.FechamentosMensais
            .AnyAsync(f => f.UsuarioId == usuarioId && f.AnoMes == anoMes);
    }

    public async Task<bool> MesEstaFechadoAsync(Guid usuarioId, string anoMes)
    {
        var fechamento = await GetByUsuarioEAnoMesAsync(usuarioId, anoMes);
        return fechamento?.Status == Domain.Enums.StatusFechamento.Fechado;
    }
}
