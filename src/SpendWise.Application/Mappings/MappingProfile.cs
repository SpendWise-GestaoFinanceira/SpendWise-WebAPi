using AutoMapper;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Valor));

        CreateMap<Categoria, CategoriaDto>()
            .ForMember(dest => dest.Limite, opt => opt.MapFrom(src => GetLimiteValue(src.Limite)))
            .ForMember(dest => dest.Cor, opt => opt.MapFrom(src => src.Cor));

        CreateMap<Categoria, CategoriaComProgressoDto>()
            .ForMember(dest => dest.Limite, opt => opt.MapFrom(src => GetLimiteValue(src.Limite)))
            .ForMember(dest => dest.Cor, opt => opt.MapFrom(src => src.Cor));

        CreateMap<Transacao, TransacaoDto>()
            .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor.Valor))
            .ForMember(dest => dest.Moeda, opt => opt.MapFrom(src => src.Valor.Moeda))
            .ForMember(dest => dest.CategoriaNome, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nome : null))
            .ForMember(dest => dest.CategoriaCor, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Cor : null))
            .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria))
            .ForMember(dest => dest.UsuarioNome, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nome : null));

        CreateMap<OrcamentoMensal, OrcamentoMensalDto>()
            .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor))
            .ForMember(dest => dest.ValorGasto, opt => opt.Ignore())
            .ForMember(dest => dest.ValorRestante, opt => opt.Ignore())
            .ForMember(dest => dest.PercentualUtilizado, opt => opt.Ignore());

        CreateMap<FechamentoMensal, FechamentoMensalDto>();

        CreateMap<Meta, MetaDto>()
            .ForMember(dest => dest.ValorAlvo, opt => opt.MapFrom(src => src.ValorObjetivo.Valor))
            .ForMember(dest => dest.MoedaAlvo, opt => opt.MapFrom(src => src.ValorObjetivo.Moeda))
            .ForMember(dest => dest.ValorAtual, opt => opt.MapFrom(src => src.ValorAtual.Valor))
            .ForMember(dest => dest.MoedaAtual, opt => opt.MapFrom(src => src.ValorAtual.Moeda))
            .ForMember(dest => dest.PercentualProgresso, opt => opt.MapFrom(src => src.CalcularPercentualProgresso()))
            .ForMember(dest => dest.DiasRestantes, opt => opt.MapFrom(src => src.CalcularDiasRestantes()))
            .ForMember(dest => dest.ValorRestante, opt => opt.MapFrom(src => src.CalcularValorRestante().Valor))
            .ForMember(dest => dest.StatusDescricao, opt => opt.MapFrom(src => src.ObterStatusDescricao()))
            .ForMember(dest => dest.IsAlcancada, opt => opt.MapFrom(src => src.DataAlcancada.HasValue))
            .ForMember(dest => dest.IsVencida, opt => opt.MapFrom(src => src.CalcularDiasRestantes() == 0 && !src.DataAlcancada.HasValue))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Nome + " - " + src.Descricao))
            .ForMember(dest => dest.ProjecaoAlcance, opt => opt.Ignore()); // Calculado quando necessário

        CreateMap<Meta, MetaResumoDto>()
            .ForMember(dest => dest.ValorAlvo, opt => opt.MapFrom(src => src.ValorObjetivo.Valor))
            .ForMember(dest => dest.ValorAtual, opt => opt.MapFrom(src => src.ValorAtual.Valor))
            .ForMember(dest => dest.PercentualProgresso, opt => opt.MapFrom(src => src.CalcularPercentualProgresso()))
            .ForMember(dest => dest.DiasRestantes, opt => opt.MapFrom(src => src.CalcularDiasRestantes()))
            .ForMember(dest => dest.StatusDescricao, opt => opt.MapFrom(src => src.ObterStatusDescricao()))
            .ForMember(dest => dest.IsAlcancada, opt => opt.MapFrom(src => src.DataAlcancada.HasValue))
            .ForMember(dest => dest.IsVencida, opt => opt.MapFrom(src => src.CalcularDiasRestantes() == 0 && !src.DataAlcancada.HasValue))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Nome + " - " + src.Descricao));
    }

    private static decimal? GetLimiteValue(Money? limite)
    {
        return limite?.Valor;
    }
}
