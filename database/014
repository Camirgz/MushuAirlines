USE MushuAirlines;
GO

CREATE TABLE AirportNameHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AirportCode VARCHAR(3) NOT NULL,
    PreviousAirportName VARCHAR(200) NOT NULL,
    NewAirportName VARCHAR(200) NOT NULL,
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_AirportNameHistory_Airport
    FOREIGN KEY (AirportCode)
    REFERENCES Airport(Code)
);
