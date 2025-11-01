using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Infrastructure.Configurations;

public class MetaConfiguration : IEntityTypeConfiguration<Meta>
{
    public void Configure(EntityTypeBuilder<Meta> builder)
    {
        builder.ToTable("Metas");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Descricao)
            .HasMaxLength(500);

        // Configuração para Value Object Money - ValorObjetivo
        builder.OwnsOne(m => m.ValorObjetivo, valorObjetivo =>
        {
            valorObjetivo.Property(money => money.Valor)
                .HasColumnName("ValorObjetivo")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        // Configuração para Value Object Money - ValorAtual
        builder.OwnsOne(m => m.ValorAtual, valorAtual =>
        {
            valorAtual.Property(money => money.Valor)
                .HasColumnName("ValorAtual")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.Property(m => m.Prazo)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(m => m.DataAlcancada)
            .HasColumnType("date");

        builder.Property(m => m.IsAtiva)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // Relacionamento com Usuario
        builder.HasOne(m => m.Usuario)
            .WithMany(u => u.Metas)
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para performance
        builder.HasIndex(m => m.UsuarioId)
            .HasDatabaseName("IX_Metas_UsuarioId");

        builder.HasIndex(m => new { m.UsuarioId, m.IsAtiva })
            .HasDatabaseName("IX_Metas_UsuarioId_IsAtiva");

        builder.HasIndex(m => new { m.UsuarioId, m.Prazo })
            .HasDatabaseName("IX_Metas_UsuarioId_Prazo");

        builder.HasIndex(m => m.DataAlcancada)
            .HasDatabaseName("IX_Metas_DataAlcancada");
    }
}
