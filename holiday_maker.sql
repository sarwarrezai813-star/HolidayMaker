
-- Skapar databasen för vårt bokningssystem.
CREATE DATABASE holiday_maker;

USE holiday_maker;

-- Skapar en tabell för att lagra olika orter.
CREATE TABLE ort (
     id INT AUTO_INCREMENT PRIMARY KEY,
     namn VARCHAR(100) NOT NULL UNIQUE -- Samma ortnamn får inte registreras två gånger.
);

-- Tabellen för hotell och andra boenden.
CREATE TABLE boende (
    id INT AUTO_INCREMENT PRIMARY KEY,
    namn VARCHAR(100) NOT NULL,
    ort_id INT NOT NULL,
    har_pool BOOLEAN NOT NULL DEFAULT FALSE, -- Visar om boendet har pool,
    har_restaurang BOOLEAN NOT NULL DEFAULT FALSE, -- och restaurang.
    avstand_strand_m INT NOT NULL, -- Sparar avstånd i meter.
    avstand_centrum_m INT NOT NULL,
    omdome DECIMAL(2,1) NOT NULL, -- Sparar boendets omdöme mellan 1 och 5.
    CONSTRAINT fk_boende_ort
    FOREIGN KEY (ort_id) REFERENCES ort(id), -- Kopplar boendet till en ort.
    CONSTRAINT chk_strand CHECK (avstand_strand_m >= 0), -- Kontrollerar att avstånd inte,
    CONSTRAINT chk_centrum CHECK (avstand_centrum_m >= 0), -- är negativt.
    CONSTRAINT chk_omdome CHECK (omdome BETWEEN 1 AND 5), -- Kontrollerar omdöme är mellan 1 och 5.
    CONSTRAINT uq_boende_namn_ort UNIQUE (namn, ort_id) -- Hindrar samma hotellnamn på samma ort  
);

-- Skapar tabellen för hotellrum.
CREATE TABLE rum (
   id INT AUTO_INCREMENT PRIMARY KEY,
   boende_id INT NOT NULL,
   rumsnummer VARCHAR(10) NOT NULL, -- Sparar rumsnummer både siffror och bokstäver.
   max_personer INT NOT NULL,
   pris_per_natt DECIMAL(10,2) NOT NULL ,
   CONSTRAINT fk_rum_boende 
   FOREIGN KEY (boende_id) REFERENCES boende(id), -- kopplar varje rum till ett befintligt hotell.
   CONSTRAINT chk_max_personer CHECK (max_personer >= 1), -- rummet måste ha plats minst 1 person.
   CONSTRAINT chk_pris_per_natt CHECK (pris_per_natt > 0), --  pris per natt måste vara större än 0.
   CONSTRAINT uq_rum_boende_nummer UNIQUE (boende_id, rumsnummer) -- Hindrar dubbla rumsnummer på samma hotel.
);

-- Skapar tabellen för att registrera kunder.
CREATE TABLE kund (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fornamn VARCHAR(100) NOT NULL,
    efternamn VARCHAR(100) NOT NULL,
    epost VARCHAR(150) NOT NULL UNIQUE, -- samma kund inte registeras två gånger.
    telefon VARCHAR(20) NOT NULL
);

-- Denna tabell sparar hotellbokningar.
CREATE TABLE bokning (
    id INT AUTO_INCREMENT PRIMARY KEY, -- Ger varje bokning ett unikt id AUTO.
    kund_id INT NOT NULL, -- visar vilken kund har gjort bokningen.
    rum_id INT NOT NULL, -- visar vilken hotellrum kunden har bokat.
    incheckning DATE NOT NULL, -- Datum för kunden checkar in h.
    utcheckning DATE NOT NULL, -- Datum då kunden checkar ut.
    antal_personer INT NOT NULL, -- antal personer som bor på rummet.
    CONSTRAINT fk_bokning_kund
    FOREIGN KEY (kund_id) REFERENCES kund(id), -- kopplar bokningen till en befintlig kund.
    CONSTRAINT fk_bokning_rum
    FOREIGN KEY (rum_id) REFERENCES rum(id), -- samma här.
    CONSTRAINT chk_bokning_datum CHECK (utcheckning > incheckning), 
    CONSTRAINT chk_bokning_personer CHECK (antal_personer >= 1) -- en bokning måste innehålla minst en p.
    
);

