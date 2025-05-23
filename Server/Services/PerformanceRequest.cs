using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a request for performance metrics.
/// Contains properties for CPU, RAM, and disk usage, as well as the user's email and name.
/// </summary>
public class PerformanceRequest
{
    public required double CpuUsage { get; set; }
    public required double RamUsage { get; set; }
    public required double DiskUsage { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
}