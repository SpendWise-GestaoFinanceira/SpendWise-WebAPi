using Microsoft.EntityFrameworkCore;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;
using SpendWise.Infrastructure.Configurations;

namespace SpendWise.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }
    public DbSet<OrcamentoMensal> OrcamentosMensais { get; set; }
    public DbSet<FechamentoMensal> FechamentosMensais { get; set; }
    public DbSet<Meta> Metas { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure Value Objects
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.Email)
                .HasConversion(
                    email => email.Valor,
                    value => new Email(value))
                .HasMaxLength(255);

            // Índice único no email
            entity.HasIndex(nameof(Usuario.Email))
                .IsUnique()
                .HasDatabaseName("IX_Usuarios_Email");
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.OwnsOne(e => e.Valor, money =>
            {
                money.Property(m => m.Valor)
                    .HasColumnName("Valor")
                    .HasPrecision(18, 2);
                money.Property(m => m.Moeda)
                    .HasColumnName("Moeda")
                    .HasMaxLength(3)
                    .HasDefaultValue("BRL");
            });
        });


        modelBuilder.Entity<OrcamentoMensal>(entity =>
        {
            entity.OwnsOne(e => e.Valor, money =>
            {
                money.Property(m => m.Valor)
                    .HasColumnName("Valor")
                    .HasPrecision(18, 2);
                money.Property(m => m.Moeda)
                    .HasColumnName("Moeda")
                    .HasMaxLength(3)
                    .HasDefaultValue("BRL");
            });

            // Índice para otimizar consultas por usuário e período
            entity.HasIndex(e => new { e.UsuarioId, e.AnoMes })
                .IsUnique()
                .HasDatabaseName("IX_OrcamentosMensais_Usuario_Periodo");
        });
    }
}
