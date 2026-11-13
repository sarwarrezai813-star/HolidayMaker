
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
    // Läser in kundens uppgifter och sparar kunden i databasen.
    public static async Task RegistreraKundAsync(MySqlConnection connection)
    {
        Console.WriteLine("Du valde att registrera en ny kund");
                Console.Write("Ange ny kunds förnamn: ");
                // läser kundens förnamn, efternamn och epost
                string? nyttFornamn = Console.ReadLine();
                Console.Write("Ange ny kunds efternamn: ");
                string? nyttEfternamn = Console.ReadLine();
                Console.Write("Ange kunderns e-postadress: ");
                string? nyEpost = Console.ReadLine();
                Console.Write("Ange kundens telefonnummer:");
                string? nyttTelefonnummer = Console.ReadLine();
                // kontroll att kundens fornamn inte tomt
                if (string.IsNullOrWhiteSpace(nyttFornamn))
                {
                    Console.WriteLine("Förnamn får inte vara tomt.");
                }
                if (string.IsNullOrWhiteSpace(nyttEfternamn))
                {
                    Console.WriteLine("Efternamn får inte vara tomt.");
                }
                if (string.IsNullOrWhiteSpace(nyEpost))
                {
                    Console.WriteLine("E-postadressen får inte vara tomt.");
                }
                if (string.IsNullOrWhiteSpace(nyttTelefonnummer))
                {
                    Console.WriteLine("Telefonnummer får inte vara tomt.");
                }
                // kontroll om någon av kundens uppgifter saknas
                if (string.IsNullOrWhiteSpace(nyttFornamn) ||
                    string.IsNullOrWhiteSpace(nyttEfternamn) ||
                   string.IsNullOrWhiteSpace(nyEpost) ||
                   string.IsNullOrWhiteSpace(nyttTelefonnummer))
                {
                    return;
                }
                // SQL-fråga som ska registrera en ny kund i databasen
                string insertSql = @"INSERT INTO kund (fornamn, efternamn, epost, telefon)
                VALUES (@fornamn, @efternamn, @epost, @telefon);";
                // detta kommando ska skicka SQL-frågan till MySQL
                await using var insertCommand = new MySqlCommand(insertSql, connection);
                // skickar kundens förnamn, efternamn, epost och telefon som parameter till SQL
                insertCommand.Parameters.AddWithValue("@fornamn", nyttFornamn);
                insertCommand.Parameters.AddWithValue("@efternamn", nyttEfternamn);
                insertCommand.Parameters.AddWithValue("@epost", nyEpost);
                insertCommand.Parameters.AddWithValue("@telefon", nyttTelefonnummer);
                try
                {
                    int antalRader = await insertCommand.ExecuteNonQueryAsync();
                    if (antalRader == 1)
                    {
                        Console.WriteLine("Kunden har registrerats!");
                    }
                    else
                    {
                        Console.WriteLine("Kunden kunde inte registreras.");
                    }
                }
                catch (MySqlException)
                {
                    Console.WriteLine("Ett databasfel uppstod. Kontrollera kunduppgifterna.");
                }
    }
}