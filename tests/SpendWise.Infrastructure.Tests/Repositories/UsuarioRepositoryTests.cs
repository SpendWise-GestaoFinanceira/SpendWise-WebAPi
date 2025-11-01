using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;
using SpendWise.Infrastructure.Data;
using SpendWise.Infrastructure.Repositories;

namespace SpendWise.Infrastructure.Tests.Repositories;

public class UsuarioRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UsuarioRepository _repository;

    public UsuarioRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UsuarioRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUsuario_WhenValidUsuario()
    {
        // Arrange
        var usuario = new Usuario(
            "João Silva",
            new Email("joao@test.com"),
            "hashedPassword",
            5000m
        );

        // Act
        await _repository.AddAsync(usuario);
        await _context.SaveChangesAsync();

        // Assert
        var savedUsuario = await _context.Usuarios.FindAsync(usuario.Id);
        savedUsuario.Should().NotBeNull();
        savedUsuario!.Nome.Should().Be("João Silva");
        savedUsuario.Email.Valor.Should().Be("joao@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUsuario_WhenEmailExists()
    {
        // Arrange
        var email = new Email("test@example.com");
        var usuario = new Usuario("Test User", email, "hash", 3000m);
        
        await _repository.AddAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Nome.Should().Be("Test User");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUsuario_WhenIdExists()
    {
        // Arrange
        var usuario = new Usuario(
            "Test User",
            new Email("test@example.com"),
            "hash",
            2000m
        );
        
        await _repository.AddAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(usuario.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(usuario.Id);
        result.Nome.Should().Be("Test User");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUsuario_WhenValidChanges()
    {
        // Arrange
        var usuario = new Usuario(
            "Original Name",
            new Email("original@test.com"),
            "hash",
            1000m
        );
        
        await _repository.AddAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        usuario.AtualizarNome("Updated Name");
        usuario.AtualizarRendaMensal(2000m);
        await _repository.UpdateAsync(usuario);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUsuario = await _context.Usuarios.FindAsync(usuario.Id);
        updatedUsuario!.Nome.Should().Be("Updated Name");
        updatedUsuario.RendaMensal.Should().Be(2000m);
        updatedUsuario.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUsuario_WhenUsuarioExists()
    {
        // Arrange
        var usuario = new Usuario(
            "To Delete",
            new Email("delete@test.com"),
            "hash",
            1500m
        );
        
        await _repository.AddAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(usuario.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedUsuario = await _context.Usuarios.FindAsync(usuario.Id);
        deletedUsuario.Should().NotBeNull();
        deletedUsuario!.IsAtivo.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
