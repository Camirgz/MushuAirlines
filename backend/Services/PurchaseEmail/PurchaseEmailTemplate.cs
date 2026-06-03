using backend.Model;
using System.Text;

namespace backend.Templates
{
    public static class PurchaseEmailTemplate
    {
        public static string Build(PurchaseConfirmationModel model)
        {
            return $@"
            <!DOCTYPE html>

            <html>

            <head>

                <meta charset='UTF-8'>

                <title>
                    Confirmación de compra
                </title>

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

            </head>

            <body style='
                margin:0;
                padding:0;
                background:#f4f4f4;
                font-family:Arial,sans-serif;
            '>

                <div style='{EmailStyles.MainContainer}'>

                    <div style='{EmailStyles.Hero}'>

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

                    {EmailSections.BuildCustomerSection(model)}

                    {EmailSections.BuildFlightSection(model)}

                    {EmailSections.BuildReservationSection(model)}

                    {EmailSections.BuildTicketsSection(model)}

                    {EmailSections.BuildPaymentSection(model)}

                    {EmailSections.BuildFooterSection()}

                </div>

            </body>

            </html>
            ";
        }
    }
}