-- Lägger till två orter i databasen.
INSERT INTO ort (namn)
VALUES ('Halmstad'), ('Göteborg');

-- Lägger till fem hotell i databaseen.
INSERT INTO boende (namn, ort_id, har_pool, har_restaurang, avstand_strand_m, avstand_centrum_m, omdome)
--        namn               ort   pool  rest   strand cetr     omdöm
VALUES ( 'Hotell Tylösand',     1,  TRUE,  TRUE,  100,  9000,  4.5),
      (  'Hotel Continental',    1,  FALSE,  TRUE,  1500, 300,   2.7),
      (  'Grand Hotel Halmstad',  1,  FALSE, TRUE,  2000,  500,   3.0),
      --   Hotell i Göteborg.
      ('Gothia Towers',            2,  TRUE,  FALSE,  12000,  2000,  4.6),
      ( 'Hotel Riverton',          2,  FALSE,  TRUE,  9000,  1000,   4.4);

-- Lägger till 20 rum i databasen.
INSERT INTO rum (boende_id, rumsnummer, max_personer, pris_per_natt)
VALUES 
-- Hotell Tylösand ID 1.
       (1, '101', 1, 900.00),
       (1, '102', 2, 1200.00),
       (1, '103', 4, 1800.00),
       (1, '12B', 2, 1400.00),
       --  Hotel Continental ID 2.
       (2, '101', 1, 700.00), 
       (2, '102', 2, 950.00),
       (2, '103', 4, 1500.00),
       (2, '104', 2, 1100.00),
       --   Grand Hotel Halmstad, ID 3.
       (3, '201', 1, 800.00), 
       (3, '202', 2, 1100.00),
       (3, '203', 3, 1700.00),
       (3, '204', 2, 1250.00),
      --   Gothia Towers, ID 4.
       (4, '301', 2, 1000.00),  
       (4, '302', 3, 1500.00),
       (4, '303', 2, 1150.00),
       (4, '304', 4, 1700.00),
    --   Hotel Riverton ID 5.
       (5, '401', 1, 750.00),  
       (5, '402', 3, 2000.00),
       (5, '403', 2, 1350.00),
       (5, '404', 4, 2500.00); 

-- Lägger till fem testkunder i databasen.
INSERT INTO kund (fornamn, efternamn, epost, telefon)
VALUES 
('Jesper', 'Ström',  'jesper@example.com', '0705556677'),
('Bella', 'Andersson', 'bella@example.com', '0701112233'),
('Sara', 'Johansson',  'sara@example.com',  '0702223344'),
('Emma', 'Nilsson',  'emma@example.com', '0703334455'),
('David', 'Karlsson', 'david@example.com', '0704445566');

