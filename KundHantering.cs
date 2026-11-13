
using MySqlConnector;
static class KundHantering
{
    public static async Task VisaAllaKunderAsync(MySqlConnection connection)
    {
        Console.WriteLine("Du valde att visa alla kunder.");


                // Hämtar ID förnamn och efternamn från databasen
                string sql = "SELECT  id, fornamn, efternamn FROM kund;";
                // Skapar ett kommando som skickar SQL-frågan till MySQL
                await using var command = new MySqlCommand(sql, connection);
                // Kör SQL-frågan och hämtar resultatet från databasen
                await using var reader = await command.ExecuteReaderAsync();
                // Läser en kund i taget så länge det finns fler kunder
                while (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(reader.GetOrdinal("id"));
                    // Hämtar kundens fornamn och efternamn från databasen
                    string fornamn = reader.GetString(reader.GetOrdinal("fornamn"));
                    string efternamn = reader.GetString(reader.GetOrdinal("efternamn"));
                    Console.WriteLine($"{id} - {fornamn} {efternamn}");
                }
                await reader.CloseAsync();
    }
}