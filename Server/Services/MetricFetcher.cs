using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Management;
using System.Threading;

namespace Server.Services
{
    /// <summary>
    /// This class provides methods to fetch system metrics such as CPU, RAM, and disk usage.
    /// </summary>
    public class MetricFetcher
    {
        /// <summary>
        /// Gets a snapshot of system metrics in a tuple of 3 doubles: CPU usage, RAM usage, and disk usage.
        /// </summary>
        /// <returns>
        /// A tuple of 3 doubles representing the current system metrics.
        /// </returns>
        public (double cpuUsage, double ramUsage, double diskUsage) GetSystemMetrics()
        {
            double cpuUsage = GetCpuUsagePercentage();
            double ramUsage = GetRamUsagePercentage();
            double diskUsage = GetDiskUsagePercentage();

            return (cpuUsage, ramUsage, diskUsage);
        }


        /// <summary>
        /// Gets the CPU usage percentage of the current system.
        /// On Windows, this method uses the PerformanceCounter class to query the system's CPU usage.
        /// On other platforms, this method measures the time it takes to execute a small delay and uses that
        /// to calculate the CPU usage.
        /// </summary>
        /// <returns>
        /// The CPU usage percentage of the current system.
        /// </returns>
        private double GetCpuUsagePercentage()
        {
            if (OperatingSystem.IsWindows())
            {
                using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    cpuCounter.NextValue(); // Initial call to set up
                    Thread.Sleep(500); // Small delay for an accurate reading
                    return Math.Round(cpuCounter.NextValue(), 2); // Return CPU usage percentage
                }
            }
            else
            {
                return GetCpuUsagePercentageCrossPlatform();
            }
        }


        /// <summary>
        /// Gets the CPU usage percentage of the current system using a cross-platform approach.
        /// This method measures the time it takes to execute a small delay and uses that to calculate the CPU usage.
        /// </summary>
        /// <returns>
        /// The CPU usage percentage of the current system.
        /// </returns>
        private double GetCpuUsagePercentageCrossPlatform()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = GetTotalCpuTime();
            Thread.Sleep(500);
            var endTime = DateTime.UtcNow;
            var endCpuUsage = GetTotalCpuTime();

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return Math.Round(cpuUsageTotal * 100, 2);
        }


        /// <summary>
        /// Gets the total CPU time for all processes on the system.
        /// This method iterates over all processes and sums up their total CPU time.
        /// If a process does not allow access to its CPU time, it is ignored.
        /// </summary>
        /// <returns>
        /// The total CPU time for all processes on the system.
        /// </returns>
        private TimeSpan GetTotalCpuTime()
        {
            var totalCpuTime = new TimeSpan();
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    totalCpuTime += proc.TotalProcessorTime;
                }
                catch
                {
                    // Some processes may not allow access to their CPU time
                }
            }
            return totalCpuTime;
        }


        /// <summary>
        /// Gets the physical memory usage percentage of the current system.
        /// </summary>
        /// <returns>
        /// The physical memory usage percentage of the current system, or -1 if the system is not Windows.
        /// </returns>
        /// <remarks>
        /// This method uses the <see cref="ManagementObjectSearcher"/> class to query the system's memory details.
        /// It then calculates the percentage of used memory by subtracting the free memory from the total memory.
        /// The result is returned as a double value between 0 and 100.
        /// </remarks>
        public double GetRamUsagePercentage()
        {
            if (!OperatingSystem.IsWindows())
            {
                return -1;
            }

            // Use ManagementObjectSearcher to query the system's memory details
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                var totalMemory = Convert.ToDouble(obj["TotalVisibleMemorySize"]); // Total physical memory (in KB)
                var freeMemory = Convert.ToDouble(obj["FreePhysicalMemory"]);     // Free physical memory (in KB)

                // Calculate percentage of used memory
                var usedMemory = totalMemory - freeMemory;
                return Math.Round((usedMemory / totalMemory) * 100, 2); // Return the usage percentage
            }

            return -1; // If no data is retrieved
        }

        /// <summary>
        /// Gets the disk usage percentage for the current system.
        /// On Windows, this method uses the PerformanceCounter class to query the disk usage.
        /// On other platforms, this method currently returns -1, as cross-platform calculation is not implemented.
        /// </summary>
        /// <returns>
        /// The disk usage percentage of the current system on Windows, or -1 on non-Windows platforms.
        /// </returns>

        private double GetDiskUsagePercentage()
        {
            if (OperatingSystem.IsWindows())
            {
                using (var diskCounter = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "_Total"))
                {
                    diskCounter.NextValue(); // Initial call to set up
                    Thread.Sleep(500); // Small delay for an accurate reading
                    return Math.Round(diskCounter.NextValue(), 2); // Return the disk queue length
                }
            }
            else
            {
                // Implement cross-platform disk usage calculation if needed
                return -1;
            }
        }

    }
}