using backend.Model;
using System.Text;

namespace backend.Templates
{
    public static class PurchaseEmailTemplate
    {
        public static string Build(
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
            <!DOCTYPE html>

            <html>

            <head>

            <meta charset='UTF-8'>

            <title>
                Confirmación de compra
            </title>

            </head>

            <body style='
                margin:0;
                padding:0;
                background:#f4f4f4;
                font-family:Arial,sans-serif;
            '>

            <div style='
                max-width:760px;
                margin:auto;
                background:white;
            '>

                <div style='
                    background:linear-gradient(90deg,#e60000,#f0a500);
                    padding:50px 30px;
                    text-align:center;
                    color:white;
                '>

                    <h1 style='
                        margin:0;
                        font-size:34px;
                    '>
                        Mushu Airlines
                    </h1>

                    <p style='
                        margin-top:8px;
                        opacity:0.9;
                    '>
                        Vuela con el dragón
                    </p>

                    <h2 style='
                        margin-top:40px;
                        font-size:32px;
                    '>
                        Confirmación de compra
                    </h2>

                    <div style='margin-top:25px;'>

                        <span style='
                            background:white;
                            color:#e60000;
                            padding:14px 28px;
                            border-radius:12px;
                            font-size:28px;
                            font-weight:bold;
                        '>

                            {model.ReservationCode}

                        </span>

                    </div>

                    <p style='
                        margin-top:30px;
                        font-size:16px;
                    '>
                        Tu compra ha sido realizada exitosamente.
                    </p>

                </div>

                <div style='
                    padding:35px;
                    text-align:center;
                    color:#444;
                    line-height:1.6;
                '>

                    Tu vuelo ha sido reservado exitosamente.
                    A continuación encontrarás toda la información
                    de tu compra y reservación.

                </div>

                <!-- CLIENTE -->

                <div style='
                    padding:0 30px 30px 30px;
                '>

                    <h3>
                        Información del cliente
                    </h3>

                    <div style='
                        background:#f7f7f7;
                        border-radius:12px;
                        padding:20px;
                    '>

                        <p>
                            <strong>
                                Nombre completo:
                            </strong>

                            {model.FullName}
                        </p>

                        <p>
                            <strong>
                                Correo electrónico:
                            </strong>

                            {model.Email}
                        </p>

                        <p>
                            <strong>
                                Pasaporte:
                            </strong>

                            {model.PassportNumber}
                        </p>

                    </div>

                </div>

                <!-- VUELO -->

                <div style='
                    padding:0 30px 30px 30px;
                '>

                    <h3>
                        Información del vuelo
                    </h3>

                    <div style='
                        background:#fff8f0;
                        border-radius:12px;
                        padding:20px;
                    '>

                        <p>
                            <strong>
                                Número de vuelo:
                            </strong>

                            {model.FlightNumber}
                        </p>

                        <p>
                            <strong>
                                Tipo de avión:
                            </strong>

                            {model.AircraftType}
                        </p>

                        <p>
                            <strong>
                                Origen:
                            </strong>

                            {model.OriginAirport}
                        </p>

                        <p>
                            <strong>
                                Destino:
                            </strong>

                            {model.DestinationAirport}
                        </p>

                        <p>
                            <strong>
                                Salida:
                            </strong>

                            {model.DepartureDate}
                        </p>

                        <p>
                            <strong>
                                Llegada:
                            </strong>

                            {model.ArrivalDate}
                        </p>

                        <p>
                            <strong>
                                Escalas:
                            </strong>

                            {model.Layover}
                        </p>

                    </div>

                </div>

                <!-- RESERVA -->

                <div style='
                    padding:0 30px 30px 30px;
                    text-align:center;
                '>

                    <h2>
                        Código de reserva
                    </h2>

                    <p style='
                        color:#666;
                    '>
                        Utiliza este código para realizar
                        check-in y consultar tu reservación
                    </p>

                    <div style='
                        border:2px solid #e60000;
                        border-radius:16px;
                        padding:30px;
                        margin-top:20px;
                    '>

                        <div style='
                            margin-bottom:25px;
                        '>

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

                <!-- BOLETOS -->

                <div style='
                    padding:0 30px 30px 30px;
                '>

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

                <!-- PAGO -->

                <div style='
                    padding:0 30px 40px 30px;
                '>

                    <h3>
                        Resumen de pago
                    </h3>

                    <div style='
                        background:#09142b;
                        color:white;
                        border-radius:12px;
                        padding:25px;
                    '>

                        <p>
                            <strong>
                                Número de factura:
                            </strong>

                            {model.InvoiceNumber}
                        </p>

                        <p>
                            <strong>
                                Método de pago:
                            </strong>

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

                <!-- FOOTER -->

                <div style='
                    background:#09142b;
                    color:white;
                    text-align:center;
                    padding:40px 30px;
                '>

                    <h2>
                        ¡Gracias por elegir Mushu Airlines!
                    </h2>

                    <p style='
                        opacity:0.85;
                    '>
                        Estamos comprometidos en brindarte
                        la mejor experiencia de vuelo
                    </p>

                    <div style='
                        margin-top:30px;
                    '>

                        <p>
                            mushuairlines@gmail.com
                        </p>

                        <p>
                            +) 2450 0000
                        </p>

                    </div>

                </div>

            </div>

            <style>

            .ticket-card{{
                background:#f7f7f7;
                border-radius:12px;
                padding:20px;
                margin-bottom:15px;
            }}

            .ticket-price{{
                margin-top:8px;
                color:#666;
            }}

            .ticket-total{{
                margin-top:10px;
                font-size:22px;
                font-weight:bold;
                color:#e60000;
            }}

            </style>

            </body>

            </html>
            ";
        }
    }
}