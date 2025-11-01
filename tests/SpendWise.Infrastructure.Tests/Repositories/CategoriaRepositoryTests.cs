using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;
using SpendWise.Domain.Enums;
using SpendWise.Infrastructure.Data;
using SpendWise.Infrastructure.Repositories;

namespace SpendWise.Infrastructure.Tests.Repositories;

public class CategoriaRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CategoriaRepository _repository;

    public CategoriaRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CategoriaRepository(_context);
    }

    [Fact]
    public async Task GetByUsuarioIdAsync_ShouldReturnCategorias_WhenUsuarioHasCategorias()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var categoria1 = new Categoria("Alimentação", TipoCategoria.Despesa, usuarioId, "Categoria de alimentação", new Money(1000, "BRL"));
        var categoria2 = new Categoria("Transporte", TipoCategoria.Despesa, usuarioId, "Categoria de transporte", new Money(500, "BRL"));
        var categoria3 = new Categoria("Casa", TipoCategoria.Despesa, Guid.NewGuid(), "Categoria de casa", new Money(800, "BRL")); // Outro usuário

        await _repository.AddAsync(categoria1);
        await _repository.AddAsync(categoria2);
        await _repository.AddAsync(categoria3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUsuarioIdAsync(usuarioId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Nome == "Alimentação");
        result.Should().Contain(c => c.Nome == "Transporte");
        result.Should().NotContain(c => c.Nome == "Casa");
    }

    [Fact]
    public async Task AddAsync_ShouldAddCategoria_WhenValidCategoria()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var categoria = new Categoria("Nova Categoria", TipoCategoria.Despesa, usuarioId, "Descrição", new Money(500, "BRL"));

        // Act
        await _repository.AddAsync(categoria);
        await _context.SaveChangesAsync();

        // Assert
        var savedCategoria = await _context.Categorias.FindAsync(categoria.Id);
        savedCategoria.Should().NotBeNull();
        savedCategoria!.Nome.Should().Be("Nova Categoria");
        savedCategoria.Tipo.Should().Be(TipoCategoria.Despesa);
        savedCategoria.Limite.Should().NotBeNull();
        savedCategoria.Limite!.Valor.Should().Be(500);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategoria_WhenValidChanges()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var categoria = new Categoria("Original", TipoCategoria.Despesa, usuarioId, "Descrição original", new Money(1000, "BRL"));

        await _repository.AddAsync(categoria);
        await _context.SaveChangesAsync();

        // Act
        categoria.AtualizarNome("Atualizada");
        categoria.AtualizarLimite(new Money(1500, "BRL"));
        await _repository.UpdateAsync(categoria);
        await _context.SaveChangesAsync();

        // Assert
        var updatedCategoria = await _context.Categorias.FindAsync(categoria.Id);
        updatedCategoria!.Nome.Should().Be("Atualizada");
        updatedCategoria.Limite.Should().NotBeNull();
        updatedCategoria.Limite!.Valor.Should().Be(1500);
        updatedCategoria.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCategoria_WhenCategoriaExists()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var categoria = new Categoria("Para Deletar", TipoCategoria.Despesa, usuarioId, "Descrição", new Money(300, "BRL"));

        await _repository.AddAsync(categoria);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(categoria.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedCategoria = await _context.Categorias.FindAsync(categoria.Id);
        deletedCategoria.Should().NotBeNull();
        deletedCategoria!.IsAtiva.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
