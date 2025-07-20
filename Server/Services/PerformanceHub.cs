using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Server.DB;
using Server.Models;

namespace Server.Services
{
    public interface IHubService
    {
        Task ConnectNotification(string sid, string warningLevel);
        Task ReceiveMetrics(double cpuUsage, double ramUsage, double diskUsage);
        Task ReceivePackets(PacketInfo[] packets);
    }

    public class SocketService : Hub<IHubService>
    {
        private static long _globalCount = 0;
        private readonly MongoDBWrapper _mongoDBWrapper;
        private readonly IMongoCollection<Device> _devicesCollection;
        private readonly EmailService _emailService;
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        private static ConcurrentQueue<Device> _devices = new();

        // Time interval to throttle email notifications
        private static readonly TimeSpan EmailInterval = TimeSpan.FromMinutes(30);
        // Tracks last email sent time per user (email address)
        private static readonly ConcurrentDictionary<string, DateTime> _lastEmailSent = new();

        public SocketService(MongoDBWrapper mongoDBWrapper, EmailService emailService)
        {
            _mongoDBWrapper = mongoDBWrapper;
            _devicesCollection = mongoDBWrapper.Devices;
            _emailService = emailService;
        }

        public override async Task OnConnectedAsync()
        {
            var sid = Context.ConnectionId;
            _connections[sid] = Context?.User?.Identity?.Name ?? string.Empty;

            try
            {
                await Clients.Caller.ConnectNotification(sid, "ok");

                var devicesList = await _devicesCollection.Find(_ => true).ToListAsync();
                if (devicesList.Any())
                {
                    _globalCount = devicesList[0].Counter;
                    foreach (var device in devicesList)
                        _devices.Enqueue(device);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.ConnectNotification(ex.Message, "err");
            }
        }

        public async Task GetMetrics(string email, string name)
        {
            var invocationNumber = Interlocked.Increment(ref _globalCount);
            var (cpuUsage, ramUsage, diskUsage) = new MetricFetcher().GetSystemMetrics();

            var cpu = _devices.FirstOrDefault(d => d.Name == "CPU");
            var ram = _devices.FirstOrDefault(d => d.Name == "RAM");
            var disk = _devices.FirstOrDefault(d => d.Name == "Disk");

            if (cpu == null || ram == null || disk == null)
            {
                Console.WriteLine("One or more devices are null, cannot update metrics.");
                await Clients.Caller.ReceiveMetrics(cpuUsage, ramUsage, diskUsage);
                return;
            }

            // First invocation: initialize averages without deviation
            if (invocationNumber == 1)
            {
                InitializeDevice(cpu, cpuUsage);
                InitializeDevice(ram, ramUsage);
                InitializeDevice(disk, diskUsage);

                await Clients.Caller.ReceiveMetrics(cpuUsage, ramUsage, diskUsage);
                return;
            }

            // Subsequent invocations: update metrics and check deviation
            bool cpuAlert = UpdateDevice(cpu, cpuUsage, out double cpuStdDev);
            bool ramAlert = UpdateDevice(ram, ramUsage, out double ramStdDev);
            bool diskAlert = UpdateDevice(disk, diskUsage, out double diskStdDev);

            // Email throttle check
            bool shouldSendEmail = false;
            var now = DateTime.UtcNow;
            if (cpuAlert || ramAlert || diskAlert)
            {
                if (!_lastEmailSent.TryGetValue(email, out var lastSent) || (now - lastSent) > EmailInterval)
                {
                    shouldSendEmail = true;
                    _lastEmailSent[email] = now;
                }
            }

            // Send email only if threshold exceeded and throttle interval passed
            if (shouldSendEmail)
            {
                var u = new User { Name = name, Email = email, Password = "dummy", date = DateOnly.FromDateTime(DateTime.Now) };
                var pdf = PdfGenerator.GeneratePerformancePdf(
                    (cpuUsage, ramUsage, diskUsage),
                    u,
                    cpu.AverageUsage,
                    ram.AverageUsage,
                    disk.AverageUsage,
                    cpuStdDev,
                    ramStdDev,
                    diskStdDev);

                await _emailService.SendEmailWithAttachmentAsync(
                    email,
                    $"{name}, Usage Report for {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                    $@"
                        <html>
                        <body>
                        Dear {name},
                        <p>Your device usage has exceeded normal thresholds.</p>
                        <p>Please find the attached performance report for details.</p>
                        <p>Best regards,</p>
                        <p>The Wire Tracer Team</p>
                        </body>
                        </html>",
                    pdf,
                    Guid.NewGuid() + ".pdf");
            }

            await Clients.Caller.ReceiveMetrics(cpuUsage, ramUsage, diskUsage);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var sid = Context.ConnectionId;
            _connections.TryRemove(sid, out _);
            Console.WriteLine($"Disconnected: {sid}");

            foreach (var device in _devices)
            {
                device.AverageUsage = device.Counter == 0
                    ? 0
                    : device.Sum / device.Counter;
                await _devicesCollection.ReplaceOneAsync(d => d.Id == device.Id, device);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>Initializes device metrics on first invocation</summary>
        private void InitializeDevice(Device device, double usage)
        {
            device.Sum = usage;
            device.Counter = 1;
            device.AverageUsage = usage;
            device.SumOfDeviations = 0;
            device.NumOfDeviations = 0;
        }

        /// <summary>
        /// Updates a device's metrics, computes its standard deviation, and checks if its current usage exceeds its previous average by its standard deviation.
        /// </summary>
        /// <param name="device">The device to update.</param>
        /// <param name="usage">The current usage of the device.</param>
        /// <param name="stdDev">The computed standard deviation of the device.</param>
        /// <returns>true if the device's current usage exceeds its previous average by its standard deviation, false otherwise.</returns>
        private bool UpdateDevice(Device device, double usage, out double stdDev)
        {
            // Update sum and count
            device.Sum += usage;
            device.Counter++;

            // Calculate deviation from previous average
            double previousAvg = device.AverageUsage;
            double deviation = usage - previousAvg;

            // Accumulate squared deviations
            device.SumOfDeviations += Math.Pow(deviation, 2);
            device.NumOfDeviations++;

            // Compute standard deviation
            stdDev = Math.Sqrt(device.SumOfDeviations / device.NumOfDeviations);

            // Update average usage
            device.AverageUsage = device.Sum / device.Counter;

            // Trigger if usage minus stdDev exceeds previous average
            return (usage - stdDev) > previousAvg;
        }
    }
}
