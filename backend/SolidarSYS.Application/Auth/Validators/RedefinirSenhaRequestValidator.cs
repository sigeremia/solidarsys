using FluentValidation;
using SolidarSYS.Application.Auth.DTOs;

namespace SolidarSYS.Application.Auth.Validators;

public class RedefinirSenhaRequestValidator : AbstractValidator<RedefinirSenhaRequestDto>
{
    public RedefinirSenhaRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("Nova senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches(@"[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches(@"\d").WithMessage("A senha deve conter ao menos um número.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("A senha deve conter ao menos um caractere especial.");

        RuleFor(x => x.ConfirmarSenha)
            .Equal(x => x.NovaSenha).WithMessage("As senhas não coincidem.");
    }
}
