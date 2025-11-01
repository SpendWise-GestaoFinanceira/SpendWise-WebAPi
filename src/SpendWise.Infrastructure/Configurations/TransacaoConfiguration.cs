using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpendWise.Domain.Entities;

namespace SpendWise.Infrastructure.Configurations;

public class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> builder)
    {
        builder.ToTable("Transacoes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Descricao)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.DataTransacao)
            .IsRequired();

        builder.Property(t => t.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Observacoes)
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);

        // Relacionamentos
        builder.HasOne(t => t.Usuario)
            .WithMany(u => u.Transacoes)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Categoria)
            .WithMany(c => c.Transacoes)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(t => t.UsuarioId)
            .HasDatabaseName("IX_Transacoes_UsuarioId");

        builder.HasIndex(t => t.CategoriaId)
            .HasDatabaseName("IX_Transacoes_CategoriaId");

        builder.HasIndex(t => t.DataTransacao)
            .HasDatabaseName("IX_Transacoes_DataTransacao");
    }
}
