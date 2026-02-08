using Microsoft.Data.Sqlite;
using System.Data;

namespace BankMore.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        SqliteSchemaInitializer.Initialize(connection);

        return connection;
    }
}
