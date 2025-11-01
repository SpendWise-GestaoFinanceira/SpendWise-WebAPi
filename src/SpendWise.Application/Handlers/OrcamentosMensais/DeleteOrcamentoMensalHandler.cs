using MediatR;
using SpendWise.Application.Commands.OrcamentosMensais;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Exceptions;
using FluentValidation;

namespace SpendWise.Application.Handlers.OrcamentosMensais;

public class DeleteOrcamentoMensalHandler : IRequestHandler<DeleteOrcamentoMensalCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteOrcamentoMensalCommand> _validator;

    public DeleteOrcamentoMensalHandler(
        IUnitOfWork unitOfWork,
        IValidator<DeleteOrcamentoMensalCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteOrcamentoMensalCommand request, CancellationToken cancellationToken)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Buscar orçamento existente
        var orcamentoMensal = await _unitOfWork.OrcamentosMensais.GetByIdAsync(request.Id);
        if (orcamentoMensal == null)
        {
            throw new ArgumentException($"Orçamento com ID {request.Id} não foi encontrado");
        }

        // Verificar se o usuário tem permissão para deletar este orçamento
        if (orcamentoMensal.UsuarioId != request.UsuarioId)
        {
            throw new UnauthorizedAccessException("Usuário não tem permissão para deletar este orçamento");
        }

        // Verificar se o mês está fechado
        var mesEstaFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(orcamentoMensal.UsuarioId, orcamentoMensal.AnoMes);
        if (mesEstaFechado)
        {
            throw new MesFechadoException(orcamentoMensal.AnoMes, "excluir orçamentos");
        }

        // Deletar orçamento
        _unitOfWork.OrcamentosMensais.Delete(orcamentoMensal);

        return true;
    }
}
