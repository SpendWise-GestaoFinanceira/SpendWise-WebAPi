using AutoMapper;
using MediatR;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Exceptions;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Utils;

namespace SpendWise.Application.Handlers.Transacoes;

public class UpdateTransacaoCommandHandler : IRequestHandler<UpdateTransacaoCommand, TransacaoDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTransacaoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TransacaoDto?> Handle(UpdateTransacaoCommand request, CancellationToken cancellationToken)
    {
        var transacao = await _unitOfWork.Transacoes.GetByIdAsync(request.Id);

        if (transacao == null)
            return null;

        // Verificar se o mês da transação original está fechado
        var anoMesOriginal = DateUtils.ToAnoMesString(transacao.DataTransacao);
        var mesOriginalFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(transacao.UsuarioId, anoMesOriginal);

        // Verificar se o mês da nova data está fechado (se mudou)
        var anoMesNovo = DateUtils.ToAnoMesString(request.DataTransacao);
        var mesNovoFechado = false;

        if (anoMesOriginal != anoMesNovo)
        {
            mesNovoFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(transacao.UsuarioId, anoMesNovo);
        }

        if (mesOriginalFechado)
        {
            throw new MesFechadoException(anoMesOriginal, "editar transações");
        }
        if (mesNovoFechado)
        {
            throw new MesFechadoException(anoMesNovo, "mover transações para");
        }

        // Usar métodos da entidade para atualizar
        transacao.AtualizarDescricao(request.Descricao);
        transacao.AtualizarValor(request.Valor);
        transacao.AtualizarDataTransacao(request.DataTransacao);
        transacao.AtualizarTipo(request.Tipo);
        transacao.AtualizarCategoria(request.CategoriaId);
        transacao.AtualizarObservacoes(request.Observacoes);

        await _unitOfWork.Transacoes.UpdateAsync(transacao);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TransacaoDto>(transacao);
    }
}
