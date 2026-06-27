USE MushuAirlines;
GO

IF EXISTS (
    SELECT 1
    FROM Airport
    WHERE Code = 'ZZZ'
)
BEGIN
    UPDATE Airport
    SET 
        AirportName = 'Aeropuerto Selenium Test',
        Country = 'Costa Rica',
        City = 'San José',
        IsDeleted = 0
    WHERE Code = 'ZZZ';
END
ELSE
BEGIN
    INSERT INTO Airport
    (
        Code,
        AirportName,
        Country,
        City,
        IsDeleted
    )
    VALUES
    (
        'ZZZ',
        'Aeropuerto Selenium Test',
        'Costa Rica',
        'San José',
        0
    );
END;
GO
