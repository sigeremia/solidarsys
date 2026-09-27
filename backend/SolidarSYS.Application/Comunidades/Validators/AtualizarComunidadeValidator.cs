using FluentValidation;
using SolidarSYS.Application.Comunidades.DTOs;

namespace SolidarSYS.Application.Comunidades.Validators;

public class AtualizarComunidadeValidator : AbstractValidator<AtualizarComunidadeDto>
{
    public AtualizarComunidadeValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome da comunidade é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Regiao)
            .NotEmpty().WithMessage("A região é obrigatória.")
            .MaximumLength(150).WithMessage("A região deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");
    }
}
