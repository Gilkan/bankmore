using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using BankMore.Infrastructure.Persistence;

namespace BankMore.Tests.Infrastructure;

public sealed class TestConnectionFactory : IConnectionFactory, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _tempFile;
    private bool _initialized;

    public TestConnectionFactory()
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
        _initialized = true;
    }

    public IDbConnection Create()
    {
        if (!_initialized)
            throw new InvalidOperationException("TestConnectionFactory not initialized.");

        return _connection;
    }

    public void Dispose()
    {
        _connection.Dispose();

        try
        {
            File.Delete(_tempFile);
        }
        catch
        {
            // ignore cleanup failures
        }
    }
}
