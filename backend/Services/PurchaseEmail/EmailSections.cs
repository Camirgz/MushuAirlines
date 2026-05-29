using backend.Model;
using System.Text;

namespace backend.Templates
{
    public static class EmailSections
    {
        public static string BuildCustomerSection(
            PurchaseConfirmationModel model)
        {
            return $@"
                <div style='{EmailStyles.Section}'>

                    <h3>
                        Información del cliente
                    </h3>

                    <div style='{EmailStyles.Card}'>

                        <p>
                            <strong>Nombre completo:</strong>
                            {model.FullName}
                        </p>

                        <p>
                            <strong>Correo electrónico:</strong>
                            {model.Email}
                        </p>

                        <p>
                            <strong>Pasaporte:</strong>
                            {model.PassportNumber}
                        </p>

                    </div>

                </div>
            ";
        }

        public static string BuildFlightSection(
            PurchaseConfirmationModel model)
        {
            return $@"
                <div style='{EmailStyles.Section}'>

                    <h3>
                        Información del vuelo
                    </h3>

                    <div style='{EmailStyles.FlightCard}'>

                        <p>
                            <strong>Número de vuelo:</strong>
                            {model.FlightNumber}
                        </p>

                        <p>
                            <strong>Tipo de avión:</strong>
                            {model.AircraftType}
                        </p>

                        <p>
                            <strong>Origen:</strong>
                            {model.OriginAirport}
                        </p>

                        <p>
                            <strong>Destino:</strong>
                            {model.DestinationAirport}
                        </p>

                        <p>
                            <strong>Salida:</strong>
                            {model.DepartureDate}
                        </p>

                        <p>
                            <strong>Llegada:</strong>
                            {model.ArrivalDate}
                        </p>

                        <p>
                            <strong>Escalas:</strong>
                            {model.Layover}
                        </p>

                    </div>

                </div>
            ";
        }

        public static string BuildReservationSection(
            PurchaseConfirmationModel model)
        {
            return $@"
                <div style='
                    {EmailStyles.Section}
                    text-align:center;
                '>

                    <h2>
                        Código de reserva
                    </h2>

                    <p style='color:#666;'>

                        Utiliza este código para realizar
                        check-in y consultar tu reservación

                    </p>

                    <div style='{EmailStyles.ReservationBox}'>

                        <div style='margin-bottom:25px;'>

                            <span style='
                                background:linear-gradient(90deg,#e60000,#f0a500);
                                color:white;
                                padding:14px 28px;
                                border-radius:10px;
                                font-size:30px;
                                font-weight:bold;
                            '>

                                {model.ReservationCode}

                            </span>

                        </div>

                        <img
                            src='cid:qrcode'
                            width='180'
                        >

                    </div>

                </div>
            ";
        }

        public static string BuildTicketsSection(
            PurchaseConfirmationModel model)
        {
            StringBuilder ticketsHtml =
                new StringBuilder();

            foreach (var detail in model.Details)
            {
                decimal pricePerSeat =
                    detail.Subtotal / detail.SeatCount;

                ticketsHtml.Append($@"
                    <div class='ticket-card'>

                        <div>
                            <strong>
                                {detail.SeatCount}
                            </strong>

                            {detail.SeatClass}
                        </div>

                        <div class='ticket-price'>
                            ${pricePerSeat:N2}
                            por asiento
                        </div>

                        <div class='ticket-total'>
                            ${detail.Subtotal:N2}
                        </div>

                    </div>
                ");
            }

            return $@"
                <div style='{EmailStyles.Section}'>

                    <h3>
                        Información de boletos
                    </h3>

                    {ticketsHtml}

                    <div style='
                        border:2px solid #e60000;
                        border-radius:12px;
                        padding:15px;
                        text-align:center;
                        margin-top:20px;
                        font-weight:bold;
                    '>

                        Total de pasajeros:
                        {model.TotalSeats}

                    </div>

                </div>
            ";
        }

        public static string BuildPaymentSection(
            PurchaseConfirmationModel model)
        {
            return $@"
                <div style='{EmailStyles.Section}'>

                    <h3>
                        Resumen de pago
                    </h3>

                    <div style='{EmailStyles.PaymentBox}'>

                        <p>
                            <strong>Número de factura:</strong>
                            {model.InvoiceNumber}
                        </p>

                        <p>
                            <strong>Método de pago:</strong>
                            {model.PaymentMethod}
                        </p>

                        <h2 style='
                            color:#f0a500;
                            margin-top:25px;
                        '>

                            Total pagado:
                            ${model.TotalPaid:N2}

                        </h2>

                    </div>

                </div>
            ";
        }

        public static string BuildFooterSection()
        {
            return $@"
                <div style='{EmailStyles.Footer}'>

                    <h2>
                        ¡Gracias por elegir Mushu Airlines!
                    </h2>

                    <p style='opacity:0.85;'>

                        Estamos comprometidos en brindarte
                        la mejor experiencia de vuelo

                    </p>

                    <div style='margin-top:30px;'>

                        <p>
                            mushuairlines@gmail.com
                        </p>

                        <p>
                            +506 8752-3817
                        </p>

                    </div>

                </div>
            ";
        }
    }
}