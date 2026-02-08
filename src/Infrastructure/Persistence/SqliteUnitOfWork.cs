using System.Data;
using Microsoft.Data.Sqlite;

namespace BankMore.Infrastructure.Persistence;

public sealed class SqliteUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IConnectionFactory _factory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public SqliteUnitOfWork(IConnectionFactory factory)
    {
        _factory = factory;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection is null)
            {
                _connection = _factory.Create();
                _connection.Open();
            }
            return _connection;
        }
    }

    public IDbTransaction? Transaction => _transaction;

    public void Begin()
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction already started");

        _transaction = Connection.BeginTransaction();
    }

    public void Commit()
    {
        _transaction?.Commit();
        DisposeTransaction();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        DisposeTransaction();
    }

    private void DisposeTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
    }
}
