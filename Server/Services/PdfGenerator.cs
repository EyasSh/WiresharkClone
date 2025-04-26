using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Server.Models;
namespace Server.Services;
public class PdfGenerator
{
    /// <summary>
    /// Generates a performance report PDF and returns it as a byte array.
    /// The PDF includes a header with the title "Performance Report", a table
    /// with the CPU, RAM, and disk usage metrics, and an optional footer with
    /// the user's name and ID. The metrics are passed as a tuple of three
    /// doubles. The user is an optional parameter, and if provided, the
    /// footer will contain the user's name and ID.
    /// </summary>
    /// <param name="metrics">Tuple of three doubles representing the CPU, RAM, and disk usage metrics.</param>
    /// <param name="user">Optional user object containing the name and ID.</param>
    /// <returns>A byte array representing the generated PDF.</returns>
    public static byte[] GeneratePerformancePdf(
    (double cpuUsage, double ramUsage, double diskUsage) metrics,
    User? user = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // 1. Page setup
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Inch);

                // 2. Header
                page.Header()
                    .Height(50)
                    .Background(Colors.Blue.Medium)
                    .Padding(10)
                    .AlignMiddle()
                    .Text("Performance Report")
                    .FontSize(22)
                    .FontColor(Colors.White)
                    .SemiBold()
                    .FontFamily("Segoe UI");

                // 3. Single Content block with Column
                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        // 3.1 Metrics table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Metric").SemiBold().FontFamily("Tahoma");
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Value").SemiBold().FontFamily("Tahoma");
                            });

                            void AddRow(string name, string value)
                            {
                                table.Cell().Padding(5)
                                     .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                     .Text(name);
                                table.Cell().Padding(5)
                                     .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                     .Text(value);
                            }

                            AddRow("CPU Usage", $"{metrics.cpuUsage / 100:P1}");
                            AddRow("RAM Usage", $"{metrics.ramUsage / 100:P1}");
                            AddRow("Disk Usage", $"{metrics.diskUsage / 100:P1}");
                        });

                        // 3.2 Conditional user info
                        if (user != null)
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text($"User: {user.Name}  |  Email: {user.Email}")
                                .FontFamily("Tahoma")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        }
                    });

                // 4. Footer
                page.Footer()
                    .AlignCenter()
                    .Padding(5)
                    .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .FontFamily("Verdana")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Generates a packet report PDF and returns it as a byte array.
    /// The PDF includes a header with the title "Packet Report", a table
    /// with the timestamp, source IP, source port, destination IP, destination port, and protocol
    /// of each packet, and an optional footer with the user's name and ID.
    /// The packets are passed as an enumerable of <see cref="PacketInfo"/>, and the user is an
    /// optional parameter, and if provided, the footer will contain the user's name and ID.
    /// </summary>
    /// <param name="packets">Enumerable of <see cref="PacketInfo"/> objects representing the packets.</param>
    /// <param name="user">Optional user object containing the name and ID.</param>
    /// <returns>A byte array representing the generated PDF.</returns>
    public static byte[] GeneratePacketPdf(
        IEnumerable<PacketInfo> packets,
        User? user = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // 1. Page setup
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Inch);

                // 2. Header
                page.Header()
                    .Height(50)
                    .Background(Colors.Blue.Medium)
                    .Padding(10)
                    .AlignMiddle()
                    .Text("Packet Report")
                    .FontSize(22)
                    .FontColor(Colors.White)
                    .SemiBold();

                // 3. Content: packets table
                page.Content().PaddingVertical(20).Table(table =>
                {
                    // Define five equal columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header row styling
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Timestamp").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Source IP").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Source Port").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Destination IP").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Destination Port").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Protocol").SemiBold();
                    });

                    // Data rows
                    foreach (var packet in packets)
                    {
                        table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(packet.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                        table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(packet.SourceIP);
                        table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(packet.SourcePort.ToString());
                        table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(packet.DestinationIP);
                        table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(packet.DestinationPort.ToString());  // :contentReference[oaicite:0]{index=0}
                        table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(packet.Protocol);                  // :contentReference[oaicite:0]{index=0}
                    }
                });

                // 4. Optional user info block
                if (user != null)
                {
                    page.Content()
                        .PaddingTop(10)
                        .ShowEntire()
                        .Text($"User: {user.Name}  |  Id: {user.Id}")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Darken1);
                }

                // 5. Footer                                        
                page.Footer()
                    .AlignCenter()
                    .Padding(5)
                    .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Lighten1);
            });
        });

        return document.GeneratePdf();  // :contentReference[oaicite:0]{index=0}
    }
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
