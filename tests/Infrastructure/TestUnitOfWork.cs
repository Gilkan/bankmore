using System;
using System.Data;
using BankMore.Infrastructure.Persistence;

namespace BankMore.Tests.Infrastructure
{
    public sealed class TestUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IDbConnection _connection;

        // Marker to indicate this UoW originates from tests
        public bool TestOriginated { get; } = true;

        // Allow optional existing transaction
        public TestUnitOfWork(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Transaction = transaction;
        }

        public IDbConnection Connection => _connection;
        public IDbTransaction? Transaction { get; private set; }

        public void Begin()
        {
            if (Transaction == null)
                Transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            Transaction?.Commit();
            Transaction?.Dispose();
            Transaction = null;
        }

        public void Rollback()
        {
            Transaction?.Rollback();
            Transaction?.Dispose();
            Transaction = null;
        }

        public void Dispose()
        {
            Transaction?.Dispose();
            // Do NOT dispose the connection here; TestDatabaseFactory manages it
        }
    }
}
