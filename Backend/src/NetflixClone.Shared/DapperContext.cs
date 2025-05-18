using Npgsql;

namespace Shared;

public class DapperContext(string connectionString) : IDisposable
{
    private NpgsqlConnection? _connection;

    public async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();

        return _connection;
    }

    public void Dispose()
    {
        _connection?.DisposeAsync();
    }
}