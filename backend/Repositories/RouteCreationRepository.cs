using backend.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class RouteCreationRepository : IFlightRepository
    {
        private readonly string _connectionString;

        public RouteCreationRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString =
                builder.Configuration.GetConnectionString("LoginContext");
        }

        public RouteCreationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LoginContext");
        }

        public IEnumerable<RouteDbModel> GetAll(
            string date = null,
            string origin = null,
            string originType = null,
            string destination = null,
            string destinationType = null)
        {
            using var connection = new SqlConnection(_connectionString);
            bool hasLocation = origin != null && destination != null;

            if (!hasLocation && date == null)
            {
                return connection.Query<RouteDbModel>(@"
                    SELECT
                        r.*,
                        COALESCE(a.Model, r.AircraftTypeId) AS AircraftTypeId
                    FROM Route r
                    LEFT JOIN Aircraft a
                        ON r.AircraftCode = a.Code;
                ").ToList();
            }

            if (!hasLocation)
                return connection.Query<RouteDbModel>(@"
                    SELECT r.* FROM Route r
                    WHERE NOT EXISTS (
                        SELECT 1 FROM ScheduledFlight sf
                        JOIN Aircraft a ON sf.AircraftCode = a.Code
                        WHERE sf.RouteCode = r.Code
                          AND sf.DepartureDate = @Date
                          AND sf.BookedSeats >= (a.EconomyRows * a.EconomySeatsPerRow
                                               + a.FirstClassRows * a.FirstClassSeatsPerRow)
                    )", new { Date = date }).ToList();

            return connection.Query<RouteDbModel>(@"
                SELECT r.*
                FROM Route r
                JOIN Airport ao ON r.OriginAirport      = ao.Code
                JOIN Airport ad ON r.DestinationAirport = ad.Code
                WHERE
                  ((@OriginType = 'city'    AND ao.City         = @Origin)
                OR (@OriginType = 'airport' AND r.OriginAirport = @Origin))
                AND
                  ((@DestType   = 'city'    AND ad.City              = @Destination)
                OR (@DestType   = 'airport' AND r.DestinationAirport = @Destination))
                AND (@Date IS NULL OR NOT EXISTS (
                    SELECT 1 FROM ScheduledFlight sf
                    JOIN Aircraft a ON sf.AircraftCode = a.Code
                    WHERE sf.RouteCode = r.Code
                      AND sf.DepartureDate = @Date
                      AND sf.BookedSeats >= (a.EconomyRows * a.EconomySeatsPerRow
                                           + a.FirstClassRows * a.FirstClassSeatsPerRow)
                ))",
                new
                {
                    Origin = origin, OriginType = originType,
                    Destination = destination, DestType = destinationType,
                    Date = date
                }).ToList();
        }

        public void InsertRoute(RouteCreationModel route)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                INSERT INTO Route
                (
                    Code,
                    OriginAirport,
                    DestinationAirport,
                    DepartureTime,
                    ArrivalTime,
                    Duration,
                    AircraftTypeId,
                    AircraftCode,
                    Frequency,
                    PriceFirstClass,
                    PriceEconomy,
                    HandBagPrice,
                    HandBagWeight,
                    BagPrice,
                    BagWeight,
                    BagMultiplier,
                    StartDate,
                    FinalizationDate,
                    EconomyClassCapacity,
                    FirstClassCapacity,
                    OriginCity,
                    DestinationCity
                )
                SELECT
                    @Code,
                    @OriginAirport,
                    @DestinationAirport,
                    @DepartureTime,
                    @ArrivalTime,
                    @Duration,
                    a.Model,
                    a.Code,
                    @Frequency,
                    @PriceFirstClass,
                    @PriceEconomy,
                    @HandBagPrice,
                    @HandBagWeight,
                    @BagPrice,
                    @BagWeight,
                    @BagMultiplier,
                    @StartDate,
                    @FinalizationDate,
                    a.EconomyRows * a.EconomySeatsPerRow,
                    a.FirstClassRows * a.FirstClassSeatsPerRow,
                    @OriginCity,
                    @DestinationCity
                FROM Aircraft a
                WHERE a.Code = @AircraftCode
                AND a.IsDeleted = 0;
            ";

            int affectedRows = connection.Execute(query, new
            {
                route.Code,
                route.OriginAirport,
                route.DestinationAirport,
                route.DepartureTime,
                route.ArrivalTime,
                route.Duration,
                route.AircraftCode,
                Frequency = string.Join(",", route.Frequency),
                route.PriceFirstClass,
                route.PriceEconomy,
                route.HandBagPrice,
                route.HandBagWeight,
                route.BagPrice,
                route.BagWeight,
                route.BagMultiplier,
                route.StartDate,
                route.FinalizationDate,
                route.OriginCity,
                route.DestinationCity
            });

            if (affectedRows == 0)
            {
                throw new Exception("No se encontró la aeronave seleccionada.");
            }
        }

        public List<RouteDbModel> GetRoutes()
        {
            var query = @"
                SELECT
                    r.*,
                    COALESCE(a.Model, r.AircraftTypeId) AS AircraftTypeId
                FROM Route r
                LEFT JOIN Aircraft a
                    ON r.AircraftCode = a.Code;
            ";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<RouteDbModel>(query).ToList();
            }
        }

        public int GetOrCreateScheduledFlight(string routeCode, DateTime date)
        {
            RouteDbModel route = GetRouteByCode(routeCode);
            ValidateFlightDate(route, date);
            ValidateRouteDateRange(route, date);

            int? existingFlight = GetExistingScheduledFlight(routeCode, date);

            if (existingFlight.HasValue)
            {
                return existingFlight.Value;
            }

            int aircraftCode = route.AircraftCode;

            if (aircraftCode <= 0)
            {
                aircraftCode = GetAircraftCode(route.AircraftTypeId);
            }

            int flightScheduleId = CreateFlightSchedule(route, date);

            int scheduledFlightId = CreateScheduledFlight(
                routeCode,
                aircraftCode,
                date,
                route.Duration
            );

            LinkFlightSchedule(flightScheduleId, scheduledFlightId);

            return scheduledFlightId;
        }
        public int? FindExistingScheduledFlight(string routeCode, DateTime date)
            => GetExistingScheduledFlight(routeCode, date);

        private int? GetExistingScheduledFlight(string routeCode,DateTime date)
        {
           using var connection = new SqlConnection(_connectionString);

            return connection.QueryFirstOrDefault<int?>(@"
                SELECT dbo.GetExistingScheduledFlight(@RouteCode,@Date)",
                new
                {
                    RouteCode = routeCode,
                    Date = date.Date
                });
        }
        private void ValidateFlightDate(RouteDbModel route,DateTime date)
        {
            Dictionary<DayOfWeek, string> days =
                new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Monday, "Lunes" },
                { DayOfWeek.Tuesday, "Martes" },
                { DayOfWeek.Wednesday, "Miércoles" },
                { DayOfWeek.Thursday, "Jueves" },
                { DayOfWeek.Friday, "Viernes" },
                { DayOfWeek.Saturday, "Sábado" },
                { DayOfWeek.Sunday, "Domingo" }
            };

            string currentDay = days[date.DayOfWeek];

            List<string> routeDays =
                route.Frequency
                    .Split(',')
                    .Select(day => day.Trim())
                    .ToList();

            if (!routeDays.Contains(currentDay))
            {
                throw new Exception(
                    "This route does not operate on " + currentDay
                );
            }
        }

        private void ValidateRouteDateRange(RouteDbModel route,DateTime date)
        {
            DateTime startDate =
                DateTime.Parse(route.StartDate);

            DateTime finalizationDate =
                DateTime.Parse(route.FinalizationDate);

            if (date.Date < startDate.Date ||
                date.Date > finalizationDate.Date)
            {
                throw new Exception(
                    "The selected date is outside the route schedule."
                );
            }
        }
        public RouteDbModel GetRouteByCode(string routeCode)
        {
            using var connection = new SqlConnection(_connectionString);

            return connection.QuerySingle<RouteDbModel>(
                @"
                SELECT
                    r.*,
                    COALESCE(a.Model, r.AircraftTypeId) AS AircraftTypeId
                FROM Route r
                LEFT JOIN Aircraft a
                    ON r.AircraftCode = a.Code
                WHERE r.Code = @Code;
                ",
                new { Code = routeCode }
            );
        }
        private int GetAircraftCode(string aircraftType)
        {
            using var connection = new SqlConnection(_connectionString);

            int? aircraftCode = connection.QueryFirstOrDefault<int?>(@"SELECT dbo.GetAircraftCode(@Type)", new
            {
                Type = aircraftType
            });

            if (aircraftCode == null)
            {
                aircraftCode = connection.QueryFirstOrDefault<int?>(@"
                    SELECT TOP 1 Code
                    FROM   Aircraft
                    ORDER  BY Code");
            }

            if (aircraftCode == null)
            {
                throw new Exception(
                    $"No hay aeronaves de tipo '{aircraftType}' disponibles. " +
                    "Un administrador debe registrar aeronaves antes de que se puedan crear vuelos programados.");
            }

            return aircraftCode.Value;
        }
        private int CreateFlightSchedule(RouteDbModel route,DateTime date)
        {
            using var connection = new SqlConnection(_connectionString);

            int newId = GetNextFlightScheduleId();
            DateTime arrivalDate = CalculateArrivalDate(date,route.Duration);

            connection.Execute(@"
                INSERT INTO FlightSchedule(Id,OriginAirport,DestinationAirport,Duration,DepartureTime,ArrivalTime, DepartureDate,ArrivalDate)
                VALUES
                (
                    @Id,
                    @OriginAirport,
                    @DestinationAirport,
                    @Duration,
                    @DepartureTime,
                    @ArrivalTime,
                    @DepartureDate,
                    @ArrivalDate
                )",
                new
                {
                    Id = newId,
                    route.OriginAirport,
                    route.DestinationAirport,
                    route.Duration,
                    route.DepartureTime,
                    route.ArrivalTime,
                    DepartureDate = date,
                    ArrivalDate = arrivalDate
                });

            return newId;
        }
        private int CreateScheduledFlight(string routeCode,int aircraftCode, DateTime date,string duration)
        {
            using var connection = new SqlConnection(_connectionString);
            int newId = GetNextScheduledFlightId();
            DateTime arrivalDate = CalculateArrivalDate(date,duration);

            connection.Execute(@"
                INSERT INTO ScheduledFlight
                (Id,AircraftCode,Status,DepartureDate,ArrivalDate,BookedSeats,RouteCode)
                VALUES
                (
                    @Id,
                    @AircraftCode,
                    'Scheduled',
                    @DepartureDate,
                    @ArrivalDate,
                    0,
                    @RouteCode
                )",
                new
                {
                    Id = newId,
                    AircraftCode = aircraftCode,
                    RouteCode = routeCode,
                    DepartureDate = date,
                    ArrivalDate = arrivalDate
                });

            return newId;
        }
        private void LinkFlightSchedule(int flightScheduleId,int scheduledFlightId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute(@"
                INSERT INTO FlightScheduleHasScheduledFlight(FlightScheduleId,ScheduledFlightId)
                VALUES
                (
                    @FlightScheduleId,
                    @ScheduledFlightId
                )",
                new
                {
                    FlightScheduleId = flightScheduleId,
                    ScheduledFlightId = scheduledFlightId
                });
        }
        private int GetNextFlightScheduleId()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.QuerySingle<int>(@"
                SELECT ISNULL(MAX(Id), 0) + 1
                FROM FlightSchedule");
        }
        private int GetNextScheduledFlightId()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.QuerySingle<int>(@"
                SELECT ISNULL(MAX(Id), 0) + 1
                FROM ScheduledFlight");
        }
        private DateTime CalculateArrivalDate(DateTime departureDate,string duration)
        {
            string[] parts = duration.Split(':');
            TimeSpan durationTime = new TimeSpan(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                0
            );
            return departureDate.Add(durationTime);
        }
    }
}