using backend.Model;
using System.Text;

namespace backend.Templates
{
    public static class EmailSections
    {
        public static string BuildCustomerSection(PurchaseConfirmationModel model)
        {
            StringBuilder passengersHtml = new();

            if (model.PassengerBaggageDetails != null)
            {
                foreach (var passenger in model.PassengerBaggageDetails)
                {
                    passengersHtml.Append($@"<li>{passenger.PassengerFullName}</li>");
                }
            }

            return $@"
                <div style='{EmailStyles.Section}'>

                    <h3>
                        Información del cliente
                    </h3>

                    <div style='{EmailStyles.Card}'>

                        <p>
                            <strong>Comprador:</strong>
                            {model.FullName}
                        </p>

                        <p>
                            <strong>Correo electrónico:</strong>
                            {model.Email}
                        </p>

                        <p>
                            <strong>Pasajeros:</strong>
                        </p>

                        <ul>
                            {passengersHtml}
                        </ul>

                    </div>

                </div>
            ";
        }

        public static string BuildFlightSection(
            PurchaseConfirmationModel model)
        {
            string secondFlightHtml = "";

            if (!string.IsNullOrEmpty(model.FlightNumber2))
            {
                secondFlightHtml = $@"
                    <hr style='margin:20px 0;'>

                    <h4>
                        Segundo vuelo
                    </h4>

                    <p>
                        <strong>Número de vuelo:</strong>
                        {model.FlightNumber2}
                    </p>

                    <p>
                        <strong>Origen:</strong>
                        {model.OriginAirport2}
                    </p>

                    <p>
                        <strong>Destino:</strong>
                        {model.DestinationAirport2}
                    </p>

                    <p>
                        <strong>Salida:</strong>
                        {model.DepartureDate2}
                    </p>

                    <p>
                        <strong>Llegada:</strong>
                        {model.ArrivalDate2}
                    </p>
                ";
            }

            return $@"
                <div style='{EmailStyles.Section}'>

                    <h3>
                        Información del vuelo
                    </h3>

                    <div style='{EmailStyles.FlightCard}'>

                        <h4>
                            Primer vuelo
                        </h4>

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
                            {(string.IsNullOrWhiteSpace(model.FlightNumber2)
                                ? "Vuelo directo"
                                : model.OriginAirport2)}
                        </p>

                        {secondFlightHtml}

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
                ticketsHtml.Append($@"
                    <div class='ticket-card'>

                        <div>
                            <strong>
                                {detail.SeatCount}
                            </strong>

                            {detail.SeatClass}
                        </div>

                        <div class='ticket-price'>
                            ${detail.PricePerSeat:N2}
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
            StringBuilder baggageHtml = new();

            if (model.BaggageDetails != null &&
                model.BaggageDetails.Count > 0)
            {
                foreach (var baggage in model.BaggageDetails)
                {
                    var baggageType =
                        baggage.Type == BaggageType.HandBaggage
                            ? "Equipaje de Mano"
                            : "Equipaje Documentado";

                    baggageHtml.Append($@"
                        <div style='
                            display:flex;
                            justify-content:space-between;
                            margin-bottom:10px;
                            padding:10px 0;
                            border-bottom:1px solid rgba(255,255,255,0.15);
                        '>

                            <span>
                                {baggageType}
                                ({baggage.Quantity})
                            </span>

                            <strong>
                                ${baggage.Subtotal:N2}
                            </strong>

                        </div>
                    ");
                }
            }

            StringBuilder passengerBaggageHtml = new();

            if (model.PassengerBaggageDetails != null &&
                model.PassengerBaggageDetails.Count > 0)
            {
                passengerBaggageHtml.Append(@"
                    <div style='margin-top:25px;'>

                        <h4 style='margin-bottom:15px;'>
                            Equipaje por pasajero
                        </h4>

                        <table style='
                            width:100%;
                            border-collapse:collapse;
                            background:rgba(255,255,255,0.08);
                            border-radius:8px;
                        '>

                            <tr>
                                <th style='padding:10px;text-align:left;border-bottom:1px solid rgba(255,255,255,0.2);'>
                                    Pasajero
                                </th>

                                <th style='padding:10px;border-bottom:1px solid rgba(255,255,255,0.2);'>
                                    Mano
                                </th>

                                <th style='padding:10px;border-bottom:1px solid rgba(255,255,255,0.2);'>
                                    Documentado
                                </th>

                                <th style='padding:10px;border-bottom:1px solid rgba(255,255,255,0.2);'>
                                    Subtotal
                                </th>
                            </tr>
                ");

                foreach (var passenger in model.PassengerBaggageDetails)
                {
                    passengerBaggageHtml.Append($@"
                        <tr>

                            <td style='padding:10px;border-bottom:1px solid rgba(255,255,255,0.1);'>
                                {passenger.PassengerFullName}
                            </td>

                            <td style='padding:10px;text-align:center;border-bottom:1px solid rgba(255,255,255,0.1);'>
                                {passenger.HandBagCount}
                            </td>

                            <td style='padding:10px;text-align:center;border-bottom:1px solid rgba(255,255,255,0.1);'>
                                {passenger.CheckedBagCount}
                            </td>

                            <td style='padding:10px;text-align:right;border-bottom:1px solid rgba(255,255,255,0.1);'>
                                ${passenger.BaggageSubtotal:N2}
                            </td>

                        </tr>
                    ");
                }

                passengerBaggageHtml.Append($@"
                        <tr style='font-weight:bold;'>

                            <td style='padding:10px;'>
                                Total equipaje
                            </td>

                            <td style='padding:10px;text-align:center;'>
                                {model.PassengerBaggageDetails.Sum(p => p.HandBagCount)}
                            </td>

                            <td style='padding:10px;text-align:center;'>
                                {model.PassengerBaggageDetails.Sum(p => p.CheckedBagCount)}
                            </td>

                            <td style='padding:10px;text-align:right;'>
                                ${model.PassengerBaggageDetails.Sum(p => p.BaggageSubtotal):N2}
                            </td>

                        </tr>

                    </table>

                </div>
                ");
            }

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

                        <div style='
                            margin-top:20px;
                            padding-top:15px;
                            border-top:1px solid rgba(255,255,255,0.2);
                        '>

                            <div style='
                                display:flex;
                                justify-content:space-between;
                                margin-bottom:10px;
                            '>

                                <span>
                                    Asientos ({model.TotalSeats})
                                </span>

                                <strong>
                                    ${(model.Details?.Sum(d => d.Subtotal) ?? 0m):N2}
                                </strong>

                            </div>

                            {baggageHtml}

                            {passengerBaggageHtml}

                        </div>

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
        public static string BuildInvoiceNoteSection()
        {
            return @"
                <div style='
                    padding:0 30px 30px 30px;
                '>

                    <div style='
                        background:#eef5ff;
                        border:1px solid #c7dcff;
                        border-radius:12px;
                        padding:22px;
                        text-align:center;
                        color:#4b5563;
                        line-height:1.6;
                    '>

                        Esta factura ha sido generada automáticamente.
                        Para cualquier aclaración, por favor contacta
                        a nuestro equipo de soporte.

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