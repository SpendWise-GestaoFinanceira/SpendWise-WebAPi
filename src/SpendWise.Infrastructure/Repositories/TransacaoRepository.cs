using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using SpendWise.Infrastructure.Data;

namespace SpendWise.Infrastructure.Repositories;

public class TransacaoRepository : ITransacaoRepository
{
    private readonly ApplicationDbContext _context;

    public TransacaoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transacao?> GetByIdAsync(Guid id)
    {
        return await _context.Transacoes
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transacao>> GetAllAsync()
    {
        return await _context.Transacoes
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transacao>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transacao>> GetByPeriodoAsync(Guid usuarioId, Periodo periodo)
    {
        return await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId 
                && t.DataTransacao.Date >= periodo.DataInicio 
                && t.DataTransacao.Date <= periodo.DataFim)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transacao>> GetByTipoAsync(Guid usuarioId, TipoTransacao tipo)
    {
        return await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId && t.Tipo == tipo)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transacao>> GetByCategoriaAsync(Guid categoriaId)
    {
        return await _context.Transacoes
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .Where(t => t.CategoriaId == categoriaId)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync();
    }

    public async Task<Transacao> AddAsync(Transacao transacao)
    {
        var result = await _context.Transacoes.AddAsync(transacao);
        return result.Entity;
    }

    public async Task UpdateAsync(Transacao transacao)
    {
        _context.Transacoes.Update(transacao);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var transacao = await _context.Transacoes.FindAsync(id);
        if (transacao != null)
        {
            _context.Transacoes.Remove(transacao);
        }
    }

    public async Task<decimal> GetTotalByTipoAsync(Guid usuarioId, TipoTransacao tipo, Periodo periodo)
    {
        return await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId 
                && t.Tipo == tipo
                && t.DataTransacao.Date >= periodo.DataInicio 
                && t.DataTransacao.Date <= periodo.DataFim)
            .SumAsync(t => t.Valor.Valor);
    }

    public async Task<decimal> GetSaldoAsync(Guid usuarioId, Periodo periodo)
    {
        var receitas = await GetTotalByTipoAsync(usuarioId, TipoTransacao.Receita, periodo);
        var despesas = await GetTotalByTipoAsync(usuarioId, TipoTransacao.Despesa, periodo);
        
        return receitas - despesas;
    }

    public async Task<IEnumerable<Transacao>> GetAdvancedFilteredAsync(
        Guid usuarioId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        decimal? valorMinimo = null,
        decimal? valorMaximo = null,
        Guid? categoriaId = null,
        TipoTransacao? tipo = null,
        string? descricao = null,
        string? observacoes = null,
        string orderBy = "DataTransacao",
        bool ascending = false,
        int skip = 0,
        int take = 10)
    {
        var query = _context.Transacoes
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId);

        // Aplicar filtros
        query = ApplyFilters(query, dataInicio, dataFim, valorMinimo, valorMaximo, 
                           categoriaId, tipo, descricao, observacoes);

        // Aplicar ordenação
        query = ApplyOrdering(query, orderBy, ascending);

        // Aplicar paginação
        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountAdvancedFilteredAsync(
        Guid usuarioId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        decimal? valorMinimo = null,
        decimal? valorMaximo = null,
        Guid? categoriaId = null,
        TipoTransacao? tipo = null,
        string? descricao = null,
        string? observacoes = null)
    {
        var query = _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId);

        // Aplicar filtros
        query = ApplyFilters(query, dataInicio, dataFim, valorMinimo, valorMaximo, 
                           categoriaId, tipo, descricao, observacoes);

        return await query.CountAsync();
    }

    private IQueryable<Transacao> ApplyFilters(
        IQueryable<Transacao> query,
        DateTime? dataInicio,
        DateTime? dataFim,
        decimal? valorMinimo,
        decimal? valorMaximo,
        Guid? categoriaId,
        TipoTransacao? tipo,
        string? descricao,
        string? observacoes)
    {
        if (dataInicio.HasValue)
            query = query.Where(t => t.DataTransacao.Date >= dataInicio.Value.Date);

        if (dataFim.HasValue)
            query = query.Where(t => t.DataTransacao.Date <= dataFim.Value.Date);

        if (valorMinimo.HasValue)
            query = query.Where(t => t.Valor.Valor >= valorMinimo.Value);

        if (valorMaximo.HasValue)
            query = query.Where(t => t.Valor.Valor <= valorMaximo.Value);

        if (categoriaId.HasValue)
            query = query.Where(t => t.CategoriaId == categoriaId.Value);

        if (tipo.HasValue)
            query = query.Where(t => t.Tipo == tipo.Value);

        if (!string.IsNullOrWhiteSpace(descricao))
            query = query.Where(t => t.Descricao.Contains(descricao));

        if (!string.IsNullOrWhiteSpace(observacoes))
            query = query.Where(t => t.Observacoes != null && t.Observacoes.Contains(observacoes));

        return query;
    }

    private IQueryable<Transacao> ApplyOrdering(
        IQueryable<Transacao> query,
        string orderBy,
        bool ascending)
    {
        return orderBy.ToLower() switch
        {
            "valor" => ascending 
                ? query.OrderBy(t => t.Valor.Valor) 
                : query.OrderByDescending(t => t.Valor.Valor),
            "descricao" => ascending 
                ? query.OrderBy(t => t.Descricao) 
                : query.OrderByDescending(t => t.Descricao),
            "categoria" => ascending 
                ? query.OrderBy(t => t.Categoria != null ? t.Categoria.Nome : "") 
                : query.OrderByDescending(t => t.Categoria != null ? t.Categoria.Nome : ""),
            _ => ascending 
                ? query.OrderBy(t => t.DataTransacao) 
                : query.OrderByDescending(t => t.DataTransacao)
        };
    }

    public async Task<IEnumerable<Transacao>> BuscarPorPeriodoComCategoriasAsync(
        Guid usuarioId,
        DateTime dataInicio,
        DateTime dataFim,
        List<Guid>? categoriaIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId 
                && t.DataTransacao >= dataInicio 
                && t.DataTransacao <= dataFim);

        if (categoriaIds != null && categoriaIds.Any())
        {
            query = query.Where(t => categoriaIds.Contains(t.CategoriaId));
        }

        return await query
            .OrderBy(t => t.DataTransacao)
            .ToListAsync(cancellationToken);
    }
}
