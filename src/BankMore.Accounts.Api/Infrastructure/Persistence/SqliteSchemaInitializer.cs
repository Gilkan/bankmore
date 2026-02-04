using Dapper;
using System.Data;

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
        conn.Execute(TransferenciaTable);
        EnsureMovimentoSchema(conn);
        conn.Execute(MovimentoTable);
        conn.Execute(CreateTransferenciaIndexes);
    }

    private static void EnsureMovimentoSchema(IDbConnection conn)
    {
        var tableExists = conn.ExecuteScalar<int>(@"
            SELECT COUNT(1)
            FROM sqlite_master
            WHERE type='table'
            AND name='movimento';");

        if (tableExists == 0)
            return;

        var hasTransferColumn = conn.ExecuteScalar<int>(@"
            SELECT COUNT(1)
            FROM pragma_table_info('movimento')
            WHERE name = 'idtransferencia';");

        if (hasTransferColumn > 0)
            return;

        using var tx = conn.BeginTransaction();

        try
        {
            conn.Execute(@"
            ALTER TABLE movimento RENAME TO movimento_old;

            CREATE TABLE movimento (
                idmovimento TEXT PRIMARY KEY,
                idcontacorrente TEXT NOT NULL,
                idtransferencia TEXT NULL,
                identificacao_requisicao TEXT NOT NULL,
                datamovimento TEXT NOT NULL,
                tipo CHAR(1) NOT NULL,
                valor REAL NOT NULL,
                FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente),
                FOREIGN KEY(idtransferencia) REFERENCES transferencia(idtransferencia),
                UNIQUE(idcontacorrente, identificacao_requisicao)
            );

            INSERT INTO movimento (
                idmovimento,
                idcontacorrente,
                identificacao_requisicao,
                datamovimento,
                tipo,
                valor
            )
            SELECT
                idmovimento,
                idcontacorrente,
                identificacao_requisicao,
                datamovimento,
                tipo,
                valor
            FROM movimento_old;

            DROP TABLE movimento_old;
        ");

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
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
            idtransferencia TEXT NULL,
            identificacao_requisicao TEXT NOT NULL,
            datamovimento TEXT NOT NULL,
            tipo CHAR(1) NOT NULL,
            valor REAL NOT NULL,
            FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente),
            FOREIGN KEY(idtransferencia) REFERENCES transferencia(idtransferencia),
            UNIQUE(idcontacorrente, identificacao_requisicao)
        );
    ";

    private const string TransferenciaTable = @"
        CREATE TABLE IF NOT EXISTS transferencia (
            idtransferencia TEXT PRIMARY KEY,
            idcontaorigem TEXT NOT NULL,
            idcontadestino TEXT NOT NULL,
            identificacao_requisicao TEXT NOT NULL,
            datahora TEXT NOT NULL,
            valor REAL NOT NULL,
            FOREIGN KEY(idcontaorigem) REFERENCES contacorrente(idcontacorrente),
            FOREIGN KEY(idcontadestino) REFERENCES contacorrente(idcontacorrente),
            UNIQUE(idcontaorigem, identificacao_requisicao)
        );
    ";

    private const string CreateTransferenciaIndexes = @"
        CREATE INDEX IF NOT EXISTS idx_transferencia_origem
            ON transferencia(idcontaorigem);

        CREATE INDEX IF NOT EXISTS idx_transferencia_destino
            ON transferencia(idcontadestino);

        CREATE INDEX IF NOT EXISTS idx_movimento_transferencia
            ON movimento(idtransferencia);
    ";
}
