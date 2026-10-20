using FluentValidation;
using SolidarSYS.Application.Auth.Validators;
using SolidarSYS.Application.Beneficiarios.DTOs;

namespace SolidarSYS.Application.Beneficiarios.Validators;

public class AtualizarBeneficiarioValidator : AbstractValidator<AtualizarBeneficiarioDto>
{
    public AtualizarBeneficiarioValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório.")
            .MaximumLength(14).WithMessage("CPF inválido.")
            .Must(cpf => !string.IsNullOrWhiteSpace(cpf) && CpfValidator.IsValid(cpf))
            .WithMessage("CPF inválido.");

        RuleFor(x => x.DataNascimento)
            .Must(d => d != default).WithMessage("A data de nascimento é obrigatória.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("A data de nascimento não pode estar no futuro.")
            .Must(d => d.Year >= 1900).WithMessage("Data de nascimento inválida.");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("O endereço é obrigatório.")
            .MaximumLength(255).WithMessage("O endereço deve ter no máximo 255 caracteres.");

        RuleFor(x => x.IdComunidade)
            .GreaterThan(0).WithMessage("A comunidade é obrigatória.");
    }
}