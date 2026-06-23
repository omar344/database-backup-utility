class DatabaseConnectionInfo
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string DatabaseName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public DatabaseConnectionInfo(string host, int port, string databaseName, string username, string password)
    {
        Host = host;
        Port = port;
        DatabaseName = databaseName;
        Username = username;
        Password = password;
    }
}