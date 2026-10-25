using SolidarSYS.Application.Auth.Validators;
using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Doadores.Validators;

public static class DoadorDocumento
{
    public static string Normalizar(string documento)
    {
        var texto = documento.Trim();
        var digitos = new string(texto.Where(char.IsDigit).ToArray());
        if (digitos.Length is 11 or 14)
            return digitos;

        return string.Join(' ', texto.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    public static bool EhValido(TipoDoador tipo, string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return false;

        var digitos = new string(documento.Where(char.IsDigit).ToArray());
        if (tipo == TipoDoador.PESSOA_FISICA && digitos.Length == 11)
            return CpfValidator.IsValid(digitos);
        if (tipo == TipoDoador.PESSOA_JURIDICA && digitos.Length == 14)
            return CnpjValido(digitos);
        if (tipo == TipoDoador.PESSOA_FISICA && digitos.Length == 14)
            return false;
        if (tipo == TipoDoador.PESSOA_JURIDICA && digitos.Length == 11)
            return false;

        return documento.Trim().Length <= 18;
    }

    private static bool CnpjValido(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
            return false;

        var primeiro = CalcularDigito(cnpj[..12], [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        var segundo = CalcularDigito(cnpj[..12] + primeiro, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        return cnpj[12] - '0' == primeiro && cnpj[13] - '0' == segundo;
    }

    private static int CalcularDigito(string baseNumerica, int[] pesos)
    {
        var soma = baseNumerica.Select((caractere, indice) => (caractere - '0') * pesos[indice]).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}