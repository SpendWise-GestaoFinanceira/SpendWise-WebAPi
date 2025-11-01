using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.FechamentoMensal;

public record GetFechamentosMensaisByUsuarioQuery(Guid UsuarioId) : IRequest<IEnumerable<FechamentoMensalDto>>;
