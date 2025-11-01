using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SpendWise.API.Tests.Fixtures;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;
using SpendWise.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SpendWise.API.Tests.Controllers;

public class HealthControllerTests : IClassFixture<SpendWiseWebApplicationFactory>
{
    private readonly SpendWiseWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthControllerTests(SpendWiseWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnHealthy_WhenSystemIsWorking()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        healthStatus.GetProperty("status").GetString().Should().Be("healthy");
        healthStatus.GetProperty("timestamp").GetString().Should().NotBeNullOrEmpty();
        healthStatus.GetProperty("version").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetHealth_ShouldIncludeDatabaseStatus_WhenConnected()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        healthStatus.GetProperty("database").GetProperty("status").GetString().Should().Be("healthy");
        healthStatus.GetProperty("database").GetProperty("responseTime").GetString().Should().NotBeNullOrEmpty();
    }
}
