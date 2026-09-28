using FluentValidation;
using SolidarSYS.Application.Usuarios.DTOs;

namespace SolidarSYS.Application.Usuarios.Validators;

public class AtualizarUsuarioValidator : AbstractValidator<AtualizarUsuarioDto>
{
    public AtualizarUsuarioValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DataNascimento).NotEmpty();
        RuleFor(x => x.NivelAcesso).IsInEnum();
    }
}