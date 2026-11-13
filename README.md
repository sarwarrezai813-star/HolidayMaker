
# Holiday Maker

HolidayMaker är ett C#-konslapplikation som använder MySQL för att hantera kunder, rum och bokningar.
Du kan se alla kunder, registrera kunder, söka efter lediga rum med filter och sortering, skapa och visa bokningar, ändra antal personer och avboka.

Skriptet skapar databasen `holiday_maker` med tabellerna `ort`, `boende`, `rum`, `kund`, och `bokning`, och lägger in terstdata.
- 5 boende på 2 orter (Halmstad, Göteborg)
- 20 rum med kapacitet 1-4 personer
- 5 kunder
- 25 bokningar i december 2026 

## Förutsättningar
- .NET 10 SDK för att bygga och köra programmet. Projektet använder `net10.0`.
- MySQL på `localhost:3306`
- DBeaver för att köra `holiday_maker.sql`

Starta MySQL på `localhost:3306`. Om du använder Docker Desktop, starta din MySQL-container.
Kör hela skriptet `holiday_maker.sql` i DBeaver vid första installationen.
 Öppna en terminal i mappen där `HolidayMaker.csproj` ligger.
Kör följande kommando i nedan zsh. Skriv lösenord för MySQL-användare `root` när du får frågan och tryck Enter. Tecknen är dolda medan du skriver.

```zsh
read -rs 'MYSQL_PASSWORD?Ange ditt MySQL-lösenord: '
```

Kör sedan i samma terminal:
```zsh
export MYSQL_PASSWORD
dotnet run
```

## Menyval

| val  | Funktion |
| ---  | ---  |
| 1.   | Visa alla kunder |
| 2.   | Registrera ny kund |
| 3    | Sök lediga rum (ort, datum, antal personer,    filter och sortering)|
| 4.   | Skapa en bokning |
| 5.   | Visa kundens bokningar |
| 6.   | Ändra antal personer i en bokning |
| 7.   | Avboka |
| 0.   | Avsluta programmet |

Datum anges i formatet `ÅÅÅÅ-MM-DD`, t.ex. `2026-12-10`.

## Projektfiler 
| Fil  | Vad filen innehåller |
| ---  | --- |
| [Program.cs](Program.cs) | C#-koden för menyn, rumssökningen och hanteringen av kunder och bokningar. |
| [HolidayMaker.csproj](HolidayMaker.csproj) | Projektets inställningar, bland annat .NET-versionen och paketet MySqlConnector. |
| [holiday_maker.sql](holiday_maker.sql) | SQL-skriptet som skapar databasen, tabellerna och lägger in testdata. |
| [README.md](README.md) | Instruktioner för att starta och använda programmet. |
| [ER-diagram](<holiday_maker copy.png>) | Visar hur tabellerna i databasen hänger ihop. 
| [TESTER.md](TESTER.md) | De åtta manuella testerna och deras resultat. |
| [REFLEKTION.nd](REFLEKTION.md) | Minreflektion om datamodellen, säkerheten, resultat, möliga förbättring osv. |

## ER-diagram 
![ER-diagram för HolidayMaker](<holiday_maker copy.png>)


