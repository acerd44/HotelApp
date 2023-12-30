SELECT 
  * 
FROM 
  Guest;

SELECT 
  COUNT(*) AS AmountOfGuests 
FROM 
  Guest;

SELECT 
  g.Name AS [Guest Name], 
  i.TotalSum AS [Sum], 
  i.IsPaid AS [Invoice Paid off] 
FROM 
  Guest AS g 
  INNER JOIN Invoice AS i ON i.GuestId = g.Id;

SELECT 
  g.Name AS [Guest Name], 
  i.TotalSum AS [Sum], 
  i.PaidDate AS [Paid Date],
  i.IsPaid AS [Invoice Paid off] 
FROM 
  Guest AS g 
  INNER JOIN Invoice AS i ON i.GuestId = g.Id
WHERE
  i.TotalSum > 100;

SELECT 
  * 
FROM 
  Guest g 
  INNER JOIN Booking b ON b.GuestId = g.Id 
  INNER JOIN Invoice i on i.GuestId = g.Id;

SELECT 
  * 
FROM 
  Guest g 
  INNER JOIN Booking b ON b.GuestId = g.Id 
ORDER BY 
  g.Id;

SELECT 
  Address, 
  COUNT(Address) AS GuestsWithSameAddress 
FROM 
  Guest 
GROUP BY 
  [Address] 
ORDER BY 
  [GuestsWithSameAddress] DESC;

SELECT
  *
FROM
  Room
WHERE
  Beds = 2;

INSERT INTO Guest
VALUES ('Richard Chalk', '-', 321321123, 1)