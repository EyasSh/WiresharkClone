using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server.Services
{
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
        /// Gets the CPU usage percentage for the current system.
        /// On Windows, this method uses the PerformanceCounter class to query the CPU usage.
        /// On other platforms, this method uses the <see cref="GetCpuUsagePercentageCrossPlatform"/> method instead.
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
        /// Gets the CPU usage percentage for the current system using a cross-platform approach.
        /// This method takes two measurements of the total CPU time, waits briefly, and then subtracts the two measurements.
        /// This difference is then divided by the number of processors and the amount of time that elapsed between the two measurements.
        /// The result is the CPU usage percentage.
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
        /// </summary>
        /// <returns>
        /// The total CPU time for all processes on the system.
        /// </returns>
        /// <remarks>
        /// This method iterates over all processes on the system and adds their CPU times together.
        /// Some processes may not allow access to their CPU time, in which case they are ignored.
        /// </remarks>
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
        /// Gets the percentage of physical memory that is currently in use.
        /// </summary>
        /// <returns>
        /// The percentage of physical memory that is currently in use.
        /// </returns>
        /// <remarks>
        /// This method is only supported on Windows.
        /// </remarks>
        public double GetRamUsagePercentage()
        {
            if (!OperatingSystem.IsWindows())
            {
                return -1;
            }
            using (var memoryCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use"))
            {
                return Math.Round(memoryCounter.NextValue(), 2); // Return memory usage percentage
            }
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