
# Reflektion - Holiday Maker

## Datamodellen
I databasen finns fem tabeller: `ort`, `boende`, `rum`, `kund` och `bokning`.

- Ort: Lade orterna i en egen tabell för att ortnamnet bara lagras en gång. Ett boende kopplas till sin ort med `ort_id`. Till exampel kan flera hotell i Halmstad använda samma `ort_id`.
- Boende: Tabellen innehåller boendets namn, ort (`ort_id`),
om det har pool och restaurang, avstånd till strand och centrum samt omdöme. Eftersom de gäller hela boendet och inte varje rum.
- Rum: Rummet tillhör ett boende och rummets innhåller rummets ID, rumsnummer, max antal personer och pris per natt. Varje rum kopplas till sitt boende med `boende_id`,. Jag lade kapacitet och pris i tabellen `rum`, för att de kan skilja sig mellan rum på samma hotell.
- Kund: Tabellen innehåller kundens ID, förnamn, efternamn, epost och telefonnummer. Jag lade kunduppgifterna i en egen tabell eftersom en kund kan ha flera bokningar. 
- Bokning: Tabellen innehåller bokningsnummer, kund-ID, rum-ID, incheckningsdatum, utcheckningsdatumoch antal personer. Jag använder `kund_id` för att koppla bokningen till kunden och `rum_id`, för att koppla den till rummet. Datumen används för att kontrollera om rummet redan är bokat under den perioden.

## Säkerhet
- Jag har använd parametrar i SQL-frågorna för värden som (`@kund_id`), (`@or_id`t, osv.). Det skyddar mot SQL-injection.
- Sorteringen kan inte vara en parameter och därför väljs `ORDER BY`, från fasta texter.
- Lösenord till MySQL ligger i miljövariabeln `MYSQL_PASSWORD`, och inte i källkoden.
- Inmatninng kontrolleras med `TryParse` och `TryParseExact` så felaktig inmatning kraschar inte programmet.

## Resultat 
Jag genomförde åtta tester och dokumenterade i TESTER.md och alla åtta blev godkända. Programmet nekade överlappande bokningar och incheckning samma dag som den tidigare kund utcheckade.
## Möjliga förbättringar 
Om jag har haft mer tid då gjorde till mer automatisering i appen.

## Användning av AI 

Jag har använd AI som stöd under arbetet. AI som förklara ordning i uppgiften samt visa ställa av koden som jag har inte förståt fick hjälp av AI för förklaring.