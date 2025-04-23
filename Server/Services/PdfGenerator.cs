using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace Server.Services;
public class PdfGenerator
{
    public static void GenerateSimplePdf()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "output", "hello.pdf");

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().Text("Hello from QuestPDF!").FontSize(20);
                    col.Item().Text("This PDF was generated and saved locally. 🎉");
                });
            });
        })
        .GeneratePdf(filePath);
    }
}
