namespace backend.Templates
{
    public static class EmailStyles
    {
        public const string MainContainer = @"
            max-width:760px;
            margin:auto;
            background:white;
        ";

        public const string Hero = @"
            background:linear-gradient(90deg,#e60000,#f0a500);
            padding:50px 30px;
            text-align:center;
            color:white;
        ";

        public const string Section = @"
            padding:0 30px 30px 30px;
        ";

        public const string Card = @"
            background:#f7f7f7;
            border-radius:12px;
            padding:20px;
        ";

        public const string FlightCard = @"
            background:#fff8f0;
            border-radius:12px;
            padding:20px;
        ";

        public const string ReservationBox = @"
            border:2px solid #e60000;
            border-radius:16px;
            padding:30px;
            margin-top:20px;
        ";

        public const string PaymentBox = @"
            background:#09142b;
            color:white;
            border-radius:12px;
            padding:25px;
        ";

        public const string Footer = @"
            background:#09142b;
            color:white;
            text-align:center;
            padding:40px 30px;
        ";
    }
}