using MediatR;
using SpendWise.Application.Commands.FechamentoMensal;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.FechamentoMensal;

public class ReabrirMesCommandHandler : IRequestHandler<ReabrirMesCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReabrirMesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ReabrirMesCommand request, CancellationToken cancellationToken)
    {
        var fechamento = await _unitOfWork.FechamentosMensais
            .GetByUsuarioEAnoMesAsync(request.UsuarioId, request.AnoMes);

        if (fechamento == null)
            return false;

        fechamento.Reabrir($"Solicitação de reabertura em {DateTime.UtcNow:dd/MM/yyyy HH:mm}");
        
        await _unitOfWork.FechamentosMensais.UpdateAsync(fechamento);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
