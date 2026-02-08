using System.Data;

namespace BankMore.Infrastructure.Persistence;

public sealed class SqliteUnitOfWork : IUnitOfWork
{
    private readonly SqliteConnectionFactory _factory;

    public IDbConnection Connection { get; private set; } = null!;
    public IDbTransaction Transaction { get; private set; } = null!;

    public SqliteUnitOfWork(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Begin()
    {
        if (Connection != null)
            throw new InvalidOperationException("Transaction already started.");

        Connection = _factory.Create();
        Connection.Open();
        Transaction = Connection.BeginTransaction();
    }


    public void Commit()
    {
        Transaction.Commit();
        Transaction.Dispose();
        Connection.Close();
        Connection.Dispose();

        Transaction = null!;
        Connection = null!;
    }

    public void Rollback()
    {
        Transaction.Rollback();
        Transaction.Dispose();
        Connection.Close();
        Connection.Dispose();

        Transaction = null!;
        Connection = null!;
    }


    public void Dispose()
    {
        Transaction?.Dispose();
        Connection?.Dispose();
    }
}
