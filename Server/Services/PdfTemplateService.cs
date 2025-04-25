using System;
using System.Text;
using Server.Models;
namespace Server.Services;
public class PdfTemplateService
{
    /// <summary>
    /// Generates an HTML string for a performance report, formatted for PDF conversion.
    /// </summary>
    /// <param name="metrics">A tuple of 3 doubles containing performance metrics: CPU usage, RAM usage, and disk usage.</param>
    /// <param name="user">The user for whom the report is generated.</param>
    /// <returns>A string containing the HTML representation of the performance report.</returns>
    /// <remarks>
    /// The generated HTML includes a styled table with performance metrics, a footer with report generation time and user information, and a styled header with the report title.
    /// </remarks>
    public static string PerformanceReportTemplate((double cpuUsage, double ramUsage, double diskUsage) metrics, User? user = null)
    {
        if (user == null)
        {
            throw new ArgumentException("User cannot be null.");
        }
        var html = new StringBuilder();

        html.Append(@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset=""utf-8"">
                    <title>Performance Report</title>
                    <style>
                        /* Define page size and margins for PDF */
                         @page {
                            size: A4;
                            margin: 1in;
                        }
                        body {
                            font-family: Arial, sans-serif;
                            margin: 0;
                            color: #333;
                        }
                        h1 {
                            text-align: center;
                            font-size: 24px;
                            margin-bottom: 20px;
                        }
                        table {
                            width: 100%;
                            border-collapse: collapse;
                            margin-bottom: 20px;
                            font-size: 12px;
                        }
                        thead {
                            background-color: #2F4F4F;
                            color: white;
                        }
                        th, td {
                            border: 1px solid #aaa;
                            padding: 6px 8px;
                        }
                        tr:nth-child(even) {
                            background-color: #f9f9f9;
                        }
                        tr:hover {
                            background-color: #f1f1f1;
                        }
                        footer {
                                position: fixed;
                                bottom: 0;
                                width: 100%;
                                text-align: center;
                                font-size: 10px;
                                color: #666;
                            }
                    </style>
                </head>
            <body>
            <h1>Performance Report</h1>
                <table>
                    <thead>
                        <tr>
                            <th>Metric</th>
                            <th>Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>CPU Usage</td>
                            <td>" + metrics.cpuUsage + @"</td>
                        </tr>
                        <tr>
                            <td>RAM Usage</td>
                            <td>" + metrics.ramUsage + @"</td>
                        </tr>
                        <tr>
                            <td>Disk Usage</td>
                            <td>" + metrics.diskUsage + @"</td>
                        </tr>
                    </tbody>
                </table>
                <footer>
                    Report generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
                    <p>
                        For User: " + user.Name + @"
                        <br />
                        Id: " + user.Id + @"
                    </p>
                </footer>
            </body>
            </html>");
        return html.ToString();
    }
    /// <summary>
    /// Generates an HTML string for a packet report, formatted for PDF conversion.
    /// </summary>
    /// <param name="packets">A collection of PacketInfo objects containing packet details.</param>
    /// <returns>A string containing the HTML representation of the packet report.</returns>
    /// <remarks>
    /// The generated HTML includes a styled table with packet information such as timestamp, source IP,
    /// source port, destination IP, destination port, and protocol. The HTML is styled for PDF output
    /// with a fixed header and footer, alternating row colors, and hover effects.
    /// </remarks>
    public static string PacketReportTemplate(IEnumerable<PacketInfo> packets, User? user = null)
    {
        if (user == null || (packets == null || !packets.Any()))
        {
            throw new ArgumentException("User or packets cannot be null or empty.");

        }
        var html = new StringBuilder();

        html.Append(@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset=""utf-8"">
                    <title>Packet Report</title>
                    <style>
                        /* Define page size and margins for PDF */
                         @page {
                            size: A4;
                            margin: 1in;
                        }
                        body {
                            font-family: Arial, sans-serif;
                            margin: 0;
                            color: #333;
                        }
                        h1 {
                            text-align: center;
                            font-size: 24px;
                            margin-bottom: 20px;
                        }
                        table {
                            width: 100%;
                            border-collapse: collapse;
                            margin-bottom: 20px;
                            font-size: 12px;
                        }
                        thead {
                            background-color: #2F4F4F;
                            color: white;
                        }
                        th, td {
                            border: 1px solid #aaa;
                            padding: 6px 8px;
                        }
                        tr:nth-child(even) {
                            background-color: #f9f9f9;
                        }
                        tr:hover {
                            background-color: #f1f1f1;
                        }
                        footer {
                                position: fixed;
                                bottom: 0;
                                width: 100%;
                                text-align: center;
                                font-size: 10px;
                                color: #666;
                            }
                            .page-break {
                                page-break-after: always;
                            }   
                    </style>
                </head>
            <body>
            <h1>Packet Report</h1>
                <table>
                    <thead>
                        <tr>
                            <th>Timestamp</th>
                            <th>Source IP</th>
                            <th>Source Port</th>
                            <th>Destination IP</th>
                            <th>Destination Port</th>
                            <th>Protocol</th>
                        </tr>
                    </thead>
                    <tbody>
");

        foreach (var packet in packets)
        {
            html.AppendFormat(@"
            <tr>
                <td>{0:yyyy-MM-dd HH:mm:ss}</td>
                <td>{1}</td>
                <td>{2}</td>
                <td>{3}</td>
                <td>{4}</td>
                <td>{5}</td>
                <td>{6}</td>
            </tr>",
                packet.Timestamp,
                packet.SourceIP,
                packet.SourcePort,
                packet.DestinationIP,
                packet.DestinationPort,
                packet.Protocol);
        }

        html.Append(@"
                </tbody>
            </table>
            <footer>
                Report generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
                <p>
                    For User: " + user.Name + @"
                    <br />
                    Id: " + user.Id + @"

                </p>
            </footer>
        </body>
    </html>");

        return html.ToString();
    }

}