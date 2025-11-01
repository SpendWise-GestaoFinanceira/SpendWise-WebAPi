using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;

namespace SpendWise.Infrastructure.Configurations;

public class FechamentoMensalConfiguration : IEntityTypeConfiguration<FechamentoMensal>
{
    public void Configure(EntityTypeBuilder<FechamentoMensal> builder)
    {
        builder.ToTable("FechamentosMensais");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(f => f.UsuarioId)
            .IsRequired();

        builder.Property(f => f.AnoMes)
            .IsRequired()
            .HasMaxLength(7)
            .HasComment("Formato YYYY-MM");

        builder.Property(f => f.DataFechamento)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion(
                v => (int)v,
                v => (StatusFechamento)v);

        builder.Property(f => f.TotalReceitas)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(f => f.TotalDespesas)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(f => f.SaldoFinal)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(f => f.Observacoes)
            .HasMaxLength(1000);

        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(f => f.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relacionamentos
        builder.HasOne(f => f.Usuario)
            .WithMany()
            .HasForeignKey(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(f => new { f.UsuarioId, f.AnoMes })
            .IsUnique()
            .HasDatabaseName("IX_FechamentosMensais_Usuario_AnoMes");

        builder.HasIndex(f => f.UsuarioId)
            .HasDatabaseName("IX_FechamentosMensais_UsuarioId");

        builder.HasIndex(f => f.AnoMes)
            .HasDatabaseName("IX_FechamentosMensais_AnoMes");

        builder.HasIndex(f => f.Status)
            .HasDatabaseName("IX_FechamentosMensais_Status");
    }
}
