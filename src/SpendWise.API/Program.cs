using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendWise.Application;
using SpendWise.Infrastructure;
using SpendWise.Infrastructure.Data;
using System.Text;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/spendwise-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicação SpendWise API");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog como provedor de logging
    builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SpendWise API",
        Version = "v1",
        Description = "API para gerenciamento de finanças pessoais"
    });
    
    // Configurar JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. \r\n\r\n Digite 'Bearer' [espaço] e então seu token no campo de texto abaixo.\r\n\r\nExemplo: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=spendwise;Username=financas_user;Password=financas_pass";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["JWT:SecretKey"] ?? "MinhaChaveSecretaSuperSeguraParaJWT2024!@#$%";
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"] ?? "SpendWise",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"] ?? "SpendWise",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add Application Services
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline
// Swagger disponível em todos os ambientes para facilitar testes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpendWise API v1");
    c.RoutePrefix = "swagger";
});
// Add Request Logging Middleware
app.UseMiddleware<SpendWise.API.Middleware.RequestLoggingMiddleware>();

// Add Error Handling Middleware
app.UseMiddleware<SpendWise.API.Middleware.ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

    app.MapControllers();

    // Aplicar migrations automaticamente em produção
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Log.Information("Verificando e aplicando migrations...");
            context.Database.Migrate();
            Log.Information("Migrations aplicadas com sucesso");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao aplicar migrations");
            throw;
        }
    }

    Log.Information("SpendWise API iniciada com sucesso");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}
