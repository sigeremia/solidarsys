using FluentValidation;
using SolidarSYS.Application.Doadores.DTOs;

namespace SolidarSYS.Application.Doadores.Validators;

public abstract class DoadorRequestValidator<TDto> : AbstractValidator<TDto>
    where TDto : DoadorDto
{
    protected DoadorRequestValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("O tipo de doador é inválido.");
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");
        RuleFor(x => x.Documento)
            .NotEmpty().WithMessage("O documento é obrigatório.")
            .MaximumLength(18).WithMessage("O documento deve ter no máximo 18 caracteres.")
            .Must((dto, documento) => DoadorDocumento.EhValido(dto.Tipo, documento))
            .WithMessage("Documento inválido.");
        RuleFor(x => x.NomeFantasia)
            .MaximumLength(100).WithMessage("O nome fantasia deve ter no máximo 100 caracteres.");
        RuleFor(x => x.Telefone)
            .Must(TelefoneValido).WithMessage("Informe um telefone com DDD válido.");
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(150).WithMessage("O email deve ter no máximo 150 caracteres.");
        RuleFor(x => x.Observacao)
            .MaximumLength(2000).WithMessage("A observação deve ter no máximo 2000 caracteres.");
    }

    private static bool TelefoneValido(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return true;

        var permitido = telefone.All(c => char.IsDigit(c) || c is '+' or '(' or ')' or '-' or ' ');
        var digitos = telefone.Count(char.IsDigit);
        return permitido && digitos is 10 or 11;
    }
}