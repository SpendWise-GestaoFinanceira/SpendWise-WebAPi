using MediatR;
using SpendWise.Application.DTOs.Relatorios;

namespace SpendWise.Application.Queries.Relatorios;

public class ComparativoMesesQuery : IRequest<ComparativoMesesResponseDto>
{
    public Guid UsuarioId { get; set; }
    public int AnoInicio { get; set; }
    public int MesInicio { get; set; } = 1;
    public int AnoFim { get; set; }
    public int MesFim { get; set; } = 12;
    public bool IncluirDetalhes { get; set; } = false;
    public List<Guid>? CategoriaIds { get; set; }

    public ComparativoMesesQuery() { }

    public ComparativoMesesQuery(Guid usuarioId, ComparativoMesesRequestDto request)
    {
        UsuarioId = usuarioId;
        AnoInicio = request.AnoInicio;
        MesInicio = request.MesInicio;
        AnoFim = request.AnoFim;
        MesFim = request.MesFim;
        IncluirDetalhes = request.IncluirDetalhes;
        CategoriaIds = request.CategoriaIds;
    }
}
