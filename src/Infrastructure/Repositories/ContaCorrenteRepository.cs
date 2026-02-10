using BankMore.Domain.Entities;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Persistence;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;


namespace BankMore.Infrastructure.Repositories;

public sealed class ContaCorrenteRepository : IContaCorrenteRepository
{
    private readonly IConnectionFactory _factory;
    private readonly int _numeroInicial;
    private readonly bool _useStringGuids;

    public ContaCorrenteRepository(IConnectionFactory factory, int numeroInicial, IOptions<DatabaseOptions> dbOptions)
    {
        _factory = factory;
        _numeroInicial = numeroInicial;
        _useStringGuids = dbOptions.Value.UseStringGuids;
    }

    public async Task<bool> ExistePorCpfAsync(string cpf, IDbConnection? conn = null, IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
            var normalizedCpf = BankMore.Domain.ValueObjects.Cpf.Validate(cpf).Value;
            const string sql = "SELECT 1 FROM contacorrente WHERE cpf = @cpf LIMIT 1";
            return await connection.ExecuteScalarAsync<int?>(sql, new { cpf = normalizedCpf }, tx) != null;
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task<bool> ExistePorNumeroAsync(int numero, IDbConnection? conn = null, IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
            const string sql = "SELECT 1 FROM contacorrente WHERE numero = @numero LIMIT 1";
            return await connection.ExecuteScalarAsync<int?>(sql, new { numero }, tx) != null;
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task<int> GetNextNumeroAsync(IDbConnection? conn = null, IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
            const string sql = "SELECT COALESCE(MAX(numero), 0) + 1 FROM contacorrente";
            var nextNumero = await connection.ExecuteScalarAsync<int>(sql, transaction: tx);
            return Math.Max(nextNumero, _numeroInicial);
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task InserirAsync(
        ContaCorrente conta,
        IDbConnection? conn = null,
        IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        const string sql = """
            INSERT INTO contacorrente
            (idcontacorrente, numero, nome, cpf, ativo, senha, salt)
            VALUES
            (@Id, @Numero, @Nome, @Cpf, @Ativo, @Senha, @Salt)
        """;
        try
        {
            await connection.ExecuteAsync(sql, new
            {
                Id = _useStringGuids ? conta.IdContaCorrente.ToString() : conta.IdContaCorrente,
                conta.Numero,
                conta.Nome,
                Cpf = conta.Cpf.Value,
                Ativo = conta.Ativo ? 1 : 0,
                Senha = conta.SenhaHash,
                conta.Salt
            }, tx);
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task<ContaCorrente?> ObterPorNumeroAsync(int numero, IDbConnection? conn = null, IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
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

            var dto = await connection.QuerySingleOrDefaultAsync<ContaCorrenteDto>(sql, new { numero }, tx);
            if (dto is null) return null;

            return ContaCorrente.Hydrate(
                _useStringGuids ? dto.IdContaCorrente : Guid.Parse(dto.IdContaCorrente),
                dto.Numero,
                dto.Nome,
                dto.Cpf,
                dto.Ativo,
                dto.SenhaHash,
                dto.Salt
            );
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task<ContaCorrente?> ObterPorIdAsync(object idContaCorrente, IDbConnection? conn = null, IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
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

            var dto = await connection.QuerySingleOrDefaultAsync<ContaCorrenteDto>(sql, new { Id = _useStringGuids ? idContaCorrente.ToString() : idContaCorrente }, tx);
            if (dto is null) return null;

            return ContaCorrente.Hydrate(
                _useStringGuids ? dto.IdContaCorrente : Guid.Parse(dto.IdContaCorrente),
                dto.Numero,
                dto.Nome,
                dto.Cpf,
                dto.Ativo,
                dto.SenhaHash,
                dto.Salt
            );
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task<IEnumerable<ContaCorrente>> ObterTodosAsync(IDbConnection? conn = null, IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
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

            var dtos = await connection.QueryAsync<ContaCorrenteDto>(sql, transaction: tx);
            return dtos.Select(dto => ContaCorrente.Hydrate(
                _useStringGuids ? dto.IdContaCorrente : Guid.Parse(dto.IdContaCorrente),
                dto.Numero,
                dto.Nome,
                dto.Cpf,
                dto.Ativo,
                dto.SenhaHash,
                dto.Salt
            ));
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task<int> AtualizarStatusAsync(object idContaCorrente, bool ativo, IDbConnection? conn = null, IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
            const string sql = @"
                UPDATE contacorrente
                SET ativo = @Ativo
                WHERE idcontacorrente = @IdContaCorrente
            ";

            return await connection.ExecuteAsync(sql, new
            {
                IdContaCorrente = _useStringGuids ? idContaCorrente.ToString() : idContaCorrente,
                Ativo = ativo
            }, tx);
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
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
}
