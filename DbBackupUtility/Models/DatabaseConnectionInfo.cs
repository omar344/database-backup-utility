namespace DbBackupUtility.Models;

public class DatabaseConnectionInfo
{
    public string Host { get; init; }
    public int Port { get; init; }
    public string DatabaseName { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }

    public DatabaseConnectionInfo(string host, int port, string databaseName, string username, string password)
    {
        Host = host;
        Port = port;
        DatabaseName = databaseName;
        Username = username;
        Password = password;
    }
}