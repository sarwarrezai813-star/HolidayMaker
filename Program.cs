
// Gör att vi kan använda MySQL-klasser i C#
using System.Diagnostics.Metrics;
using MySqlConnector;

// Hämtar anslutningssträngen från Database.cs och skapar anslutningen
await using var connection = new MySqlConnection(Database.HamtaConnectionString());
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
                // Visar kundlistaan med den öppna databasanslutningen
                await KundHantering.VisaAllaKunderAsync(connection);
                break;
            }
        case "2":
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
                    break;
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

                break;
            }
            case "3":
            {
                Console.WriteLine("Du valde att söka lediga rum.");
                // Visar vilka orter kunden kan välja mellan
                Console.WriteLine("1. Halmstad");
                Console.WriteLine("2. Göteborg");
                // Kunden får välja ort
                Console.Write("Välja ort (1 eller 2)");
                string? ortInput = Console.ReadLine();
                if (ortInput != "1" && ortInput != "2")
                {
                    Console.WriteLine("Ogiltigt ort. Välja 1 eller 2.");
                    break;
                }
                // Omvandlar kundens ortval från text till heltal
                int ortId = int.Parse(ortInput!);
                // hur många personer vill boka rummet
                Console.Write("Ange antal personer: ");
                // Här läser kundens svar
                string? antalInput = Console.ReadLine();
                // kontroll att användare skriv giltigt heltal samt större än null
                bool giltigtAntal = int.TryParse(antalInput, out int antalPersoner)
                && antalPersoner > 0;
                if (!giltigtAntal)
                {
                    Console.WriteLine("Ogiltigt antal personer. Ange ett positvt heltal.");
                    break;
                }
                // frågar kunden om incheckningsdatum
                Console.Write("Ange incheckningsdatum (ÅÅÅÅ-MM-DD): ");
                string? incheckningInput = Console.ReadLine();
                // Frågar kunden om utcheckningsdatum
                Console.Write("Ange utcheckningsdatum (ÅÅÅÅ-MM-DD): ");
                string? utcheckningInput = Console.ReadLine();
                // kontroll att kunden har skrivit giltigt incheckningsdatum
                bool giltigIncheckning = DateTime.TryParseExact(
                    incheckningInput,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime incheckning);
                if (!giltigIncheckning)
                {
                    Console.WriteLine("Ogiltigt incheckningsdatum. Använd ÅÅÅÅ-MM-DD.");
                    break;
                }
                // kontroll för utcheckningsdatum
                bool giltigUtcheckning = DateTime.TryParseExact(
                    utcheckningInput,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime utcheckning);
                if (!giltigUtcheckning || utcheckning <= incheckning)
                {
                    Console.WriteLine("Ogiltigt utcheckningsdatum. Det måste vara efter incheckningen.");
                    break;
                }
                Console.Write("Kräver du pool? (j/n): ");
                // läser svaret tar bort extra mellanslag och gör om det till små bokstäver
                string? poolSvar = Console.ReadLine()?.Trim().ToLowerInvariant();
                // avbryter sökningen om svaret är något annant än j eller n
                if (poolSvar != "j" && poolSvar != "n")
                {
                    Console.WriteLine("Skriv j eller n.");
                    break;
                }
                // sparar om användaren kräver pool
                bool kravPaPool = poolSvar == "j";
                Console.Write("Kräver du restaurang? (j/n):");
                string? restaurangSvar = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (restaurangSvar != "j" && restaurangSvar != "n")
                {
                    Console.WriteLine("Skriv j eller n.");
                    break;
                }
                bool kravPaRestaurang = restaurangSvar == "j";
                Console.Write("Maxavstånd till strand i meter (Enter för att hoppa över): ");
                string? strandSvar = Console.ReadLine();
                int? maxStrandMeter = null;
                if (!string.IsNullOrWhiteSpace(strandSvar))
                {
                    if (!int.TryParse(strandSvar, out int meter) || meter < 0)
                    {
                        Console.WriteLine("Ange ett helta från 0 och uppåt.");
                        break;
                    }
                     maxStrandMeter = meter;
                }
                Console.Write("Maxavstånd till centrum i meter (Enter för att hoppa över): ");
                string? centrumSvar = Console.ReadLine();
                int? maxCentrumMeter = null;
                if (!string.IsNullOrWhiteSpace(centrumSvar))
                {
                    if (!int.TryParse(centrumSvar, out int centrumMeter) || centrumMeter < 0)
                    {
                        Console.WriteLine("Ange ett heltal från 0 och uppåt.");
                        break;
                    }
                    maxCentrumMeter = centrumMeter;
                }
                Console.Write("Minsta omdöme (Enter för att hoppa över): ");
                string? omdomeSvar = Console.ReadLine()?.Trim();
                decimal? minOmdome = null;
                if (!string.IsNullOrWhiteSpace(omdomeSvar))
                {
                    if (!decimal.TryParse(omdomeSvar.Replace(',','.'),
                    System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal omdome))
                    {
                        Console.WriteLine("Ange ett giltigt omdöme, till exempel 4,5.");
                        break;
                    }
                    minOmdome = omdome;
                }
                // Visar hur sökresultatet kan sorteras
                Console.WriteLine("1. lägsta pris först");
                Console.WriteLine("2. Högsta omdöme först.");
                Console.WriteLine("3. Närmast stranden först.");
                Console.WriteLine("4. Närmast centrum först.");
                Console.Write("Välja sortering (Enter för lägsta pris): ");
                string? sorteringsVal = Console.ReadLine()?.Trim();
                // Översätter menynvalet till ett av våra fasta SQL-vilkor
                string sortering = sorteringsVal switch
                {
                    "2" => "bo.omdome DESC",
                    "3" => "bo.avstand_strand_m ASC",
                    "4" => "bo.avstand_centrum_m ASC",
                    _ => "r.pris_per_natt ASC"
                };
                
                string rumSql = $"""
                    SELECT r.id, r.boende_id, r.rumsnummer, r.max_personer, r.pris_per_natt,
                    bo.namn AS boende_namn, o.namn AS ort_namn
                    FROM rum r
                    JOIN boende bo ON r.boende_id = bo.id
                    JOIN ort o ON bo.ort_id = o.id
                    WHERE r.max_personer >= @antalPersoner
                    AND bo.ort_id = @ortId
                    AND (@kravPaPool = 0 OR bo.har_pool = 1)
                    AND (@kravPaRestaurang = 0 OR bo.har_restaurang = 1)
                    AND (@maxStrandMeter IS NULL OR bo.avstand_strand_m <= @maxStrandMeter)
                    AND (@maxCentrumMeter IS NULL OR bo.avstand_centrum_m <= @maxCentrumMeter)
                    AND (@minOmdome IS NULL OR bo.omdome >= @minOmdome)
                    AND NOT EXISTS (
                    SELECT 1
                    FROM bokning b
                    WHERE b.rum_id = r.id
                    AND b.incheckning < @utcheckning
                    AND b.utcheckning > @incheckning
                    )
                    ORDER BY {sortering};
                    """;
                await using var rumCommand = new MySqlCommand(rumSql, connection);
                rumCommand.Parameters.AddWithValue("@antalPersoner", antalPersoner);
                rumCommand.Parameters.AddWithValue("@ortId", ortId);
                rumCommand.Parameters.AddWithValue("@incheckning", incheckning);
                rumCommand.Parameters.AddWithValue("@utcheckning", utcheckning);
                rumCommand.Parameters.AddWithValue("@kravPaPool", kravPaPool);
                rumCommand.Parameters.AddWithValue("@kravPaRestaurang", kravPaRestaurang);
                rumCommand.Parameters.AddWithValue("@maxStrandMeter", (object?)maxStrandMeter ?? DBNull.Value);
                rumCommand.Parameters.AddWithValue("@maxCentrumMeter", (object?)maxCentrumMeter ?? DBNull.Value);
                rumCommand.Parameters.AddWithValue("@minOmdome", (object?)minOmdome ?? DBNull.Value);
                await using var rumReader = await rumCommand.ExecuteReaderAsync();
                while (await rumReader.ReadAsync())
                {
                    // Hämtar boendes namn från databasen
                    string boendeNamn = rumReader.GetString(rumReader.GetOrdinal("boende_namn"));
                    // Hämtar ortens namn från databasen
                    string ortNamn = rumReader.GetString(rumReader.GetOrdinal("ort_namn"));
                    // Hämtar hur många personer som får bo i rummet
                    int kapacitet = rumReader.GetInt32(rumReader.GetOrdinal("max_personer"));
                    // Hämtar rummets ID från databasen
                    int rumId = rumReader.GetInt32(rumReader.GetOrdinal("id"));
                    // Hämtar rummets nummer från databasen
                    string rumsnummer = rumReader.GetString(rumReader.GetOrdinal("rumsnummer"));
                    // Hämtar rummets pris per natt
                    decimal prisPerNatt = rumReader.GetDecimal(rumReader.GetOrdinal("pris_per_natt"));
                    // visar rummets ID, nummer och pris per natt
                    Console.WriteLine($"Rum-ID: {rumId} | Boende: {boendeNamn} | Ort: {ortNamn} | Rum: {rumsnummer} | Kapacitet: {kapacitet} personer | Pris: {prisPerNatt} kr/natt");

                }
                // stänger läsningen av rum från MySQL
                await rumReader.CloseAsync();
                break;
            }
        case "4":
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
                            break;
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
                            break;
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
                            break;
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
                            break;
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
                            break;
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
                            break;
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
                            break;
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

