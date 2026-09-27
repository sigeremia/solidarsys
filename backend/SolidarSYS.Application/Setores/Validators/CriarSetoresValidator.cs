using FluentValidation;
using SolidarSYS.Application.Setores.DTOs;

namespace SolidarSYS.Application.Setores.Validators;

public class CriarSetoresValidator : AbstractValidator<CriarSetoresDto>
{
    public CriarSetoresValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do setor é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");
    }
}
