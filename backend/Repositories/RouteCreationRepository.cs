using backend.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using backend.Interfaces;
using System.Data;

namespace backend.Repositories
{
    public class RouteCreationRepository : IFlightRepository, IRouteCreationRepository
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

            string baseSelect = @"
                SELECT
                    r.Code,
                    r.OriginAirport,
                    r.DestinationAirport,
                    r.DepartureTime,
                    r.ArrivalTime,
                    r.Duration,
                    COALESCE(ar.Model, r.AircraftTypeId) AS AircraftTypeId,
                    r.AircraftCode,
                    r.Frequency,
                    r.PriceFirstClass,
                    r.PriceEconomy,
                    r.HandBagPrice,
                    r.HandBagWeight,
                    r.BagPrice,
                    r.BagWeight,
                    r.BagMultiplier,
                    r.StartDate,
                    r.FinalizationDate,
                    r.EconomyClassCapacity,
                    r.FirstClassCapacity,
                    r.OriginCity,
                    r.DestinationCity
                FROM Route r
                INNER JOIN Airport ao
                    ON r.OriginAirport = ao.Code
                AND ao.IsDeleted = 0
                INNER JOIN Airport ad
                    ON r.DestinationAirport = ad.Code
                AND ad.IsDeleted = 0
                INNER JOIN Aircraft ar
                    ON r.AircraftCode = ar.Code
                AND ar.IsDeleted = 0
                WHERE r.IsDeleted = 0
            ";

            if (!hasLocation && date == null)
            {
                return connection.Query<RouteDbModel>(baseSelect).ToList();
            }

            if (!hasLocation)
            {
                return connection.Query<RouteDbModel>(
                    baseSelect + @"
                    AND NOT EXISTS (
                        SELECT 1
                        FROM ScheduledFlight sf
                        JOIN Aircraft a ON sf.AircraftCode = a.Code
                        WHERE sf.RouteCode = r.Code
                        AND sf.DepartureDate = @Date
                        AND sf.BookedSeats >= (
                                a.EconomyRows * a.EconomySeatsPerRow
                            + a.FirstClassRows * a.FirstClassSeatsPerRow
                        )
                    );",
                    new { Date = date }
                ).ToList();
            }

            return connection.Query<RouteDbModel>(
                baseSelect + @"
                AND (
                    (@OriginType = 'city' AND ao.City = @Origin)
                    OR
                    (@OriginType = 'airport' AND r.OriginAirport = @Origin)
                )
                AND (
                    (@DestType = 'city' AND ad.City = @Destination)
                    OR
                    (@DestType = 'airport' AND r.DestinationAirport = @Destination)
                )
                AND (
                    @Date IS NULL
                    OR NOT EXISTS (
                        SELECT 1
                        FROM ScheduledFlight sf
                        JOIN Aircraft a ON sf.AircraftCode = a.Code
                        WHERE sf.RouteCode = r.Code
                        AND sf.DepartureDate = @Date
                        AND sf.BookedSeats >= (
                                a.EconomyRows * a.EconomySeatsPerRow
                            + a.FirstClassRows * a.FirstClassSeatsPerRow
                        )
                    )
                );",
                new
                {
                    Origin = origin,
                    OriginType = originType,
                    Destination = destination,
                    DestType = destinationType,
                    Date = date
                }
            ).ToList();
        }

        public IEnumerable<string> GetIntermediateDestinations(string originAirport)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.Query<string>(
                "SELECT DISTINCT DestinationAirport FROM Route WHERE OriginAirport = @Origin",
                new { Origin = originAirport }).ToList();
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
                    r.Code,
                    r.OriginAirport,
                    r.DestinationAirport,
                    r.DepartureTime,
                    r.ArrivalTime,
                    r.Duration,
                    COALESCE(a.Model, r.AircraftTypeId) AS AircraftTypeId,
                    r.AircraftCode,
                    r.Frequency,
                    r.PriceFirstClass,
                    r.PriceEconomy,
                    r.HandBagPrice,
                    r.HandBagWeight,
                    r.BagPrice,
                    r.BagWeight,
                    r.BagMultiplier,
                    r.StartDate,
                    r.FinalizationDate,
                    r.EconomyClassCapacity,
                    r.FirstClassCapacity,
                    r.OriginCity,
                    r.DestinationCity
                FROM Route r
                LEFT JOIN Aircraft a
                    ON r.AircraftCode = a.Code
                WHERE r.IsDeleted = 0;
            ";

            using var connection = new SqlConnection(_connectionString);

            return connection.Query<RouteDbModel>(query).ToList();
        }

        public int GetOrCreateScheduledFlight(string routeCode, DateTime date)
        {
            RouteDbModel? route = GetRouteByCode(routeCode);

            if (route == null)
            {
                throw new Exception("La ruta seleccionada no existe o fue eliminada.");
            }

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
        public RouteDbModel? GetRouteByCode(string routeCode)
        {
            using var connection = new SqlConnection(_connectionString);

            return connection.QuerySingleOrDefault<RouteDbModel>(
                @"
                SELECT
                    r.Code,
                    r.OriginAirport,
                    r.DestinationAirport,
                    r.DepartureTime,
                    r.ArrivalTime,
                    r.Duration,
                    COALESCE(a.Model, r.AircraftTypeId) AS AircraftTypeId,
                    r.AircraftCode,
                    r.Frequency,
                    r.PriceFirstClass,
                    r.PriceEconomy,
                    r.HandBagPrice,
                    r.HandBagWeight,
                    r.BagPrice,
                    r.BagWeight,
                    r.BagMultiplier,
                    r.StartDate,
                    r.FinalizationDate,
                    r.EconomyClassCapacity,
                    r.FirstClassCapacity,
                    r.OriginCity,
                    r.DestinationCity
                FROM Route r
                LEFT JOIN Aircraft a
                    ON r.AircraftCode = a.Code
                WHERE UPPER(LTRIM(RTRIM(r.Code))) = UPPER(LTRIM(RTRIM(@Code)))
                AND r.IsDeleted = 0;
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

        public bool DeleteRoute(string code)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            try
            {
                bool wasDeleted = connection.ExecuteScalar<bool>(
                    "DeleteRoute",
                    new { Code = code },
                    commandType: CommandType.StoredProcedure
                );

                return wasDeleted;
            }
            catch
            {
                throw;
            }
        }
    }
}
