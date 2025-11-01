using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Infrastructure.Data;

namespace SpendWise.Infrastructure.Repositories;

public class OrcamentoMensalRepository : IOrcamentoMensalRepository
{
    private readonly ApplicationDbContext _context;

    public OrcamentoMensalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrcamentoMensal?> GetByIdAsync(Guid id)
    {
        return await _context.OrcamentosMensais
            .Include(o => o.Usuario)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<OrcamentoMensal>> GetAllAsync()
    {
        return await _context.OrcamentosMensais
            .Include(o => o.Usuario)
            .OrderByDescending(o => o.AnoMes)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrcamentoMensal>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.OrcamentosMensais
            .Include(o => o.Usuario)
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.AnoMes)
            .ToListAsync();
    }

    public async Task<OrcamentoMensal?> GetByUsuarioEAnoMesAsync(Guid usuarioId, string anoMes)
    {
        return await _context.OrcamentosMensais
            .Include(o => o.Usuario)
            .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId && o.AnoMes == anoMes);
    }

    public async Task<IEnumerable<OrcamentoMensal>> GetByAnoMesAsync(string anoMes)
    {
        return await _context.OrcamentosMensais
            .Include(o => o.Usuario)
            .Where(o => o.AnoMes == anoMes)
            .ToListAsync();
    }

    public async Task AddAsync(OrcamentoMensal orcamento)
    {
        await _context.OrcamentosMensais.AddAsync(orcamento);
    }

    public void Update(OrcamentoMensal orcamento)
    {
        _context.OrcamentosMensais.Update(orcamento);
    }

    public void Delete(OrcamentoMensal orcamento)
    {
        _context.OrcamentosMensais.Remove(orcamento);
    }

    public async Task<IEnumerable<OrcamentoMensal>> BuscarPorPeriodoAsync(
        Guid usuarioId,
        int anoInicio,
        int mesInicio,
        int anoFim,
        int mesFim,
        CancellationToken cancellationToken = default)
    {
        var orcamentos = new List<OrcamentoMensal>();
        
        var dataAtual = new DateTime(anoInicio, mesInicio, 1);
        var dataFim = new DateTime(anoFim, mesFim, 1);

        while (dataAtual <= dataFim)
        {
            var anoMes = $"{dataAtual.Year:0000}-{dataAtual.Month:00}";
            var orcamento = await _context.OrcamentosMensais
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId && o.AnoMes == anoMes, cancellationToken);
            
            if (orcamento != null)
            {
                orcamentos.Add(orcamento);
            }

            dataAtual = dataAtual.AddMonths(1);
        }

        return orcamentos;
    }
}