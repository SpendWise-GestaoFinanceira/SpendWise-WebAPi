using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SpendWise.API.Tests.Fixtures;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;
using SpendWise.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SpendWise.API.Tests.Controllers;

public class CategoriasControllerTests : IClassFixture<SpendWiseWebApplicationFactory>
{
    private readonly SpendWiseWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoriasControllerTests(SpendWiseWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateCategoria_ShouldReturnCreated_WhenValidData()
    {
        // Arrange
        var usuario = await CreateTestUsuarioAsync();
        
        var createCategoriaDto = new
        {
            Nome = "Alimentação",
            Tipo = "Despesa",
            LimiteOrcamento = 1000.00,
            UsuarioId = usuario.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categorias", createCategoriaDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var categoria = JsonSerializer.Deserialize<JsonElement>(content);
        
        categoria.GetProperty("nome").GetString().Should().Be("Alimentação");
        categoria.GetProperty("tipo").GetString().Should().Be("Despesa");
        categoria.GetProperty("limiteOrcamento").GetDecimal().Should().Be(1000.00m);
    }

    [Fact]
    public async Task GetCategorias_ShouldReturnList_WhenCategoriasExist()
    {
        // Arrange
        var usuario = await CreateTestUsuarioAsync();
        await CreateTestCategoriaAsync(usuario.Id, "Transporte", "Despesa");
        await CreateTestCategoriaAsync(usuario.Id, "Salário", "Receita");

        // Act
        var response = await _client.GetAsync($"/api/categorias/usuario/{usuario.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var categorias = JsonSerializer.Deserialize<JsonElement>(content);
        
        categorias.GetArrayLength().Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateCategoria_ShouldReturnOk_WhenValidData()
    {
        // Arrange
        var usuario = await CreateTestUsuarioAsync();
        var categoria = await CreateTestCategoriaAsync(usuario.Id, "Casa", "Despesa");
        
        var updateCategoriaDto = new
        {
            Id = categoria.Id,
            Nome = "Moradia",
            Tipo = "Despesa",
            LimiteOrcamento = 1500.00,
            UsuarioId = usuario.Id
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categorias/{categoria.Id}", updateCategoriaDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCategoria_ShouldReturnNoContent_WhenCategoriaExists()
    {
        // Arrange
        var usuario = await CreateTestUsuarioAsync();
        var categoria = await CreateTestCategoriaAsync(usuario.Id, "Lazer", "Despesa");

        // Act
        var response = await _client.DeleteAsync($"/api/categorias/{categoria.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<Usuario> CreateTestUsuarioAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var usuario = new Usuario(
            "Test User",
            new Email("test@example.com"),
            "hashedPassword",
            5000m
        );
        
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private async Task<Categoria> CreateTestCategoriaAsync(Guid usuarioId, string nome, string tipo)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var categoria = new Categoria(nome, tipo, usuarioId, 500m);
        
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();
        return categoria;
    }
}
