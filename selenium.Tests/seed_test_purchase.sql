USE MushuAirlines;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM Purchase WHERE ReservationCode = 'SELENM')
OR EXISTS (SELECT 1 FROM Route    WHERE Code            = 'SELENRTE')
OR EXISTS (SELECT 1 FROM Airport  WHERE Code            IN ('XTS', 'XTD'))
BEGIN
    ROLLBACK;
    RAISERROR('Datos de prueba ya existen. Ejecute cleanup_test_purchase.sql primero.', 16, 1);
    RETURN;
END;

INSERT INTO Airport (Code, AirportName, Country, City)
VALUES ('XTS', 'Test Source Airport',      'Test Country', 'Test City A');

INSERT INTO Airport (Code, AirportName, Country, City)
VALUES ('XTD', 'Test Destination Airport', 'Test Country', 'Test City B');

DECLARE @AircraftTypeId INT;
SELECT @AircraftTypeId = ISNULL(MAX(Id), 0) + 1 FROM AircraftType;

INSERT INTO AircraftType (Id, AircraftType)
VALUES (@AircraftTypeId, 'SeleniumTestType');

DECLARE @AircraftCode INT;
SELECT @AircraftCode = ISNULL(MAX(Code), 0) + 1 FROM Aircraft;

INSERT INTO Aircraft
    (Code, Model, [Type], MaxTakeOffWeight,
     EconomyRows, EconomySeatsPerRow, FirstClassRows, FirstClassSeatsPerRow)
VALUES
    (@AircraftCode, 'Boeing 737 Selenium Test', @AircraftTypeId, 79015.0,
     25, 6, 3, 4);

INSERT INTO Route (
    Code, OriginAirport, DestinationAirport,
    DepartureTime, ArrivalTime, Duration, AircraftTypeId,
    Frequency,
    PriceFirstClass, PriceEconomy,
    HandBagPrice, HandBagWeight,
    BagPrice, BagWeight, BagMultiplier,
    StartDate, FinalizationDate,
    EconomyClassCapacity, FirstClassCapacity,
    OriginCity, DestinationCity
)
VALUES (
    'SELENRTE', 'XTS', 'XTD',
    '08:00', '12:00', '04:00', 'SeleniumTestType',
    'Lunes,Martes,Miércoles,Jueves,Viernes,Sábado,Domingo',
    300.00, 150.00,
    15.00, 10.00,
    35.00, 23.00, 1.5,
    CAST(GETDATE() AS DATE), CAST(DATEADD(year, 2, GETDATE()) AS DATE),
    150, 50,
    'Test City A', 'Test City B'
);

DECLARE @FlightScheduleId INT;
SELECT @FlightScheduleId = ISNULL(MAX(Id), 0) + 1 FROM FlightSchedule;

DECLARE @DepartureDay DATE = CAST(DATEADD(day, 90, GETDATE()) AS DATE);

INSERT INTO FlightSchedule
    (Id, OriginAirport, DestinationAirport, Duration,
     DepartureTime, ArrivalTime, DepartureDate, ArrivalDate)
VALUES
    (@FlightScheduleId, 'XTS', 'XTD', '04:00',
     '08:00:00', '12:00:00',
     @DepartureDay, @DepartureDay);

DECLARE @ScheduledId INT;
SELECT @ScheduledId = ISNULL(MAX(Id), 0) + 1 FROM ScheduledFlight;

INSERT INTO ScheduledFlight
    (Id, AircraftCode, Status, DepartureDate, ArrivalDate, BookedSeats, RouteCode)
VALUES
    (@ScheduledId, @AircraftCode, 'Scheduled',
     CAST(@DepartureDay AS DATETIME) + CAST('08:00:00' AS DATETIME),
     CAST(@DepartureDay AS DATETIME) + CAST('12:00:00' AS DATETIME),
     0, 'SELENRTE');

INSERT INTO FlightScheduleHasScheduledFlight (FlightScheduleId, ScheduledFlightId)
VALUES (@FlightScheduleId, @ScheduledId);

INSERT INTO [User] DEFAULT VALUES;
DECLARE @UserId INT = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Person (Id, FirstName, LastName, Ssn, Nationality, BirthDate)
VALUES (@UserId, 'Test', 'Selenium', 'SELENIUMPRUEBA000001', 'CR', '1990-06-15');

INSERT INTO Passenger (Id) VALUES (@UserId);

DECLARE @BookingCode INT;
SELECT @BookingCode = ISNULL(MAX(BookingCode), 0) + 1
FROM Itinerary WITH (UPDLOCK, HOLDLOCK);

INSERT INTO Itinerary (BookingCode, PassengerBooks)
VALUES (@BookingCode, @UserId);

INSERT INTO Purchase
    (PassengerId, BookingCode, ReservationCode, InvoiceNumber,
     PaymentMethod, Email, TotalPaid, TotalSeats, PurchaseDate)
VALUES
    (@UserId, @BookingCode, 'SELENM', 'MA-TEST-SELENM-0001',
     'Visa', 'selenium.test@mushu.test', 150.00, 1, GETDATE());

DECLARE @PurchaseId INT = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO PurchaseDetail (PurchaseId, SeatClass, SeatCount, Subtotal)
VALUES (@PurchaseId, 'Economy', 1, 150.00);

INSERT INTO Ticket (ScheduledId, PassengerHas, SeatNumber, SeatClass)
VALUES (@ScheduledId, @UserId, 1, 'Economy');

INSERT INTO TicketBaggage
    (ScheduledFlightId, PassengerId, BookingCode, HandBagCount, CheckedBagCount, BaggageSubtotal)
VALUES
    (@ScheduledId, @UserId, @BookingCode, 0, 0, 0.00);

INSERT INTO ItineraryScheduledFlight (ScheduledId, BookingCode)
VALUES (@ScheduledId, @BookingCode);

COMMIT;
