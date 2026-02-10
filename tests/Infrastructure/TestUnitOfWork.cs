using System;
using System.Data;
using BankMore.Infrastructure.Persistence;

namespace BankMore.Tests.Infrastructure;

public sealed class TestUnitOfWork : IUnitOfWork
{
    private readonly bool _ownsTransaction;

    public TestUnitOfWork(IDbConnection connection, IDbTransaction? transaction = null)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));

        if (transaction != null)
        {
            Transaction = transaction;
            _ownsTransaction = false;
        }
        else
        {
            Transaction = null;
            _ownsTransaction = true;
        }
    }

    public IDbConnection Connection { get; }

    public IDbTransaction? Transaction { get; private set; }

    public void Begin()
    {
        if (Connection.State != ConnectionState.Open)
            Connection.Open();

        if (_ownsTransaction && Transaction == null)
            Transaction = Connection.BeginTransaction();
    }


    public void Commit()
    {
        if (_ownsTransaction && Transaction != null)
        {
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }
    }

    public void Rollback()
    {
        if (_ownsTransaction && Transaction != null)
        {
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }
    }

    public void Dispose()
    {
        // Intentionally empty.
        // TestUnitOfWork does NOT own the connection.
        // The test controls connection lifetime explicitly.
    }
}