INSERT INTO bokning (kund_id, rum_id, incheckning, utcheckning, antal_personer)
VALUES 
-- kund_id   rum_id incheck     utcheck     antal_personer.
          
         (1,     1, '2026-12-10', '2026-12-13', 1),  -- Jesper, kund 1
         (1,     2, '2026-12-14', '2026-12-17', 2),
         (1,     5, '2026-12-18', '2026-12-20', 1),
         (1,     9, '2026-12-21', '2026-12-24', 1),
         (1,    13, '2026-12-26', '2026-12-29', 2),
         
         (2,    3, '2026-12-10', '2026-12-13',  3),  -- Bella, kund 2
         (2,    6, '2026-12-14', '2026-12-17',  2),
         (2,   10, '2026-12-18', '2026-12-21',  2),
         (2,   14, '2026-12-22', '2026-12-25',  3),
         (2,  17, '2026-12-26', '2026-12-29',   1),
         
         (3,  4, '2026-12-10',  '2026-12-12',   2), -- Sara kund 3
         (3,  7,  '2026-12-13', '2026-12-20',   4),
         (3, 11, '2026-12-17',  '2026-12-20',   3),
         (3, 15, '2026-12-21',  '2026-12-24',   2),
         (3, 18, '2026-12-26',  '2026-12-29',   3),
         
         (4, 1,  '2026-12-14',  '2026-12-17',   1),  -- Emma kund 4
         (4, 8,   '2026-12-10',  '2026-12-13',   2),
         (4, 12, '2026-12-14',  '2026-12-15',   2),
         (4, 16, '2026-12-18',  '2026-12-21',   4),
         (4, 19, '2026-12-22',  '2026-12-25',   2),
         
         (5,  2, '2026-12-10',  '2026-12-13',   2),  -- David kund 5
         (5,  5, '2026-12-14',  '2026-12-17',   1),
         (5,  9, '2026-12-18',  '2026-12-21',   1),
         (5, 13, '2026-12-22',  '2026-12-25',   2),
         (5, 20, '2026-12-18',  '2026-12-21',   4);
    
    -- Visar alla rum på hotell i Halmstad.
   SELECT r.rumsnummer, b.namn AS hotell, o.namn AS stad, r.max_personer, r.pris_per_natt
   FROM rum r
   JOIN boende b ON r.boende_id = b.id
   JOIN ort o ON b.ort_id = o.id
   WHERE o.namn = 'Halmstad';
    
 --  Visar lediga rum i Halmstad mellan 10 och 13 december.
    SELECT r.rumsnummer, b.namn AS hotell, r.pris_per_natt
    FROM rum r
    JOIN boende b ON r.boende_id = b.id 
    JOIN ort o ON b.ort_id = o.id 
    WHERE o.namn = 'Halmstad'
    AND r.max_personer >= 2
    AND NOT EXISTS (
      SELECT 1 
      FROM bokning bk 
      WHERE bk.rum_id = r.id 
      AND bk.incheckning < '2026-12-13'
      AND bk.utcheckning > '2026-12-10'
    );
 
 SELECT r.rumsnummer, b.namn AS hotell, r.pris_per_natt
    FROM rum r
    JOIN boende b ON r.boende_id = b.id 
    JOIN ort o ON b.ort_id = o.id 
    WHERE o.namn = 'Halmstad'
    AND r.max_personer >= 1
    AND NOT EXISTS (
      SELECT 1 
      FROM bokning bk 
      WHERE bk.rum_id = r.id 
      AND bk.incheckning < '2026-12-14'
      AND bk.utcheckning > '2026-12-13'
    );
 
 -- Visar att hotell har pool
    SELECT namn, har_pool
    FROM boende
    WHERE har_pool = 1;
 
 -- Hotell som har restaurang
   SELECT namn, har_restaurang
   FROM boende
   WHERE har_restaurang = 1;
 
 -- Hämtar rumsnummer hotellnamn och pris
    SELECT r.rumsnummer, b.namn AS hotell, r.pris_per_natt
    FROM rum r
    JOIN boende b ON r.boende_id = b.id
    ORDER BY r.pris_per_natt ASC;
 
 -- Visar hotellets namn och omdöme
    SELECT namn, omdome
    FROM boende
    ORDER BY omdome DESC;
 
 -- Hämtar bokning som gäller samma rum i samma tid
  SELECT b1.id, b2.id, b1.rum_id
  FROM bokning b1
  JOIN bokning b2
  ON b1.rum_id = b2.rum_id 
  AND b1.id < b2.id 
  WHERE b1.incheckning < b2.utcheckning 
  AND b1.utcheckning > b2.incheckning ;
  
 -- Här ser vi ingen bokning har fler personer än rummet tillåter
  SELECT b.id, b.antal_personer, r.max_personer
  FROM bokning b
  JOIN rum r ON b.rum_id = r.id 
  WHERE b.antal_personer > r.max_personer;
  
 -- Visar bokningsnummer och antal nätter och pris för varje natt samt totalpris
  SELECT b.id, DATEDIFF(b.utcheckning, b.incheckning) AS antal_natter,
  r.pris_per_natt, DATEDIFF(b.utcheckning, b.incheckning) * r.pris_per_natt AS totalpris
  FROM bokning b 
  JOIN rum r ON b.rum_id = r.id;
  
  SELECT k.fornamn, k.efternamn, r.rumsnummer, bo.namn AS hotell, o.namn AS stad
  FROM bokning b 
  JOIN kund k ON b.kund_id = k.id
  JOIN rum r ON b.rum_id = r.id 
  JOIN boende bo ON r.boende_id = bo.id 
  JOIN ort o ON bo.ort_id = o.id;
  
  SELECT o.namn AS ort, bo.namn AS boende, bo.har_restaurang
  FROM boende bo
  JOIN ort o ON bo.ort_id = o.id 
  ORDER BY o.namn, bo.namn;

SELECT namn, omdome
FROM boende
ORDER BY omdome DESC;

WHERE max_personer >= 4
AND pris_per_natt <=1800;

SHOW COLUMNS FROM rum;

SELECT id, boende_id, rumsnummer
FROM rum

DESCRIBE kund;