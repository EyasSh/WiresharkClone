using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main()
    {
        while (true)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/k echo you've been hacked",
                CreateNoWindow = false,
                UseShellExecute = true
            });

            Thread.Sleep(5000); // 5 second delay
        }
    }
}
