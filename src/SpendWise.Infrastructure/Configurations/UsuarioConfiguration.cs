using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpendWise.Domain.Entities;

namespace SpendWise.Infrastructure.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(200);

        // Email configurado no ApplicationDbContext com HasConversion

        builder.Property(u => u.Senha)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.RendaMensal)
            .HasPrecision(18, 2);

        builder.Property(u => u.IsAtivo)
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        // Campos para reset de senha
        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(100);

        builder.Property(u => u.PasswordResetTokenExpiry);

        // Relacionamentos
        builder.HasMany(u => u.Categorias)
            .WithOne(c => c.Usuario)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Transacoes)
            .WithOne(t => t.Usuario)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
