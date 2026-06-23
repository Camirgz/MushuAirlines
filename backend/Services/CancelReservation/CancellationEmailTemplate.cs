using backend.Model;

namespace backend.Templates
{
    public static class CancellationEmailTemplate
    {
        public static string Build(
            CancellationReservationModel model)
        {
             string baseUrl = "http://localhost:8080/"; 
           // string baseUrl = "https://mushu-airlines.vercel.app/";
            string url = $"{baseUrl}cancel-reservation/{model.ReservationCode}";

            return $@"
            <!DOCTYPE html>

            <html>

            <body style='font-family:Arial;background:#f4f4f4;padding:40px;'>

            <div style='
            max-width:650px;
            margin:auto;
            background:white;
            padding:40px;
            border-radius:15px;
            text-align:center;
            '>

            <h1 style='color:#d62828'>
            Mushu Airlines
            </h1>

            <h2>
            Solicitud de cancelación
            </h2>

            <p>

            Se recibió una solicitud para cancelar la siguiente reserva.

            </p>

            <h1>

            {model.ReservationCode}

            </h1>

            <p>

            Si usted realizó esta solicitud,
            presione el botón de abajo.

            </p>

            <a
            href='{url}'
            style='
            display:inline-block;
            padding:14px 28px;
            background:#d62828;
            color:white;
            text-decoration:none;
            border-radius:10px;
            font-weight:bold;
            '>

            Cancelar reserva

            </a>

            <p style='margin-top:40px;color:gray'>

            Si usted no solicitó esta cancelación,
            simplemente ignore este correo.

            </p>

            </div>

            </body>

            </html>";
        }
    }
}