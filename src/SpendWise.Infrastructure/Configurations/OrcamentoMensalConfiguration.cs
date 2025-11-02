using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpendWise.Domain.Entities;

namespace SpendWise.Infrastructure.Configurations;

public class OrcamentoMensalConfiguration : IEntityTypeConfiguration<OrcamentoMensal>
{
    public void Configure(EntityTypeBuilder<OrcamentoMensal> builder)
    {
        builder.ToTable("orcamentos_mensais");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(o => o.AnoMes)
            .HasColumnName("ano_mes")
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("atualizado_em");

        // Configuração do Value Object Money
        builder.OwnsOne(o => o.Valor, money =>
        {
            money.Property(m => m.Valor)
                .HasColumnName("valor")
                .HasColumnType("decimal(14,2)")
                .IsRequired();

            money.Property(m => m.Moeda)
                .HasColumnName("moeda")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        // Relacionamentos
        builder.HasOne(o => o.Usuario)
            .WithMany()
            .HasForeignKey(o => o.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(o => new { o.UsuarioId, o.AnoMes })
            .IsUnique()
            .HasDatabaseName("ix_orcamento_usuario_anomes");

        builder.HasIndex(o => o.AnoMes)
            .HasDatabaseName("ix_orcamento_anomes");

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("ck_orcamento_valor_positivo", "\"Valor\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("ck_orcamento_anomes_formato", "ano_mes ~ '^[0-9]{4}-(0[1-9]|1[0-2])$'"));
    }
}
