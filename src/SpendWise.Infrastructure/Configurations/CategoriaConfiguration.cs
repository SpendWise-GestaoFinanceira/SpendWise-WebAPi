using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpendWise.Domain.Entities;

namespace SpendWise.Infrastructure.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Descricao)
            .HasMaxLength(500);

        builder.Property(c => c.Cor)
            .HasMaxLength(50);

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.IsAtiva)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        // Configuração do Value Object Money para Limite
        builder.OwnsOne(c => c.Limite, limite =>
        {
            limite.Property(m => m.Valor)
                .HasColumnName("LimiteValor")
                .HasPrecision(18, 2);
            
            limite.Property(m => m.Moeda)
                .HasColumnName("LimiteMoeda")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        // Relacionamentos
        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Categorias)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Transacoes)
            .WithOne(t => t.Categoria)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(c => new { c.UsuarioId, c.Nome })
            .IsUnique()
            .HasDatabaseName("IX_Categorias_Usuario_Nome");
    }
}
