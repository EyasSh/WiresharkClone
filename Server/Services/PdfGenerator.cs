using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace Server.Services;
public class PdfGenerator
{
    /// <summary>
    /// Generates a simple PDF and returns it as a byte array.
    /// </summary>
    public static byte[] GenerateSimplePdfBytes()
    {
        // Create the document
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().Text("Hello from QuestPDF! 🎉").FontSize(20);
                    col.Item().Text("This PDF was generated in-memory for emailing.");
                });
            });
        });

        // Return the PDF as a byte array
        // QuestPDF supports GeneratePdf() overload that returns byte[] directly
        var pdfBytes = document.GeneratePdf();
        return pdfBytes;  // :contentReference[oaicite:0]{index=0}
    }
}
