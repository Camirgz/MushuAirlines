using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class PurchaseService
    {
        private readonly RouteCreationService routeService;
        private readonly PurchaseRepository purchaseRepository;

        public PurchaseService()
        {
            routeService = new RouteCreationService();
            purchaseRepository = new PurchaseRepository();
        }

        public string Purchase(
            string routeCode,
            DateTime flightDate,
            int passengerId,
            int seatNumber)
        {
            try
            {
                // 1. Obtener o crear el vuelo específico
                int scheduledFlightId =
                    routeService.GetOrCreateScheduledFlight(
                        routeCode,
                        flightDate
                    );

                // 2. Obtener información de la ruta
                RouteCreationModel route =
                    routeService.GetRouteByCode(routeCode);

                // 3. Generar QR simple
                string qr =
                    Guid.NewGuid().ToString();

                // 4. Crear ticket
                purchaseRepository.CreateTicket(
                    scheduledFlightId,
                    qr,
                    passengerId,
                    seatNumber
                );

                // 5. Retornar mensaje simple para probar
                return
                    "Purchase completed successfully.\n" +
                    "Route: " + route.Code + "\n" +
                    "From: " + route.OriginAirport + "\n" +
                    "To: " + route.DestinationAirport + "\n" +
                    "Flight Id: " + scheduledFlightId + "\n" +
                    "QR: " + qr;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }
}