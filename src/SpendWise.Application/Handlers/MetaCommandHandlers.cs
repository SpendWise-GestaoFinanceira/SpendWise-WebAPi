using AutoMapper;
using MediatR;
using SpendWise.Application.Commands.Metas;
using SpendWise.Application.DTOs;
using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers;

public class CreateMetaHandler : IRequestHandler<CreateMetaCommand, MetaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEnumerable<IBusinessRule> _businessRules;

    public CreateMetaHandler(IUnitOfWork unitOfWork, IMapper mapper, IEnumerable<IBusinessRule> businessRules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<MetaDto> Handle(CreateMetaCommand request, CancellationToken cancellationToken)
    {
        // Validar se o usuário existe
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId);
        if (usuario == null)
            throw new ArgumentException("Usuário não encontrado.");

        // Validar regras de negócio temporais
        var temporalRule = _businessRules.OfType<TemporalValidationRule>().FirstOrDefault();
        if (temporalRule != null)
        {
            var context = new { CurrentDate = DateTime.UtcNow, TargetDate = request.Prazo };
            
            // Como não temos acesso direto ao ValidationContext, vamos criar uma validação simples
            if (request.Prazo <= DateTime.UtcNow.Date)
                throw new ArgumentException("A data de prazo deve ser futura");
        }

        // Criar a meta
        var meta = new Meta(
            request.Nome,
            request.Descricao,
            new Money(request.ValorObjetivo),
            request.Prazo,
            request.UsuarioId
        );

        // Adicionar valor inicial se fornecido
        if (request.ValorAtual > 0)
        {
            meta.AdicionarProgresso(new Money(request.ValorAtual));
        }

        var metaCriada = await _unitOfWork.Metas.AddAsync(meta);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<MetaDto>(metaCriada);
    }
}

public class UpdateMetaHandler : IRequestHandler<UpdateMetaCommand, MetaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEnumerable<IBusinessRule> _businessRules;

    public UpdateMetaHandler(IUnitOfWork unitOfWork, IMapper mapper, IEnumerable<IBusinessRule> businessRules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<MetaDto> Handle(UpdateMetaCommand request, CancellationToken cancellationToken)
    {
        var meta = await _unitOfWork.Metas.GetByIdAsync(request.Id);
        if (meta == null)
            throw new ArgumentException("Meta não encontrada.");

        // Validar regras de negócio temporais se o prazo foi alterado
        if (request.Prazo.HasValue && request.Prazo != meta.Prazo)
        {
            var temporalRule = _businessRules.OfType<TemporalValidationRule>().FirstOrDefault();
            if (temporalRule != null)
            {
                // Validação temporal simples
                if (request.Prazo.Value <= DateTime.UtcNow.Date)
                    throw new ArgumentException("A data de prazo deve ser futura");
            }
        }

        // Atualizar dados
        if (!string.IsNullOrEmpty(request.Nome))
            meta.AtualizarNome(request.Nome);

        if (!string.IsNullOrEmpty(request.Descricao))
            meta.AtualizarDescricao(request.Descricao);

        if (request.ValorObjetivo.HasValue)
            meta.AtualizarValorObjetivo(new Money(request.ValorObjetivo.Value));

        if (request.Prazo.HasValue)
            meta.AtualizarPrazo(request.Prazo.Value);

        await _unitOfWork.Metas.UpdateAsync(meta);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<MetaDto>(meta);
    }
}

public class UpdateProgressoMetaHandler : IRequestHandler<UpdateProgressoMetaCommand, MetaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProgressoMetaHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MetaDto> Handle(UpdateProgressoMetaCommand request, CancellationToken cancellationToken)
    {
        var meta = await _unitOfWork.Metas.GetByIdAsync(request.MetaId);
        if (meta == null)
            throw new ArgumentException("Meta não encontrada.");

        if (request.ValorProgresso > 0)
        {
            meta.AdicionarProgresso(new Money(request.ValorProgresso));
        }
        else if (request.ValorProgresso < 0)
        {
            meta.RemoverProgresso(new Money(Math.Abs(request.ValorProgresso)));
        }

        await _unitOfWork.Metas.UpdateAsync(meta);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<MetaDto>(meta);
    }
}

public class DeleteMetaHandler : IRequestHandler<DeleteMetaCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMetaHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteMetaCommand request, CancellationToken cancellationToken)
    {
        var meta = await _unitOfWork.Metas.GetByIdAsync(request.Id);
        if (meta == null)
            return false;

        await _unitOfWork.Metas.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

public class ToggleMetaStatusHandler : IRequestHandler<ToggleMetaStatusCommand, MetaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ToggleMetaStatusHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MetaDto> Handle(ToggleMetaStatusCommand request, CancellationToken cancellationToken)
    {
        var meta = await _unitOfWork.Metas.GetByIdAsync(request.MetaId);
        if (meta == null)
            throw new ArgumentException("Meta não encontrada.");

        if (meta.IsAtiva)
            meta.Desativar();
        else
            meta.Reativar();

        await _unitOfWork.Metas.UpdateAsync(meta);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<MetaDto>(meta);
    }
}
