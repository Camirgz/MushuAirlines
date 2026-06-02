using backend.Model;

namespace backend.Templates
{
    public static class InvoiceEmailTemplate
    {
        public static string Build(PurchaseConfirmationModel model)
        {
            return $@"
            <!DOCTYPE html>

            <html>

            <head>

                <meta charset='UTF-8'>

                <title>
                    Factura de compra
                </title>

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
                            Factura de compra
                        </p>

                    </div>

                    <div style='{EmailStyles.Section}'>

                        <div style='{EmailStyles.Card}'>

                            <h2>
                                Información de factura
                            </h2>

                            <p>
                                <strong>
                                    Número de factura:
                                </strong>

                                {model.InvoiceNumber}
                            </p>

                            <p>
                                <strong>
                                    Código de reserva:
                                </strong>

                                {model.ReservationCode}
                            </p>

                            <h2 style='
                                color:#e60000;
                                margin-top:30px;
                            '>

                                Total pagado:
                                ${model.TotalPaid:N2}

                            </h2>

                        </div>

                    </div>
                    {EmailSections.BuildInvoiceNoteSection()}
                    {EmailSections.BuildFooterSection()}

                </div>

            </body>

            </html>
            ";
        }
    }
}