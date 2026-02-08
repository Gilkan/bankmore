using System.Text.RegularExpressions;

namespace BankMore.Domain.ValueObjects;

public sealed class Cpf
{
    public string Value { get; private set; } = null!;

    private Cpf() { }

    public static Cpf Validate(string cpf)
    {
        cpf = OnlyDigits(cpf);

        var rules = new List<ICpfRule>
        {
            new LengthRule(),
            new NotAllSameDigitsRule(),
            new CheckDigitRule()
        };

        foreach (var rule in rules)
            rule.Check(cpf);

        return new Cpf { Value = cpf };
    }

    private static string OnlyDigits(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("CPF inválido", "INVALID_DOCUMENT");

        return Regex.Replace(input, @"\D", "");
    }

    private interface ICpfRule
    {
        void Check(string cpf);
    }

    private sealed class LengthRule : ICpfRule
    {
        public void Check(string cpf)
        {
            if (cpf.Length != 11)
                throw new DomainException("CPF deve conter 11 dígitos", "INVALID_DOCUMENT");
        }
    }

    private sealed class NotAllSameDigitsRule : ICpfRule
    {
        public void Check(string cpf)
        {
            if (cpf.Distinct().Count() == 1)
                throw new DomainException("CPF inválido", "INVALID_DOCUMENT");
        }
    }

    private sealed class CheckDigitRule : ICpfRule
    {
        public void Check(string cpf)
        {
            int[] numbers = cpf.Select(c => int.Parse(c.ToString())).ToArray();

            int sum = 0;
            for (int i = 0; i < 9; i++) sum += numbers[i] * (10 - i);
            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;
            if (numbers[9] != firstDigit)
                throw new DomainException("CPF inválido", "INVALID_DOCUMENT");

            sum = 0;
            for (int i = 0; i < 10; i++) sum += numbers[i] * (11 - i);
            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;
            if (numbers[10] != secondDigit)
                throw new DomainException("CPF inválido", "INVALID_DOCUMENT");
        }
    }

    public static Cpf GenerateRandomCpf()
    {
        var random = new Random();
        int[] numbers = new int[11];

        for (int i = 0; i < 9; i++)
            numbers[i] = random.Next(0, 10);

        if (numbers.Distinct().Count() == 1)
            numbers[8] = (numbers[8] + 1) % 10;

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);
        int remainder = sum % 11;
        numbers[9] = remainder < 2 ? 0 : 11 - remainder;

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += numbers[i] * (11 - i);
        remainder = sum % 11;
        numbers[10] = remainder < 2 ? 0 : 11 - remainder;

        string cpfValue = string.Join("", numbers);

        return new Cpf { Value = cpfValue };
    }

    public static string GenerateRandomCpfString()
    {
        return GenerateRandomCpf().Value;
    }
}
