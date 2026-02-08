using System.Security.Cryptography;
using BankMore.Domain.ValueObjects;
using BankMore.Domain.Repositories;

namespace BankMore.Domain.Entities;

public sealed class ContaCorrente
{
    public Guid IdContaCorrente { get; private set; }
    public int Numero { get; private set; }
    public string Nome { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;
    public bool Ativo { get; private set; }

    public string SenhaHash { get; private set; } = null!;
    public string Salt { get; private set; } = null!;

    private ContaCorrente() { } // ORM


    public void SetNumero(int numero) { Numero = numero; }
    public static ContaCorrente Criar(string nome, string cpf, string senha, int proximoNumero = 1001)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório", "INVALID_ACCOUNT");

        if (string.IsNullOrWhiteSpace(senha))
            throw new DomainException("Senha é obrigatória", "INVALID_ACCOUNT");

        var cpfObj = Cpf.Validate(cpf);

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = HashSenha(senha, saltBytes);

        return new ContaCorrente
        {
            IdContaCorrente = Guid.NewGuid(),
            Numero = proximoNumero,
            Nome = nome,
            Cpf = cpfObj,
            Ativo = true,
            Salt = Convert.ToBase64String(saltBytes),
            SenhaHash = Convert.ToBase64String(hashBytes)
        };
    }

    public void Inativar(string senha)
    {
        if (!Ativo)
            throw new DomainException("Conta já está inativa", "INACTIVE_ACCOUNT");

        if (!SenhaValida(senha))
            throw new DomainException("Senha inválida", "USER_UNAUTHORIZED");

        Ativo = false;
    }

    public bool SenhaValida(string senha)
    {
        var saltBytes = Convert.FromBase64String(Salt);
        var hashBytes = HashSenha(senha, saltBytes);
        return Convert.ToBase64String(hashBytes) == SenhaHash;
    }

    private static byte[] HashSenha(string senha, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            senha,
            salt,
            100_000,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(32);
    }

    public static ContaCorrente Hydrate(
        Guid id,
        int numero,
        string nome,
        string cpf,
        bool ativo,
        string senhaHash,
        string salt)
    {
        return new ContaCorrente
        {
            IdContaCorrente = id,
            Numero = numero,
            Nome = nome,
            Cpf = Cpf.Validate(cpf),
            Ativo = ativo,
            SenhaHash = senhaHash,
            Salt = salt
        };
    }

    public void ValidarDebito(decimal saldoAtual, decimal valor)
    {
        const decimal limite = 0m; // TODO: substituir por persistência futura

        if (saldoAtual + limite < valor)
            throw new DomainException(
                "Saldo insuficiente",
                "INSUFFICIENT_FUNDS");
    }
}
