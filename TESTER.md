
# Testdokumentation - HolidayMaker

## Test 1 - Lediga och upptagna rum

- Genomförande: Jag kontrollerade i menyval 5 att bokning 26 gällde rum 102 på Hotel Continental. Sedan sökte jag i menyval 3 efter lediga rum i Halmstad för en person, med incheckning 2026-12.10 och utcheckning 2026-12-12.
- Resultat: Det bokade rummet filtreras bort och minst ett ledigt rum visas. Och rum 102 på Hotel Continental saknades i listan. Rum 101 på samma hotell visades som ledigt.
- Bedömning Godkänt.

## Test 2 - Incheckning samma dag som en annan kund checkar ut

- Bokning 26 hade utcheckning 2026-12-12 från rum 102 på Hotel Continental. I menyval 4 bokade jag samma rum (rum-ID 6) för kund 2, med incheckning 2026-12-12 och utcheckning 2026-12-13.
- Resultat programmet bekräftade bokning 29 för en natt.
- Bedömning: Godkänt.

## Test 3 - Överlappande bokningar

- I menyval 4 försökte jag boka rum 102 på Hotel Continental (rum-ID 6) för kund 2. Bokning 26 gällde 10-12 december 2026.
- Försök A: 11-12 december 2026. Den nya perioden överlappade en natt i bokning 26.
- Försök B: 9-13 december 2026. Den nya perioden omslöt hela bokning 26.
- Resultat: Programmet nekar både försöken samt visade att, rummet är redan bokat under dessa datum och återgick till huvudmenyn.
- Bedömning: Godkänt.

## Test 4 - För många personer i ett rum

- I menyval 4 försökte jag boka rum 101 på Hotel Continental (rum-ID 5) för två personer, 2026-10-12.
- Resultat: Programmet nekar bokningen för att antalet personer går över rummets kapacitat samt programmet visar rummet har plats för en personer och går tillbaka till huvudmeny.
- Bedömning: Godkänt.

## Test 5 - kombinerade sökfilter och sortering

- I menyval 3 sökte jag efter lediga rum i Halmstad för 1 person den 2026-10-12. Jag krävde restaurang och högst 500 meter till centrum. Jag körde samma sökning med lägsta pris först och sedan med högsta omdöme först.
- Resultat: Hotellen ska uppfylla både filtren. Priserna ska visas i stigande ordning vid prissortering och hotellen i fallande ordning efter omdöme vid omdömessortering. Både sökningarna visade samma 6 rum på Grand Hotel Halmstad och Hotel Continental. Både hotellen har restaurang och ligger högst 500 meter från centrum. Vid prissortering visades rum 101 på Hotel Continental först för 700 kr per natt och priserna steg till 1 700 kr per natt. Vid omdömessortering visades Hotel Halmstad (3,0) före Hotel Continental (2,7).
- Bedömning: Godkänt.

## Test 6 - Ändra antal personer

- I menyval 6 ändrade jag bokning 29 från 1 till 2 personer. Sedan försökte jag ändra till 3 personer. Rummet har plats för högst 2 personer.
- Resultat: Ändringen till 2 personer sparas och försöket med 3 personer nekas.
- Bedömning: Godkänt.

## Test 7 - Avbokning frigör rummet

- I menyval 3 sökte jag efter ledigt rum i Halmstad för 2 personer, med incheckning 2026-12-12 och utcheckning 2026-12-13. Sedan avbokade jag bokning 29 i menyval 7 och upprapade samma sökning.
- Resultat: Rum 102 på Hotel Continental (rum-ID 6) ska saknas före avbokningen och visas som ledigt efteråt. 
- Bedömning: Godkänt.

## Test 8 - Bokningen finns kvar efter omstart

- Jag visade bokning 26 för kund 1 i menyval 5. Sedan avslutade jag programmet med menyval 0, startade det igen `dotnet run` och visade kunden bokningar på nytt.
- Resultat: Bokning 26 visas med samma uppgifter efter omstarten.
- Bedömning: godkänt.
