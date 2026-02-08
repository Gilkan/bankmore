using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using BankMore.Infrastructure.Persistence;

namespace BankMore.Tests.Infrastructure;

public sealed class TestDatabaseFactory : IDisposable, IConnectionFactory
{
    private readonly SqliteConnection _connection;
    private readonly string _tempFile;

    public IConnectionFactory ConnectionFactory => this;

    public TestDatabaseFactory()
    {
        _tempFile = Path.GetTempFileName();
        var connectionString = $"Data Source={_tempFile};Foreign Keys=True";

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }

        SqliteSchemaInitializer.Initialize(_connection);
    }

    public IDbConnection Create() => _connection;

    public void Dispose()
    {
        _connection.Dispose();

        try
        {
            File.Delete(_tempFile);
        }
        catch
        {
            // ignore
        }
    }
}
