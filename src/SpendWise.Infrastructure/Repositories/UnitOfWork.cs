using Microsoft.EntityFrameworkCore.Storage;
using SpendWise.Domain.Interfaces;
using SpendWise.Infrastructure.Data;
using SpendWise.Infrastructure.Repositories;

namespace SpendWise.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IUsuarioRepository? _usuarios;
    private ICategoriaRepository? _categorias;
    private ITransacaoRepository? _transacoes;
    private IOrcamentoMensalRepository? _orcamentosMensais;
    private IFechamentoMensalRepository? _fechamentosMensais;
    private IMetaRepository? _metas;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUsuarioRepository Usuarios =>
        _usuarios ??= new UsuarioRepository(_context);

    public ICategoriaRepository Categorias =>
        _categorias ??= new CategoriaRepository(_context);

    public ITransacaoRepository Transacoes =>
        _transacoes ??= new TransacaoRepository(_context);

    public IOrcamentoMensalRepository OrcamentosMensais =>
        _orcamentosMensais ??= new OrcamentoMensalRepository(_context);

    public IFechamentoMensalRepository FechamentosMensais =>
        _fechamentosMensais ??= new FechamentoMensalRepository(_context);

    public IMetaRepository Metas =>
        _metas ??= new MetaRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}