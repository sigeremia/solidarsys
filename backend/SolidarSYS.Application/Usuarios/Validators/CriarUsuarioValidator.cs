using FluentValidation;
using SolidarSYS.Application.Auth.Validators;
using SolidarSYS.Application.Usuarios.DTOs;

namespace SolidarSYS.Application.Usuarios.Validators;

public class CriarUsuarioValidator : AbstractValidator<CriarUsuarioDto>
{
    public CriarUsuarioValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("A senha deve conter ao menos um caractere especial.");
        RuleFor(x => x.Cpf).NotEmpty().Must(CpfValidator.IsValid).WithMessage("CPF inválido.");
        RuleFor(x => x.DataNascimento).NotEmpty();
        RuleFor(x => x.NivelAcesso).IsInEnum();
    }
}