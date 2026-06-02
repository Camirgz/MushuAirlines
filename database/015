USE MushuAirlines;
GO

CREATE OR ALTER TRIGGER TR_Airport_UpdateName
ON Airport
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(AirportName)
    BEGIN
        INSERT INTO AirportNameHistory
        (
            AirportCode,
            PreviousAirportName,
            NewAirportName,
            UpdatedAt
        )
        SELECT
            d.Code,
            d.AirportName,
            i.AirportName,
            GETDATE()
        FROM inserted i
        INNER JOIN deleted d
            ON i.Code = d.Code
        WHERE LTRIM(RTRIM(i.AirportName)) <> LTRIM(RTRIM(d.AirportName));
    END
END;
GO


SELECT * FROM AirportNameHistory;
