using FluentValidation;
using MediatR;
using SpendWise.Application.Commands.Categorias;
using SpendWise.Application.DTOs.Categorias;
using SpendWise.Application.Queries.Categorias;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Enums;

namespace SpendWise.Application.Handlers.Categorias;

public class GetPreviewExclusaoCategoriaQueryHandler : IRequestHandler<GetPreviewExclusaoCategoriaQuery, PreviewExclusaoCategoriaDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPreviewExclusaoCategoriaQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PreviewExclusaoCategoriaDto> Handle(GetPreviewExclusaoCategoriaQuery request, CancellationToken cancellationToken)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(request.CategoriaId);
        if (categoria == null || categoria.UsuarioId != request.UsuarioId)
        {
            throw new ArgumentException("Categoria não encontrada ou não pertence ao usuário");
        }

        // Buscar todas as despesas da categoria
        var transacoesDaCategoria = await _unitOfWork.Transacoes.GetByCategoriaAsync(request.CategoriaId);
        var despesas = transacoesDaCategoria.Where(t => t.Tipo == TipoTransacao.Despesa).ToList();

        // Buscar outras categorias do usuário para reatribuição
        var todasCategorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        var categoriasAlternativas = todasCategorias
            .Where(c => c.Id != request.CategoriaId)
            .Select(c => new CategoriaSimplificadaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Tipo = c.Tipo.ToString()
            })
            .ToList();

        return new PreviewExclusaoCategoriaDto
        {
            CategoriaId = categoria.Id,
            NomeCategoria = categoria.Nome,
            QuantidadeDespesas = despesas.Count,
            TotalDespesas = despesas.Sum(d => d.Valor.Valor),
            DespesasVinculadas = despesas.Take(10).Select(d => new TransacaoResumoDto
            {
                Id = d.Id,
                Descricao = d.Descricao,
                Valor = d.Valor.Valor,
                DataTransacao = d.DataTransacao
            }).ToList(),
            CategoriasAlternativas = categoriasAlternativas
        };
    }
}

public class ReatribuirDespesasCommandHandler : IRequestHandler<ReatribuirDespesasCommand, ReatribuirDespesasResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ReatribuirDespesasCommand> _validator;

    public ReatribuirDespesasCommandHandler(
        IUnitOfWork unitOfWork,
        IValidator<ReatribuirDespesasCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<ReatribuirDespesasResponseDto> Handle(ReatribuirDespesasCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Verificar se as categorias existem e pertencem ao usuário
        var categoriaOrigem = await _unitOfWork.Categorias.GetByIdAsync(request.CategoriaOrigemId);
        var categoriaDestino = await _unitOfWork.Categorias.GetByIdAsync(request.CategoriaDestinoId);

        if (categoriaOrigem == null || categoriaOrigem.UsuarioId != request.UsuarioId)
            throw new ArgumentException("Categoria de origem não encontrada ou não pertence ao usuário");

        if (categoriaDestino == null || categoriaDestino.UsuarioId != request.UsuarioId)
            throw new ArgumentException("Categoria de destino não encontrada ou não pertence ao usuário");

        // Buscar todas as despesas da categoria origem
        var transacoesDaCategoria = await _unitOfWork.Transacoes.GetByCategoriaAsync(request.CategoriaOrigemId);
        var despesas = transacoesDaCategoria.Where(t => t.Tipo == TipoTransacao.Despesa).ToList();

        // Reatribuir todas as despesas para a nova categoria
        foreach (var despesa in despesas)
        {
            despesa.AtualizarCategoria(categoriaDestino.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        return new ReatribuirDespesasResponseDto
        {
            QuantidadeDespesasMovidas = despesas.Count,
            CategoriaOrigemNome = categoriaOrigem.Nome,
            CategoriaDestinoNome = categoriaDestino.Nome,
            Message = $"{despesas.Count} despesa(s) foram movidas de '{categoriaOrigem.Nome}' para '{categoriaDestino.Nome}'"
        };
    }
}

public class DeleteCategoriaWithReatribuicaoCommandHandler : IRequestHandler<DeleteCategoriaWithReatribuicaoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IValidator<DeleteCategoriaWithReatribuicaoCommand> _validator;

    public DeleteCategoriaWithReatribuicaoCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IValidator<DeleteCategoriaWithReatribuicaoCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteCategoriaWithReatribuicaoCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var categoria = await _unitOfWork.Categorias.GetByIdAsync(request.CategoriaId);
        if (categoria == null || categoria.UsuarioId != request.UsuarioId)
        {
            throw new ArgumentException("Categoria não encontrada ou não pertence ao usuário");
        }

        // Se foi fornecida uma categoria de destino, reatribuir as despesas
        if (request.CategoriaDestinoId.HasValue)
        {
            var reatribuirCommand = new ReatribuirDespesasCommand(
                request.UsuarioId,
                request.CategoriaId,
                request.CategoriaDestinoId.Value);

            await _mediator.Send(reatribuirCommand, cancellationToken);
        }

        // Agora excluir a categoria
        await _unitOfWork.Categorias.DeleteAsync(categoria.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
