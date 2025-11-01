using MediatR;
using SpendWise.Application.DTOs.Transacoes;

namespace SpendWise.Application.Commands.Transacoes;

public record ProcessarArquivoCsvCommand(
    Guid UsuarioId,
    Stream ArquivoStream,
    string NomeArquivo
) : IRequest<PreVisualizacaoImportacaoDto>;

public record ConfirmarImportacaoCsvCommand(
    Guid UsuarioId,
    ConfirmacaoImportacaoDto Confirmacao
) : IRequest<ResultadoImportacaoDto>;
