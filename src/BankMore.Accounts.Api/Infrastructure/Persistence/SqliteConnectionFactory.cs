using Microsoft.Data.Sqlite;
using System.Data;

namespace BankMore.Accounts.Api.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create()
        => new SqliteConnection(_connectionString);
}
