
// Gör att vi kan använda MySQL-klasser i C#
using System.Diagnostics.Metrics;
using MySqlConnector;

// Hämtar anslutningssträngen från Database.cs och skapar anslutningen
await using var connection = new MySqlConnection(Databas.HamtaConnectionString());
// Öppnar anslutning till MySQL-databasen
await connection.OpenAsync();
Console.WriteLine("Anslutningen till MySQL lyckades!");
// 
bool fortsatt = true;
while (fortsatt)
{
    // Visar huvudmenyn för användaren
    Console.WriteLine("\n====HOLIDAYMAKER====");
    Console.WriteLine("1. Visa alla kunder");
    Console.WriteLine("2. Registrera ny kund");
    Console.WriteLine("3. Sök lediga rum");
    Console.WriteLine("4. Skapa en bokning");
    Console.WriteLine("5. Visa kundens bokningar");
    Console.WriteLine("6. Ändra antal personer");
    Console.WriteLine("7. Avboka");
    Console.WriteLine("0. Avsluta");
    // Här användare väljar ett alternativ
    Console.Write("Välj ett alternativ: ");
    // Här läser vilket alternativ användare väljer
    string? menyVal = Console.ReadLine();
    // kontroll vilket alternativ användare har valt
    switch (menyVal)
    {
        case "1":
            {
                // Visar kundlistan med den öppna databasanslutningen
                await KundHantering.VisaAllaKunderAsync(connection);
                break;
            }
        case "2":
            {
                // Registrerar en kund med den öppna databasanslutningen
                await KundHantering.RegistreraKundAsync(connection);

                break;
            }
            case "3":
            {
                // kör rumssökningen med den öppna databasanslutningen
                await RumSokning.SokLedigaRumAsync(connection);
                break;
            }
        case "4":
            {
                // Kör bokningsmetoden med den öppna anslutningen
                await BokningHantering.SkapaBokningAsync(connection);

                break;
            }
            // Visar bokningar för en vald kund
            case "5":
            {
                Console.WriteLine("Du valde att visa kundens bokningar.");
                Console.Write("Ange kundens ID: ");
                string? kundIdInput = Console.ReadLine();
                // kontroll att kundens ID är ett positivt heltal
                bool giltigtKundId = int.TryParse(kundIdInput, out int visaKundId) && visaKundId > 0;
                // kontoll om kund_ID är giltigt
                if (!giltigtKundId)
                {
                    Console.WriteLine("Ogiltigt kund-ID.");
                    break;
                }
                // Hämtar kundens bokningar från databasen
                string bokningarSql = """
                   SELECT b.*, bo.namn As boende_namn, r.pris_per_natt, r.rumsnummer
                   FROM bokning b
                   JOIN rum r ON b.rum_id = r.id
                   JOIN boende bo ON r.boende_id = bo.id
                   WHERE b.kund_id = @kundId
                   ORDER BY b.incheckning ASC
                """;
                // skapar SQL-kommandot som ska hämta kundens bokningar
                await using var bokningarCommand = new MySqlCommand(bokningarSql, connection);
                // Skickar kundens ID till SQL-frågan
                bokningarCommand.Parameters.AddWithValue("@kundId", visaKundId);
                // kör SQL-frågan och hämtar kundens bokningar
                await using var bokningarReader = await bokningarCommand.ExecuteReaderAsync();
                // Läser kundens bokningar en i taget
                while (await bokningarReader.ReadAsync())
                {
                    // Hämtar bokningsnumret från bokningen
                    int bokningsId = bokningarReader.GetInt32("id");
                    // Hämtar rumsnummer som text från SQL
                    string rumsnummer = bokningarReader.GetString("rumsnummer");
                    // Hämtar namnet på boende frå SQL-resultatet
                    string boendeNamn = bokningarReader.GetString("boende_namn");
                    // Hämtar incheckningsdatum för bokninngen
                    DateTime bokningIncheckningDatum = bokningarReader.GetDateTime("incheckning");
                    // Hämtar utcheckningsdatum för bokningen
                    DateTime bokningUtcheckningDatum = bokningarReader.GetDateTime("utcheckning");
                    // Hämtar hur många personer bokningen gäller
                    int bokningAntalPersoner = bokningarReader.GetInt32("antal_personer");
                    // Hämtar rummets pris per natt från SQl
                    decimal prisPerNatt = bokningarReader.GetDecimal("pris_per_natt");
                    // Räknar ut hur många nätter kunden ska bo
                    int antalNatter = (bokningUtcheckningDatum - bokningIncheckningDatum).Days;
                    // Räknar ut totalpriset genom antal nätter gånger pris per natt
                    decimal totalpris = antalNatter * prisPerNatt;
                    Console.WriteLine($"Bokningsnummer: {bokningsId}");
                    Console.WriteLine($"Rumsnummer: {rumsnummer}");
                    Console.WriteLine($"Boende: {boendeNamn}");
                    Console.WriteLine($"Incheckning: {bokningIncheckningDatum:yyyy-MM-dd}");
                    Console.WriteLine($"Utcheckning: {bokningUtcheckningDatum:yyyy-MM-dd}");
                    Console.WriteLine($"Antal personer: {bokningAntalPersoner}");
                    Console.WriteLine($"Totalpris: {totalpris:F2} kr");
                    Console.WriteLine("---------------------");
                    
                }
                break;
            }
            // Ändra antal personer
            case "6":
            {
                Console.Write("Ange bokningsnummer: ");
                string? bokningsnummerInput = Console.ReadLine();
                if (!int.TryParse(bokningsnummerInput, out int bokningsnummer) || bokningsnummer <= 0)
                {
                     Console.WriteLine("Ogiltigt bokningsnummer.");
                     break;
                }
                Console.Write("Ange nytt antal personer: ");
                string? antalPersonerInput = Console.ReadLine();
                if (!int.TryParse(antalPersonerInput, out int nyttAntalPersoner) || nyttAntalPersoner <= 0)
                {
                    Console.WriteLine("Ogiltigt antal personer.");
                    break;
                }
                // Hämtar maxantalet för rummet som hör till bokning
                string maxPersonerSql = """
                   SELECT r.max_personer
                   FROM bokning b
                   JOIN rum r ON b.rum_id = r.id
                   WHERE b.id = @bokningsnummer
                """;
                // Skapar kommandot som ska köra SQL
                await using var maxPersonerCommand = new MySqlCommand(maxPersonerSql, connection);
                maxPersonerCommand.Parameters.AddWithValue("@bokningsnummer", bokningsnummer);
                // Hämtar det enda värdet vi behöver från frågan
                object? maxPersonerResultat = await maxPersonerCommand.ExecuteScalarAsync();
                // Avbryter om SQL-frågan inte hittade någon bokning
                if (maxPersonerResultat is null)
                {
                    Console.WriteLine("Bokning hittades inte.");
                    break;
                }
                // Gör om värdet från databasen till ett heltal
                int maxPersoner = Convert.ToInt32(maxPersonerResultat);
                if (nyttAntalPersoner > maxPersoner)
                {
                    Console.WriteLine($"Rummet rymmer högst {maxPersoner} personer.");
                    break;
                }
                // Uppdaterar den valda bokningen om det nya antalet ryms i rummet
                string uppdateraAntalSql = """
                    UPDATE bokning b
                    JOIN rum r ON b.rum_id = r.id
                    SET b.antal_personer = @nyttAntalPersoner
                    WHERE b.id = @bokningsnummer
                    AND @nyttAntalPersoner <= r.max_personer
                """;
                // Skapar kommandot och skickar värdena till SQL
                await using var uppdateraAntalCommand = new MySqlCommand(uppdateraAntalSql, connection);
                uppdateraAntalCommand.Parameters.AddWithValue("@nyttAntalPersoner", nyttAntalPersoner);
                uppdateraAntalCommand.Parameters.AddWithValue("@bokningsnummer", bokningsnummer);
                int berordaRader = await uppdateraAntalCommand.ExecuteNonQueryAsync();
                if (berordaRader > 0)
                {
                    Console.WriteLine($"Antal personer i bokning {bokningsnummer} är nu {nyttAntalPersoner}.");
                
                }
                else
                {
                    Console.WriteLine("Ingen ändring sparades.");
                }
               
                break;
            }
            // för avbokning
            case "7":
            {
                Console.Write("Ange bokningsnummer att avboka: ");
                string? avbokningsnummerInpuut = Console.ReadLine();
                // kontroll för bokningsnumret är positvit heltal
                if (!int.TryParse(avbokningsnummerInpuut, out int avbokningsnummer) || avbokningsnummer <= 0)
                {
                    Console.WriteLine("Ogiltigt bokningsnummer.");
                    break;
                }
                // Tar bort bokningen med det angivna bokningsnumret när frågan körs
                string avbokaSql = """
                    DELETE FROM bokning
                    WHERE id = @bokningsnummer
                """;
                // Skapar kommandot för avbokningen
                await using var avbokaCommand = new MySqlCommand(avbokaSql, connection);
                // kopplar numret som användare skrev till parametern i WHERE
                avbokaCommand.Parameters.AddWithValue("@bokningsnummer", avbokningsnummer);
                // Kör delete och räknar hur många rader som togs bort
                int borttagnaRader = await avbokaCommand.ExecuteNonQueryAsync();
                if (borttagnaRader > 0)
                {
                    Console.WriteLine($"Bokning {avbokningsnummer} har avbokats.");
                }
                else
                {
                    Console.WriteLine("Bokningen hittades inte.");
                }

                break;
            }
    
        case "0":
            fortsatt = false;
            Console.WriteLine("HolidayMaker avslutas.");
            break;
        // Hanterar val som inte finns i menyn
        default:
            Console.WriteLine("Ogiltigt val. Försök igen.");
            break;
    }
}

