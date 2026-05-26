using backend.Services;

namespace backend.Services
{
    public class PurchaseService
    {
        private readonly RouteCreationService routeService;

        public PurchaseService()
        {
        //it uses routecreationSrvice to prepare the purchase of a flight, by ensuring that the corresponding scheduled flight exists
        // To do:dont break Solid(DIP)
            routeService = new RouteCreationService();
        }

      
        // Obtains the ScheduledFlight Id for a purchase.
        // If the ScheduledFlight does not exist,
        // it is automatically created.
        // you need to used it for obtain the ScheduledFlightId for creating tickets
   

        // use like this: you need the route code that could be XX0000(you created it in route creation) 
        // and the date of the flight that you want to purchase, and it will return the scheduled flight id that you need for create the ticket
        public int PrepareFlightPurchase(
            string routeCode,
            DateTime flightDate)
        {
            return routeService.GetOrCreateScheduledFlight(
                routeCode,
                flightDate
            );
        }
        //todo delete this example
        /*
            PurchaseService purchaseService =
                new PurchaseService();

            int scheduledFlightId =
                purchaseService.PrepareFlightPurchase(
                    "XX0000",
                    new DateTime(2026, 5, 22)
                );
        */
    }
}