using Dapper;
using System.Data;

namespace BankMore.Infrastructure.Persistence;

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
        conn.Execute(MovimentoTable);
        conn.Execute(TarifaTable);
        conn.Execute(CreateTransferenciaIndexes);
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

        CREATE INDEX IF NOT EXISTS idx_tarifa_conta
            ON tarifa(idcontacorrente);

        CREATE INDEX IF NOT EXISTS idx_tarifa_transferencia
            ON tarifa(idtransferencia);
    ";


    private const string TarifaTable = @"
        CREATE TABLE IF NOT EXISTS tarifa (
            idtarifa TEXT PRIMARY KEY,
            idcontacorrente TEXT NOT NULL,
            idtransferencia TEXT NOT NULL,
            datahora TEXT NOT NULL,
            valor REAL NOT NULL,
            FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente),
            FOREIGN KEY(idtransferencia) REFERENCES transferencia(idtransferencia),
            UNIQUE(idcontacorrente, idtransferencia)
        );
    ";

}
