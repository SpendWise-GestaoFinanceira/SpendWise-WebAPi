namespace SpendWise.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUsuarioRepository Usuarios { get; }
    ICategoriaRepository Categorias { get; }
    ITransacaoRepository Transacoes { get; }
    IOrcamentoMensalRepository OrcamentosMensais { get; }
    IFechamentoMensalRepository FechamentosMensais { get; }
    IMetaRepository Metas { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
