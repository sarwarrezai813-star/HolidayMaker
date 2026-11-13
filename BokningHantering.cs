
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
    // Hämtar och visar bokningar för den valde kunden
    public static async Task VisaKundensBokningarAsync(MySqlConnection connection)
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
                    return;
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
    }
    // Ändrar antalet personer om det nya antalet ryms i rummet
    public static async Task AndraAntalPersonerAsync(MySqlConnection connection)
    {
        Console.Write("Ange bokningsnummer: ");
                string? bokningsnummerInput = Console.ReadLine();
                if (!int.TryParse(bokningsnummerInput, out int bokningsnummer) || bokningsnummer <= 0)
                {
                     Console.WriteLine("Ogiltigt bokningsnummer.");
                     return;
                }
                Console.Write("Ange nytt antal personer: ");
                string? antalPersonerInput = Console.ReadLine();
                if (!int.TryParse(antalPersonerInput, out int nyttAntalPersoner) || nyttAntalPersoner <= 0)
                {
                    Console.WriteLine("Ogiltigt antal personer.");
                    return;
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
                    return;
                }
                // Gör om värdet från databasen till ett heltal
                int maxPersoner = Convert.ToInt32(maxPersonerResultat);
                if (nyttAntalPersoner > maxPersoner)
                {
                    Console.WriteLine($"Rummet rymmer högst {maxPersoner} personer.");
                    return;
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
    }
    // Avbokar bokningen med det angivna bokningsnummret
    public static async Task AvbokaAsync(MySqlConnection connection)
    {
        Console.Write("Ange bokningsnummer att avboka: ");
                string? avbokningsnummerInpuut = Console.ReadLine();
                // kontroll för bokningsnumret är positvit heltal
                if (!int.TryParse(avbokningsnummerInpuut, out int avbokningsnummer) || avbokningsnummer <= 0)
                {
                    Console.WriteLine("Ogiltigt bokningsnummer.");
                    return;
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
    }
}