
using MySqlConnector;

static class Database
{
    public static string HamtaConnectionString()
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            Database = "holiday_maker",
            UserID = "root",
            Password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD")
        };
        return builder.ConnectionString;
    }
}