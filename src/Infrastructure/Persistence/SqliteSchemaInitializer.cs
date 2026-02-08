using System.Data;
using Dapper;

namespace BankMore.Infrastructure.Persistence;

public static class SqliteSchemaInitializer
{
    public static void Initialize(IDbConnection conn)
    {
        conn.Execute(ContaCorrenteTable);
        conn.Execute(TransferenciaTable);
        conn.Execute(MovimentoTable);
        conn.Execute(TarifaTable);

        foreach (var sql in CreateTransferenciaIndexes)
            conn.Execute(sql);
    }

    public static IReadOnlyList<string> CreateTransferenciaIndexes => new[]
    {
        @"CREATE INDEX IF NOT EXISTS idx_transferencia_origem
          ON transferencia(idcontaorigem);",

        @"CREATE INDEX IF NOT EXISTS idx_transferencia_destino
          ON transferencia(idcontadestino);",

        @"CREATE INDEX IF NOT EXISTS idx_movimento_transferencia
          ON movimento(idtransferencia);",

        @"CREATE INDEX IF NOT EXISTS idx_tarifa_conta
          ON tarifa(idcontacorrente);",

        @"CREATE INDEX IF NOT EXISTS idx_tarifa_transferencia
          ON tarifa(idtransferencia);"
    };

    public const string ContaCorrenteTable = @"
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

    public const string TransferenciaTable = @"
        CREATE TABLE IF NOT EXISTS transferencia (
            idtransferencia TEXT PRIMARY KEY,
            idcontaorigem TEXT NOT NULL,
            idcontadestino TEXT NOT NULL,
            identificacao_requisicao TEXT NOT NULL,
            datahora TEXT NOT NULL,
            valor REAL NOT NULL,
            FOREIGN KEY(idcontaorigem) REFERENCES contacorrente(idcontacorrente) DEFERRABLE INITIALLY DEFERRED,
            FOREIGN KEY(idcontadestino) REFERENCES contacorrente(idcontacorrente) DEFERRABLE INITIALLY DEFERRED,
            UNIQUE(idcontaorigem, identificacao_requisicao)
        );
    ";

    public const string MovimentoTable = @"
        CREATE TABLE IF NOT EXISTS movimento (
            idmovimento TEXT PRIMARY KEY,
            idcontacorrente TEXT NOT NULL,
            idtransferencia TEXT NULL,
            identificacao_requisicao TEXT NOT NULL,
            datamovimento TEXT NOT NULL,
            tipo CHAR(1) NOT NULL,
            valor REAL NOT NULL,
            FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente) DEFERRABLE INITIALLY DEFERRED,
            FOREIGN KEY(idtransferencia) REFERENCES transferencia(idtransferencia) DEFERRABLE INITIALLY DEFERRED,
            UNIQUE(idcontacorrente, identificacao_requisicao)
        );
    ";

    public const string TarifaTable = @"
        CREATE TABLE IF NOT EXISTS tarifa (
            idtarifa TEXT PRIMARY KEY,
            idcontacorrente TEXT NOT NULL,
            idtransferencia TEXT NOT NULL,
            datahora TEXT NOT NULL,
            valor REAL NOT NULL,
            FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente) DEFERRABLE INITIALLY DEFERRED,
            FOREIGN KEY(idtransferencia) REFERENCES transferencia(idtransferencia) DEFERRABLE INITIALLY DEFERRED,
            UNIQUE(idcontacorrente, idtransferencia)
        );
    ";
}
