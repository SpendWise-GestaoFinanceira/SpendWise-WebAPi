using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpendWise.Infrastructure.Data;

namespace SpendWise.Infrastructure.Tests.Data;

public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Database_ShouldBeCreated_WhenEnsureCreatedIsCalled()
    {
        // Act
        var created = await _context.Database.EnsureCreatedAsync();

        // Assert
        created.Should().BeTrue();
    }

    [Fact]
    public void DbSets_ShouldBeConfigured_WhenContextIsCreated()
    {
        // Assert
        _context.Usuarios.Should().NotBeNull();
        _context.Categorias.Should().NotBeNull();
        _context.Transacoes.Should().NotBeNull();
        _context.OrcamentosMensais.Should().NotBeNull();
        _context.FechamentosMensais.Should().NotBeNull();
        _context.Metas.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
