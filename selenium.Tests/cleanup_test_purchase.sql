USE MushuAirlines;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @PurchaseId   INT  = (SELECT Id          FROM Purchase WHERE ReservationCode = 'SELENM');
DECLARE @BookingCode  INT  = (SELECT BookingCode FROM Purchase WHERE ReservationCode = 'SELENM');
DECLARE @PassengerId  INT  = (SELECT PassengerId FROM Purchase WHERE ReservationCode = 'SELENM');
DECLARE @ScheduledId  INT  = (SELECT TOP 1 Id   FROM ScheduledFlight WHERE RouteCode = 'SELENRTE');
-- @FlightSchedId no se usa; FlightSchedule se elimina por aeropuertos directamente
DECLARE @AircraftCode  INT  = (SELECT TOP 1 Code FROM Aircraft WHERE Model = 'Boeing 737 Selenium Test');
DECLARE @AircraftTypeId INT = (SELECT Id FROM AircraftType WHERE AircraftType = 'SeleniumTestType');

DELETE FROM ItineraryScheduledFlight
WHERE BookingCode = @BookingCode;

DELETE FROM TicketBaggage
WHERE BookingCode = @BookingCode;

DELETE FROM PurchaseBaggageDetail
WHERE PurchaseId = @PurchaseId;

DELETE FROM Ticket
WHERE ScheduledId   = @ScheduledId
  AND PassengerHas  = @PassengerId;

DELETE FROM PurchaseDetail
WHERE PurchaseId = @PurchaseId;

DELETE FROM Purchase
WHERE ReservationCode = 'SELENM';

DELETE FROM Itinerary
WHERE BookingCode = @BookingCode;

DELETE FROM Passenger
WHERE Id = @PassengerId;

DELETE FROM Person
WHERE Ssn = 'SELENIUMPRUEBA000001';

DELETE FROM [User]
WHERE Id = @PassengerId;

DELETE FROM FlightScheduleHasScheduledFlight
WHERE ScheduledFlightId = @ScheduledId;

DELETE FROM ScheduledFlight
WHERE RouteCode = 'SELENRTE';

DELETE FROM FlightSchedule
WHERE OriginAirport = 'XTS' AND DestinationAirport = 'XTD';

DELETE FROM Route
WHERE Code = 'SELENRTE';

DELETE FROM Aircraft
WHERE Code = @AircraftCode;

DELETE FROM AircraftType
WHERE Id = @AircraftTypeId;

DELETE FROM Airport
WHERE Code IN ('XTS', 'XTD');

COMMIT;

