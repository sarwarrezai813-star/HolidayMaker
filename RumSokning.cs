
using MySqlConnector;

static class RumSokning
{
    // Läser sökvalen och visar lediga rum från databasen
    public static async Task SokLedigaRumAsync(MySqlConnection connection)
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
                    return;
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
                    return;
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
                    return;
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
                    return;
                }
                Console.Write("Kräver du pool? (j/n): ");
                // läser svaret tar bort extra mellanslag och gör om det till små bokstäver
                string? poolSvar = Console.ReadLine()?.Trim().ToLowerInvariant();
                // avbryter sökningen om svaret är något annant än j eller n
                if (poolSvar != "j" && poolSvar != "n")
                {
                    Console.WriteLine("Skriv j eller n.");
                    return;
                }
                // sparar om användaren kräver pool
                bool kravPaPool = poolSvar == "j";
                Console.Write("Kräver du restaurang? (j/n):");
                string? restaurangSvar = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (restaurangSvar != "j" && restaurangSvar != "n")
                {
                    Console.WriteLine("Skriv j eller n.");
                    return;
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
                        return;
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
                        return;
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
                        return;
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
    }
}