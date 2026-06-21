using backend.Model;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace backend.Templates
{
    public static class ItineraryPdfTemplate
    {
        public static byte[] Build(
            PurchaseConfirmationModel model,
            byte[] qrCode)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(PdfStyles.PageMargin);

                    page.Header()
                        .Element(c => PdfSections.BuildHeader(c, model));

                    page.Content()
                        .PaddingTop(PdfStyles.SectionSpacing)
                        .Column(column =>
                        {
                            column.Spacing(PdfStyles.SectionSpacing);

                            PdfSections.BuildReservationSection(
                                column.Item(), model, qrCode);

                            PdfSections.BuildFlightSection(
                                column.Item(), model);

                            PdfSections.BuildTicketsSection(
                                column.Item(), model);

                            PdfSections.BuildPassengersSection(
                                column.Item(), model);
                        });

                    page.Footer()
                        .PaddingTop(PdfStyles.LineSpacing)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text("Mushu Airlines · Thank you for flying with us")
                                .FontSize(PdfStyles.LabelSize)
                                .FontColor(PdfStyles.LabelColor);

                            row.ConstantItem(100)
                                .AlignRight()
                                .Text(txt =>
                                {
                                    txt.Span("Page ")
                                        .FontSize(PdfStyles.LabelSize)
                                        .FontColor(PdfStyles.LabelColor);

                                    txt.CurrentPageNumber()
                                        .FontSize(PdfStyles.LabelSize)
                                        .FontColor(PdfStyles.LabelColor);
                                });
                        });
                });
            }).GeneratePdf();
        }
    }
}