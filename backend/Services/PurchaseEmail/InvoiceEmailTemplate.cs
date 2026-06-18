using backend.Model;
using System.Text;

namespace backend.Templates
{
    public static class InvoiceEmailTemplate
    {
        public static string Build(PurchaseConfirmationModel model)
        {
            StringBuilder baggageHtml = new StringBuilder();
            
            if (model.BaggageDetails != null && model.BaggageDetails.Count > 0)
            {
                foreach (var baggage in model.BaggageDetails)
                {
                    var baggageType = baggage.Type == BaggageType.HandBaggage
                        ? "Equipaje de Mano"
                        : "Equipaje Documentado";

                    baggageHtml.Append($@"
                        <tr>
                            <td style='padding:10px; border-bottom:1px solid #eee;'>
                                {baggageType} ({baggage.Quantity})
                            </td>
                            <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>
                                ${baggage.Subtotal:N2}
                            </td>
                        </tr>
                    ");
                }
            }

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

                            <h3 style='margin-top:25px; color:#555;'>
                                Desglose de Pago
                            </h3>

                            <table style='width:100%; border-collapse:collapse;'>
                                <tr style='background:#f9f9f9;'>
                                    <th style='padding:10px; text-align:left;'>
                                        Concepto
                                    </th>
                                    <th style='padding:10px; text-align:right;'>
                                        Monto
                                    </th>
                                </tr>
                                <tr>
                                    <td style='padding:10px; border-bottom:1px solid #eee;'>
                                        Asientos ({model.TotalSeats} pax)
                                    </td>
                                    <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>
                                        ${(model.Details?.Sum(d => d.Subtotal) ?? 0m):N2}
                                    </td>
                                </tr>
                                {baggageHtml}
                                <tr style='background:#fff3e0;'>
                                    <td style='padding:10px; font-weight:bold;'>
                                        TOTAL
                                    </td>
                                    <td style='padding:10px; text-align:right; font-weight:bold; color:#e60000; font-size:16px;'>
                                        ${model.TotalPaid:N2}
                                    </td>
                                </tr>
                            </table>

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