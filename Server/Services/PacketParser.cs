using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using PacketDotNet;
using ARSoft.Tools.Net.Dns;

public static class PacketParser
{
    /// <summary>
    /// Returns the raw application-layer bytes (TCP or UDP) or null if none.
    /// </summary>
    public static byte[]? GetApplicationLayerPayload(Packet packet)
    {
        var tcp = packet.Extract<TcpPacket>();
        if (tcp != null && tcp.PayloadData?.Length > 0)
            return tcp.PayloadData;

        var udp = packet.Extract<UdpPacket>();
        if (udp != null && udp.PayloadData?.Length > 0)
            return udp.PayloadData;

        return null;
    }

    /// <summary>
    /// Attempts to decode, classify, and print a raw payload:
    /// JSON, UTF-8 text, Latin1 text, or binary hex snippet.
    /// </summary>
    public static void PrintAppLayer(byte[] raw)
    {
        // Try JSON first
        if (TryExtractJsonText(raw, out var json))
        {
            Console.WriteLine("=== JSON Payload ===");
            Console.WriteLine(json);
            return;
        }

        // UTF-8 check
        string asUtf8 = Encoding.UTF8.GetString(raw);
        if (asUtf8.TrimStart().Any(c => !char.IsControl(c)))
        {
            Console.WriteLine("=== UTF-8 Text Payload ===");
            Console.WriteLine(asUtf8);
            return;
        }

        // Latin1 fallback
        var latin = ExtractLatinText(raw);
        if (!string.IsNullOrEmpty(latin))
        {
            Console.WriteLine("=== Latin1 Text Payload ===");
            Console.WriteLine(latin);
            return;
        }

        // Hex-dump fallback
        var snippet = raw
            .Take(64)
            .Select(b => b.ToString("X2"))
            .Aggregate((a, b) => a + " " + b);
        Console.WriteLine("=== Binary Payload (Hex snippet) ===");
        Console.WriteLine(snippet + (raw.Length > 64 ? " …" : ""));
    }

    /// <summary>
    /// Attempts to extract JSON text from raw bytes.
    /// Returns true if bytes decode to valid JSON object or array.
    /// </summary>
    public static bool TryExtractJsonText(byte[] raw, out string jsonText)
    {
        jsonText = null!;
        try
        {
            var text = Encoding.UTF8.GetString(raw).Trim();
            // Quick check for JSON object/array
            if ((text.StartsWith("{") && text.EndsWith("}")) ||
                (text.StartsWith("[") && text.EndsWith("]")))
            {
                JsonDocument.Parse(text);
                jsonText = text;
                return true;
            }
        }
        catch
        {
            // invalid JSON
        }
        return false;
    }

    /// <summary>
    /// Parses an mDNS (UDP/5353) payload into a DNS message.
    /// </summary>
    public static DnsMessage ParseMdns(byte[] raw)
    {
        return DnsMessage.Parse(raw);
    }

    /// <summary>
    /// Extracts a cleaned Latin1 string from raw bytes,
    /// removing non-printable characters,
    /// preserving ASCII and extended Latin.
    /// </summary>
    public static string? ExtractLatinText(byte[] raw)
    {
        string decoded = Encoding.Latin1.GetString(raw);
        var filtered = new string(decoded
            .Where(c => (c >= 0x20 && c <= 0x7E) || (c >= 0xA0 && c <= 0xFF))
            .ToArray());
        if (filtered.Any(char.IsLetterOrDigit))
            return filtered.Trim();
        return null;
    }

    /// <summary>
    /// Formats a parsed DNS message into a human-readable string.
    /// </summary>
    public static string FormatMdnsMessage(byte[] raw)
    {
        var msg = ParseMdns(raw);
        var sb = new StringBuilder();

        sb.AppendLine(";; HEADER ;;");
        sb.AppendLine($"ID:       {msg.TransactionID}");
        sb.AppendLine($"Flags:    {(msg.IsQuery ? "Query" : "Response")}, " +
                      $"{(msg.IsAuthoritiveAnswer ? "AA" : "")} " +
                      $"{(msg.IsTruncated ? "TC" : "")} " +
                      $"{(msg.IsRecursionDesired ? "RD" : "")} ");
        sb.AppendLine($"Questions: {msg.Questions.Count}, Answers: {msg.AnswerRecords.Count}");
        sb.AppendLine();

        if (msg.Questions.Any())
        {
            sb.AppendLine(";; QUESTIONS ;;");
            foreach (var q in msg.Questions)
                sb.AppendLine($"  {q.Name}  {q.RecordType}  {q.RecordClass}");
            sb.AppendLine();
        }

        if (msg.AnswerRecords.Any())
        {
            sb.AppendLine(";; ANSWERS ;;");
            foreach (var a in msg.AnswerRecords)
                sb.AppendLine($"  {a.Name}  {a.RecordType}  {a.RecordClass}  TTL={a.TimeToLive}  Data={a}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
