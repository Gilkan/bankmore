using Dapper;
using BankMore.Accounts.Api.Domain.Entities;
using BankMore.Accounts.Api.Domain.Repositories;
using BankMore.Accounts.Api.Infrastructure.Persistence;

namespace BankMore.Accounts.Api.Infrastructure.Repositories;

public sealed class ContaCorrenteRepository : IContaCorrenteRepository
{
    private readonly SqliteConnectionFactory _factory;

    public ContaCorrenteRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
        EnsureTableCreated();
    }

    public async Task<bool> ExistePorCpfAsync(string cpf)
    {
        using var conn = _factory.Create();
        var normalizedCpf = BankMore.Accounts.Api.Domain.ValueObjects.Cpf.Validate(cpf).Value;
        const string sql = "SELECT 1 FROM contacorrente WHERE cpf = @cpf LIMIT 1";
        return await conn.ExecuteScalarAsync<int?>(sql, new { cpf = normalizedCpf }) != null;
    }

    public async Task<bool> ExistePorNumeroAsync(int numero)
    {
        using var conn = _factory.Create();
        const string sql = "SELECT 1 FROM contacorrente WHERE numero = @numero LIMIT 1";
        return await conn.ExecuteScalarAsync<int?>(sql, new { numero }) != null;
    }

    public async Task<int> GetNextNumeroAsync()
    {
        using var conn = _factory.Create();
        const string sql = "SELECT COALESCE(MAX(numero), 1000) + 1 FROM contacorrente";
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    public async Task InserirAsync(ContaCorrente conta)
    {
        using var conn = _factory.Create();

        const string sql = """
            INSERT INTO contacorrente
            (idcontacorrente, numero, nome, cpf, ativo, senha, salt)
            VALUES
            (@Id, @Numero, @Nome, @Cpf, @Ativo, @Senha, @Salt)
        """;

        await conn.ExecuteAsync(sql, new
        {
            Id = conta.IdContaCorrente.ToString(),
            conta.Numero,
            conta.Nome,
            Cpf = conta.Cpf.Value,
            Ativo = conta.Ativo ? 1 : 0,
            Senha = conta.SenhaHash,
            conta.Salt
        });
    }

    public async Task<ContaCorrente?> ObterPorNumeroAsync(int numero)
    {
        using var conn = _factory.Create();
        const string sql = """
            SELECT 
                idcontacorrente,
                numero,
                nome,
                cpf,
                ativo,
                senha AS SenhaHash,
                salt
            FROM contacorrente
            WHERE numero = @numero
        """;

        var dto = await conn.QuerySingleOrDefaultAsync<ContaCorrenteDto>(sql, new { numero });
        if (dto == null) return null;

        return ContaCorrente.Hydrate(
            Guid.Parse(dto.IdContaCorrente),
            dto.Numero,
            dto.Nome,
            dto.Cpf,
            dto.Ativo,
            dto.SenhaHash,
            dto.Salt
        );
    }

    public async Task<ContaCorrente?> ObterPorIdAsync(Guid idContaCorrente)
    {
        using var conn = _factory.Create();

        const string sql = """
            SELECT
                idcontacorrente,
                numero,
                nome,
                cpf,
                ativo,
                senha as SenhaHash,
                salt
            FROM contacorrente
            WHERE idcontacorrente = @Id
            LIMIT 1;
        """;

        var dto = await conn.QuerySingleOrDefaultAsync<ContaCorrenteDto>(
            sql,
            new { Id = idContaCorrente.ToString() });

        if (dto is null)
            return null;

        return ContaCorrente.Hydrate(
            Guid.Parse(dto.IdContaCorrente),
            dto.Numero,
            dto.Nome,
            dto.Cpf,
            dto.Ativo,
            dto.SenhaHash,
            dto.Salt
        );
    }


    public async Task<IEnumerable<ContaCorrente>> ObterTodosAsync()
    {
        using var conn = _factory.Create();
        const string sql = """
            SELECT 
                idcontacorrente,
                numero,
                nome,
                cpf,
                ativo,
                senha AS SenhaHash,
                salt
            FROM contacorrente
        """;

        var dtos = await conn.QueryAsync<ContaCorrenteDto>(sql);
        return dtos.Select(dto => ContaCorrente.Hydrate(
            Guid.Parse(dto.IdContaCorrente),
            dto.Numero,
            dto.Nome,
            dto.Cpf,
            dto.Ativo,
            dto.SenhaHash,
            dto.Salt
        ));
    }

    private class ContaCorrenteDto
    {
        public string IdContaCorrente { get; set; } = null!;
        public int Numero { get; set; }
        public string Nome { get; set; } = null!;
        public string Cpf { get; set; } = null!;
        public bool Ativo { get; set; }
        public string SenhaHash { get; set; } = null!;
        public string Salt { get; set; } = null!;
    }

    public async Task<int> AtualizarStatusAsync(Guid idContaCorrente, bool ativo)
    {
        using var conn = _factory.Create();

        const string sql = @"
            UPDATE contacorrente
            SET ativo = @Ativo
            WHERE idcontacorrente = @IdContaCorrente
        ";

        return await conn.ExecuteAsync(sql, new
        {
            IdContaCorrente = idContaCorrente.ToString(),
            Ativo = ativo
        });
    }



    private void EnsureTableCreated()
    {
        using var conn = _factory.Create();
        const string sql = @"
            CREATE TABLE IF NOT EXISTS contacorrente (
                idcontacorrente TEXT PRIMARY KEY,
                numero INTEGER NOT NULL UNIQUE,
                nome TEXT NOT NULL,
                cpf TEXT UNIQUE NOT NULL,
                ativo INTEGER NOT NULL,
                senha TEXT NOT NULL,
                salt TEXT NOT NULL
            );
        ";
        conn.Execute(sql);
    }
}
