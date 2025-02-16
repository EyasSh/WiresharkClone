using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class PerformanceRequest
{
    public required double CpuUsage { get; set; }
    public required double RamUsage { get; set; }
    public required double DiskUsage { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
}