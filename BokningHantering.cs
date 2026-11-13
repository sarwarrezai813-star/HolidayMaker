
using MySqlConnector;

static class BokningHantering
{
    // Skapar en bokning med den öppna databasanslutningen
    public static async Task SkapaBokningAsync(MySqlConnection connection)
    {
        Console.WriteLine("Du valde att skapa en bokning.");
                Console.Write("Ange kundens ID: ");
                string? input = Console.ReadLine();
                // kontroll om användare skrivit ett giltigt heltal
                bool success = int.TryParse(input, out int kundId);
                // kontroll om kunden id är positivt heltal
                if (success && kundId > 0)
                {
                    // kontroll om kundens ID finns i MySQL-database
                    string checkSql = "SELECT COUNT(*) FROM kund WHERE id = @kundId;";
                    // skapar ett kommando som ska kontrollera kunden ID i MySQL
                    await using var checkCommand = new MySqlCommand(checkSql, connection);
                    // skickar kundens ID som en parameter till SQL
                    checkCommand.Parameters.AddWithValue("@kundId", kundId);
                    // Kontroll SQL-fråga och räknar hur många kunder som har detta ID
                    int antalkunder = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                    if (antalkunder > 0)
                    {
                        Console.WriteLine($"Du valde kund ID: {kundId}");
                        // frågar vilken rum kunden vill boka
                        Console.Write("Ange Rum-ID som du vill boka: ");
                        string? rumInput = Console.ReadLine();
                        // kontroll att rummets ID är ett positivt heltal
                        bool giltigtRumId = int.TryParse(rumInput, out int rumId) && rumId > 0;
                        if (!giltigtRumId)
                        {
                            Console.WriteLine("Ogiltigt Rum-ID. Ange ett positivt heltal.");
                            return;
                        }
                        // Här kontroll om rummet finns i databasen
                        string rumCheckSql = "SELECT COUNT(*) FROM rum WHERE id = @rumId;";
                        await using var rumCheckCommand = new MySqlCommand(rumCheckSql, connection);
                        // skickar rummets ID säkert till MySQL
                        rumCheckCommand.Parameters.AddWithValue("@rumId", rumId);
                        // Räknar hur många rum som har detta ID
                        int antalRum = Convert.ToInt32(await rumCheckCommand.ExecuteScalarAsync());
                        // kontroll om rummet finns i databasen
                        if (antalRum == 0)
                        {
                            Console.WriteLine("Rummet finns inte. Välja ett giltigt Rum-ID.");
                            return;
                        }
                        // frågar kunden om incheckningsdatum
                        Console.Write("Ange incheckningsdatum (ÅÅÅÅ-MM-DD): ");
                        // Sparar kundens incheckningsdatum
                        string? bokningIncheckningInput = Console.ReadLine();
                        // frågar kunden om utcheckningsdatum
                        Console.Write("Ange utcheckningsdatum (ÅÅÅÅ-MM-DD): ");
                        string? bokkningUtcheckningInput = Console.ReadLine();
                        // kontroll att incheckningsdatumet har rätt format
                        bool giltigtBokningIncheckning = DateTime.TryParseExact(
                            bokningIncheckningInput,
                            "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime bokningIncheckning

                        );
                        // Här stoppar bokning om datumet är ogiltigt
                        if (!giltigtBokningIncheckning)
                        {
                            Console.WriteLine("Ogiltigt incheckningsdatum. Använd ÅÅÅÅ-MM-DD: ");
                            return;
                        }
                        bool giltigBokningUtcheckning = DateTime.TryParseExact(
                            bokkningUtcheckningInput,
                            "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime bokningUtcheckning
                        );
                        if (!giltigBokningUtcheckning || bokningUtcheckning <= bokningIncheckning)
                        {
                            Console.WriteLine("Ogiltigt utcheckningsdatum. Det måste vara efter incheckningsdatumet.");
                            return;
                        }
                        // kontroll om rummet redan är bokat under perioden
                        string ledigtRumSql = """
                            SELECT COUNT(*)
                            FROM bokning
                            WHERE rum_id = @rumId
                            AND incheckning < @utcheckning
                            AND utcheckning > @incheckning
                        """;
                        // skapar ett SQL-kommando för att kontrollera rummets bokningar
                        await using var ledigtRumCommand = new MySqlCommand(ledigtRumSql, connection);
                        ledigtRumCommand.Parameters.AddWithValue("@rumId", rumId);
                        ledigtRumCommand.Parameters.AddWithValue("@incheckning", bokningIncheckning);
                        ledigtRumCommand.Parameters.AddWithValue("@utcheckning", bokningUtcheckning);
                        int antalKrockar = Convert.ToInt32(
                            await ledigtRumCommand.ExecuteScalarAsync()

                        );
                        if (antalKrockar > 0)
                        {
                            Console.WriteLine("Rummet är redan bokat under dessa datum.");
                            return;
                        }
                        string skapaBokningSql = """
                            INSERT INTO bokning
                            (kund_id, rum_id, incheckning, utcheckning, antal_personer)
                            VALUES
                            (@kundId, @rumId, @incheckning, @utcheckning, @antalPersoner);
                        """;
                        Console.Write("Ange antal personer: ");
                        // läser användarens svar
                        string? antalPersonerInput = Console.ReadLine();
                        // kontroll att användare har skrivit ett positivt heltal
                        bool giltigtAntalPersoner = int.TryParse(
                            antalPersonerInput,
                            out int bokningAntalPersoner 
                        )&& bokningAntalPersoner > 0;
                        if (!giltigtAntalPersoner)
                        {
                            Console.WriteLine("Ange ett giltigt antal personer.");
                            return;
                        }
                        // hämtar hur många personer som får bo i det valda rummet
                        string kapacitetSql = "SELECT max_personer, pris_per_natt FROM rum WHERE id = @rumId;";
                        await using var kapacitetCommand = new MySqlCommand(kapacitetSql, connection);
                        kapacitetCommand.Parameters.AddWithValue("@rumId", rumId);
                        // kör SQL-frågan och gör det möjligt att läsa flera kolumner.
                        await using var kapacitetReader = await kapacitetCommand.ExecuteReaderAsync();
                        // Läser raden med rummets uppgifter
                        await kapacitetReader.ReadAsync();
                        // Hämtar rummets maximal antal personer
                        int maxPersoner = kapacitetReader.GetInt32(0);
                        // Hämtar rummets pris per natt
                        decimal prisPerNatt = kapacitetReader.GetDecimal(1);
                        // Stänger läsaren innan nästa SQL-kommando körs
                        await kapacitetReader.CloseAsync();
                        if (bokningAntalPersoner > maxPersoner)
                        {
                            Console.WriteLine($"Rummet har bara plats för {maxPersoner} personer.");
                            return;
                        }
                        // Räknar hur många nätter kunden ska bo
                        int antalNatter = (bokningUtcheckning - bokningIncheckning).Days;
                        // Räknar ut totalpriset genom multiplicera antal nätter med  pris per natt
                        decimal totalpris = antalNatter * prisPerNatt;
                        await using var skapaBokningSqlCommand =
                             new MySqlCommand(skapaBokningSql, connection);
                        // skickar kundens ID till MySQL
                        skapaBokningSqlCommand.Parameters.AddWithValue("@kundId", kundId);
                        skapaBokningSqlCommand.Parameters.AddWithValue("@rumId", rumId);
                        skapaBokningSqlCommand.Parameters.AddWithValue("@incheckning", bokningIncheckning);
                        skapaBokningSqlCommand.Parameters.AddWithValue("@utcheckning", bokningUtcheckning);
                        skapaBokningSqlCommand.Parameters.AddWithValue("@antalPersoner", bokningAntalPersoner);
                        // kör SQL-kommandot och sparar bokningen i databasen
                        int antalSparadeRader = await skapaBokningSqlCommand.ExecuteNonQueryAsync();
                        if (antalSparadeRader > 0)
                        {
                            // Hämtar bokningsnummer från den sparade bokningen
                            long bokningsnummer = skapaBokningSqlCommand.LastInsertedId;
                            Console.WriteLine("Bokningen har skapats!");
                            Console.WriteLine($"Bokningsnummer: {bokningsnummer}");
                            Console.WriteLine($"Antal nätter: {antalNatter}");
                            Console.WriteLine($"Pris per natt: {prisPerNatt:f2} kr");
                            Console.WriteLine($"Totalpris: {totalpris:F2} kr");
                        }
                        else
                        {
                            Console.WriteLine("Bokningen kunde inte sparas.");
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("Kunden finns inte. Välja ett ID från kundlistan.");
                    }

                }
                else
                {
                    Console.WriteLine("Ogiltigt kund-ID. Ange ett positvt heltal.");
                }
    }
}