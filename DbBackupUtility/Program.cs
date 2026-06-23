
var postgreSqlProvider = new PostgreSqlProvider(new DatabaseConnectionInfo("localhost", 5433, "testdb", "admin", "admin"), "database-backup-utility-postgres-1");

Console.WriteLine("Testing database connection...");
bool isConnected = await postgreSqlProvider.TestConnectionAsync();

if (isConnected)
{
    Console.WriteLine("Database connection successful.");
}
else
{
    Console.WriteLine("Failed to connect to the database.");
}
