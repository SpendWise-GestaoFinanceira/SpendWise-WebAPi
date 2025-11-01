using MediatR;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Exceptions;
using SpendWise.Domain.Utils;

namespace SpendWise.Application.Handlers.Transacoes;

public class DeleteTransacaoCommandHandler : IRequestHandler<DeleteTransacaoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTransacaoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTransacaoCommand request, CancellationToken cancellationToken)
    {
        var transacao = await _unitOfWork.Transacoes.GetByIdAsync(request.Id);
        
        if (transacao == null)
            return false;

        // Verificar se o mês da transação está fechado
        var anoMes = DateUtils.ToAnoMesString(transacao.DataTransacao);
        var mesEstaFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(transacao.UsuarioId, anoMes);
        
        if (mesEstaFechado)
        {
            throw new MesFechadoException(anoMes, "excluir transações");
        }

        await _unitOfWork.Transacoes.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}