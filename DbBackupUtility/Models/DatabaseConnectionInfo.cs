namespace DbBackupUtility.Models;

public class DatabaseConnectionInfo
{
    public string Host { get; }
    public int Port { get; }
    public string DatabaseName { get; }
    public string Username { get; }
    public string Password { get; }

    public DatabaseConnectionInfo(string host, int port, string databaseName, string username, string password)
    {
        Host = host;
        Port = port;
        DatabaseName = databaseName;
        Username = username;
        Password = password;
    }
}