using System.Net.NetworkInformation;

namespace VexoJumper.Services;

public sealed class TrafficService
{
    private readonly Dictionary<string, (long Rx, long Tx)> _previous = new(StringComparer.OrdinalIgnoreCase);

    public (double DownloadBytesPerSecond, double UploadBytesPerSecond, long TotalReceived, long TotalSent)
        Sample(string? activeAdapterName = null)
    {
        long rxRate = 0, txRate = 0, rxTotal = 0, txTotal = 0;
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                        n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        (string.IsNullOrWhiteSpace(activeAdapterName) ||
                         n.Name.Equals(activeAdapterName, StringComparison.OrdinalIgnoreCase)));

        foreach (var network in interfaces)
        {
            try
            {
                var stats = network.GetIPStatistics();
                rxTotal += stats.BytesReceived;
                txTotal += stats.BytesSent;
                if (_previous.TryGetValue(network.Name, out var old))
                {
                    rxRate += Math.Max(0, stats.BytesReceived - old.Rx);
                    txRate += Math.Max(0, stats.BytesSent - old.Tx);
                }
                _previous[network.Name] = (stats.BytesReceived, stats.BytesSent);
            }
            catch (NetworkInformationException) { }
            catch (UnauthorizedAccessException) { }
        }

        // One-second timer in MainWindow determines the sampling interval.
        return (rxRate, txRate, rxTotal, txTotal);
    }
}
