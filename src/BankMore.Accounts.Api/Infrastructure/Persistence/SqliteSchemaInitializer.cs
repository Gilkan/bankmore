using Dapper;

namespace BankMore.Accounts.Api.Infrastructure.Persistence;

public sealed class SqliteSchemaInitializer
{
    private readonly SqliteConnectionFactory _factory;

    public SqliteSchemaInitializer(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Initialize()
    {
        using var conn = _factory.Create();

        conn.Execute(ContaCorrenteTable);
        conn.Execute(MovimentoTable);
        // conn.Execute(TransferenciaTable);
        // conn.Execute(IdempotenciaTable);
    }

    private const string ContaCorrenteTable = @"
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

    private const string MovimentoTable = @"
        CREATE TABLE IF NOT EXISTS movimento (
            idmovimento TEXT PRIMARY KEY,
            idcontacorrente TEXT NOT NULL,
            identificacao_requisicao TEXT NOT NULL,
            datamovimento TEXT NOT NULL,
            tipo CHAR(1) NOT NULL,
            valor REAL NOT NULL,
            FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente),
            UNIQUE(idcontacorrente, identificacao_requisicao)
        );
    ";
}
