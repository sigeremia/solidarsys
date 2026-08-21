namespace SolidarSYS.Application.Auth.Validators;

public static class CpfValidator
{
    public static bool IsValid(string cpf)
    {
        cpf = new string(cpf.Where(char.IsDigit).ToArray());
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;

        int[] pesos1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesos2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        int soma1 = cpf.Take(9).Select((c, i) => (c - '0') * pesos1[i]).Sum();
        int d1 = soma1 % 11 < 2 ? 0 : 11 - soma1 % 11;

        int soma2 = cpf.Take(10).Select((c, i) => (c - '0') * pesos2[i]).Sum();
        int d2 = soma2 % 11 < 2 ? 0 : 11 - soma2 % 11;

        return cpf[9] - '0' == d1 && cpf[10] - '0' == d2;
    }
}
