
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
                // Visar kundens bokningar med den öppna databasanslutningen
                await BokningHantering.VisaKundensBokningarAsync(connection);
                break;
            }
            // Ändra antal personer
            case "6":
            {
                // kör metoden med den öppna databasanslutningen
                await BokningHantering.AndraAntalPersonerAsync(connection);
               
                break;
            }
            // för avbokning
            case "7":
            {
                // Kör avbokningen med den öppna databasanslutningen
                await BokningHantering.AvbokaAsync(connection);

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

