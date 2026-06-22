using backend.Model;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace backend.Templates
{
    public static class PdfSections
    {
        
        public static void BuildHeader(
            IContainer container,
            PurchaseConfirmationModel model)
        {
            container
                .Background(PdfStyles.HeaderBg)
                .Padding(PdfStyles.CardPadding)
                .Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Mushu Airlines")
                            .FontSize(PdfStyles.TitleSize)
                            .Bold()
                            .FontColor(PdfStyles.WhiteColor);

                        col.Item().PaddingTop(PdfStyles.SubtitleTopPad)
                            .Text("Flight Itinerary & Reservation")
                            .FontSize(PdfStyles.SubtitleSize)
                            .FontColor(PdfStyles.AccentColor);
                    });

                    row.ConstantItem(PdfStyles.ReservationCodeColumn).Column(col =>
                    {
                        col.Item().AlignRight().Text("RESERVATION CODE")
                            .FontSize(PdfStyles.LabelSize)
                            .FontColor(PdfStyles.LabelColor);

                        col.Item().AlignRight()
                            .Text(model.ReservationCode)
                            .FontSize(PdfStyles.ReservationCodeSize)
                            .Bold()
                            .FontColor(PdfStyles.AccentColor);
                    });
                });
        }

        public static void BuildReservationSection(
            IContainer container,
            PurchaseConfirmationModel model,
            byte[] qrCode)
        {
            container
                .Background(PdfStyles.CardBackground)
                .Padding(PdfStyles.CardPadding)
                .Column(col =>
                {
                    BuildSectionTitle(col, "Passenger Information");

                    col.Item().PaddingTop(PdfStyles.InnerSpacing)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(info =>
                            {
                                info.Spacing(PdfStyles.LineSpacing);

                                BuildLabelValue(info, "Full Name",        model.FullName);
                                BuildLabelValue(info, "Email",            model.Email);
                                BuildLabelValue(info, "Reservation Code", model.ReservationCode);
                            });

                            row.ConstantItem(PdfStyles.QrColumn).Column(qrCol =>
                            {
                                qrCol.Item().AlignCenter()
                                    .Text("Scan at the airport")
                                    .FontSize(PdfStyles.LabelSize)
                                    .FontColor(PdfStyles.LabelColor);

                                qrCol.Item().PaddingTop(PdfStyles.QrTopPad)
                                    .AlignCenter()
                                    .Height(PdfStyles.QrCodeSize)
                                    .Image(qrCode);
                            });
                        });
                });
        }

        public static void BuildFlightSection(
            IContainer container,
            PurchaseConfirmationModel model)
        {
            container
                .Background(PdfStyles.CardBackground)
                .Padding(PdfStyles.CardPadding)
                .Column(col =>
                {
                    BuildSectionTitle(col, "Flight Details");

                    col.Item().PaddingTop(PdfStyles.InnerSpacing);

                    BuildFlightCard(col, model, 1);

                    if (!string.IsNullOrWhiteSpace(model.FlightNumber2))
                    {
                        col.Item().PaddingTop(PdfStyles.InnerSpacing)
                            .LineHorizontal(PdfStyles.DividerThickness)
                            .LineColor(PdfStyles.DividerColor);

                        col.Item().PaddingTop(PdfStyles.InnerSpacing)
                            .Text("Connecting Flight")
                            .FontSize(PdfStyles.SmallSize)
                            .Bold()
                            .FontColor(PdfStyles.AccentColor);

                        col.Item().PaddingTop(PdfStyles.PassengerRowPad);

                        BuildFlightCard(col, model, 2);
                    }
                });
        }

       private static void BuildFlightCard(
            ColumnDescriptor col,
            PurchaseConfirmationModel model,
            int flightNumber)
        {
            var origin      = flightNumber == 1 ? model.OriginAirport      : model.OriginAirport2;
            var destination = flightNumber == 1 ? model.DestinationAirport : model.DestinationAirport2;
            var flight      = flightNumber == 1 ? model.FlightNumber       : model.FlightNumber2;
            var aircraft    = flightNumber == 1 ? model.AircraftModel      : model.AircraftModel2;

            DateTime? departure = flightNumber == 1 ? model.DepartureDate : model.DepartureDate2;
            DateTime? arrival   = flightNumber == 1 ? model.ArrivalDate   : model.ArrivalDate2;

            // in case is overnight flight
            if (departure.HasValue && arrival.HasValue && arrival < departure)
            {
                arrival = arrival.Value.AddDays(1);
            }

            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(origin ?? "-")
                        .FontSize(PdfStyles.RouteSize)
                        .Bold()
                        .FontColor(PdfStyles.DarkColor);

                    c.Item().Text("Origin")
                        .FontSize(PdfStyles.LabelSize)
                        .FontColor(PdfStyles.LabelColor);
                });

                row.ConstantItem(PdfStyles.ArrowColumn)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text("→")
                    .FontSize(PdfStyles.RouteArrowSize)
                    .FontColor(PdfStyles.PrimaryColor);

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(destination ?? "-")
                        .FontSize(PdfStyles.RouteSize)
                        .Bold()
                        .FontColor(PdfStyles.DarkColor);

                    c.Item().Text("Destination")
                        .FontSize(PdfStyles.LabelSize)
                        .FontColor(PdfStyles.LabelColor);
                });
            });

            col.Item().PaddingTop(PdfStyles.InnerSpacing)
                .Row(row =>
                {
                    row.RelativeItem();
                    BuildLabelValueInline(row.RelativeItem(),"Flight",flight ?? "-");
                    BuildLabelValueInline(row.RelativeItem(),"Aircraft",aircraft ?? "-");
                    BuildLabelValueInline(row.RelativeItem(),"Departure",departure?.ToString("dd MMM yyyy HH:mm") ?? "-");
                    BuildLabelValueInline(row.RelativeItem(),"Arrival",arrival?.ToString("dd MMM yyyy HH:mm") ?? "-");
                });
        }

       public static void BuildTicketsSection(
    IContainer container,
    PurchaseConfirmationModel model)
{
    container
        .Background(PdfStyles.CardBackground)
        .Padding(PdfStyles.CardPadding)
        .Column(col =>
        {
            BuildSectionTitle(col, "Boarding Passes");

            if (model.Tickets != null && model.Tickets.Any())
            {
                foreach (var ticket in model.Tickets)
                {
                    col.Item()
                        .PaddingTop(PdfStyles.InnerSpacing)
                        .ShowEntire()
                        .Background(PdfStyles.TicketBackground)
                        .Border(1)
                        .BorderColor(PdfStyles.DividerColor)
                        .Padding(PdfStyles.TicketPadding)
                        .Column(ticketCol =>
                        {
                            // name
                            ticketCol.Item()
                                .Text(ticket.PassengerFullName ?? "Unknown Passenger")
                                .FontSize(PdfStyles.NormalSize)
                                .Bold()
                                .FontColor(PdfStyles.DarkColor);

                            ticketCol.Item()
                                .PaddingTop(PdfStyles.SmallSpacing)
                                .LineHorizontal(PdfStyles.DividerThickness)
                                .LineColor(PdfStyles.LightDividerColor);

                            // ticket
                            ticketCol.Item()
                                .PaddingTop(PdfStyles.SmallSpacing)
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item()
                                            .Text("FLIGHT")
                                            .FontSize(PdfStyles.SmallSize)
                                            .SemiBold()
                                            .FontColor(PdfStyles.LabelColor);

                                        c.Item()
                                            .Text(ticket.FlightNumber ?? "-")
                                            .FontSize(PdfStyles.NormalSize)
                                            .Bold();
                                    });
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item()
                                            .Text("SEAT")
                                            .FontSize(PdfStyles.SmallSize)
                                            .SemiBold()
                                            .FontColor(PdfStyles.LabelColor);

                                        c.Item()
                                            .Text(ticket.SeatNumber ?? "-")
                                            .FontSize(PdfStyles.NormalSize)
                                            .Bold();
                                    });

                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item()
                                            .Text("CLASS")
                                            .FontSize(PdfStyles.SmallSize)
                                            .SemiBold()
                                            .FontColor(PdfStyles.LabelColor);

                                        c.Item()
                                            .Text(ticket.SeatClass.ToString() ?? "-")
                                            .FontSize(PdfStyles.NormalSize)
                                            .Bold();
                                    });

                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item()
                                            .Text("GATE")
                                            .FontSize(PdfStyles.SmallSize)
                                            .SemiBold()
                                            .FontColor(PdfStyles.LabelColor);

                                        c.Item()
                                            .Text("TBD")
                                            .FontSize(PdfStyles.NormalSize)
                                            .Bold();
                                    });
                                });
                        });
                }
            }
            else
            {
                col.Item()
                    .PaddingTop(PdfStyles.InnerSpacing)
                    .Text("No boarding passes available")
                    .FontSize(PdfStyles.NormalSize)
                    .FontColor(PdfStyles.LabelColor);
            }
        });
}

        public static void BuildPassengersSection(
            IContainer container,
            PurchaseConfirmationModel model)
        {
            container
                .Background(PdfStyles.CardBackground)
                .Padding(PdfStyles.CardPadding)
                .Column(col =>
                {
                    BuildSectionTitle(col, "Passengers & Baggage");

                    foreach (var passenger in model.PassengerBaggageDetails)
                    {
                        col.Item().PaddingTop(PdfStyles.InnerSpacing)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(info =>
                                {
                                    info.Item().Text(passenger.PassengerFullName)
                                        .FontSize(PdfStyles.NormalSize)
                                        .Bold()
                                        .FontColor(PdfStyles.DarkColor);
                                });

                                BuildLabelValueInline(
                                    row.ConstantItem(PdfStyles.BaggageColumn),
                                    "Carry-on",
                                    passenger.HandBagCount.ToString());

                                BuildLabelValueInline(
                                    row.ConstantItem(PdfStyles.BaggageColumn),
                                    "Checked",
                                    passenger.CheckedBagCount.ToString());
                            });

                        col.Item().PaddingTop(PdfStyles.PassengerRowPad)
                            .LineHorizontal(PdfStyles.DividerThickness)
                            .LineColor(PdfStyles.DividerColor);
                    }
                });
        }

        private static void BuildSectionTitle(
            ColumnDescriptor col,
            string title)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(PdfStyles.AccentBarWidth)
                    .Background(PdfStyles.PrimaryColor);

                row.ConstantItem(PdfStyles.InnerSpacing);

                row.RelativeItem().Text(title)
                    .FontSize(PdfStyles.HeaderSize)
                    .Bold()
                    .FontColor(PdfStyles.DarkColor);
            });

            col.Item().PaddingTop(PdfStyles.InnerSpacing)
                .LineHorizontal(PdfStyles.DividerThickness)
                .LineColor(PdfStyles.DividerColor);
        }

        private static void BuildLabelValue(
            ColumnDescriptor col,
            string label,
            string value)
        {
            col.Item().Column(c =>
            {
                c.Item().Text(label.ToUpper())
                    .FontSize(PdfStyles.LabelSize)
                    .FontColor(PdfStyles.LabelColor);

                c.Item().Text(value ?? "-")
                    .FontSize(PdfStyles.NormalSize)
                    .FontColor(PdfStyles.DarkColor);
            });
        }

        private static void BuildLabelValueInline(
            IContainer container,
            string label,
            string value)
        {
            container.Column(c =>
            {
                c.Item().Text(label.ToUpper())
                    .FontSize(PdfStyles.LabelSize)
                    .FontColor(PdfStyles.LabelColor);

                c.Item().Text(value ?? "-")
                    .FontSize(PdfStyles.NormalSize)
                    .Bold()
                    .FontColor(PdfStyles.DarkColor);
            });
        }
    }
}