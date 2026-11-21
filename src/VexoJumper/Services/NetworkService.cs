using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using VexoJumper.Models;

namespace VexoJumper.Services;

public sealed class NetworkService
{
    public IReadOnlyList<NetworkAdapterInfo> GetAdapters()
    {
        var list = new List<NetworkAdapterInfo>();
        foreach (var network in NetworkInterface.GetAllNetworkInterfaces()
                     .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback))
        {
            try
            {
                var properties = network.GetIPProperties();
                var ipv4 = properties.UnicastAddresses
                    .FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString() ?? "—";
                var gateway = properties.GatewayAddresses
                    .FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString() ?? "—";
                var dns = properties.DnsAddresses.Select(x => x.ToString()).ToArray();
                var statistics = network.GetIPStatistics();
                list.Add(new(network.Name, network.Description, network.NetworkInterfaceType.ToString(),
                    network.OperationalStatus.ToString(), ipv4, gateway, dns,
                    statistics.BytesReceived, statistics.BytesSent));
            }
            catch (NetworkInformationException) { }
            catch (UnauthorizedAccessException) { }
        }
        return list;
    }

    /// <summary>
    /// Picks the active IPv4 interface with a default gateway.
    /// Interfaces without a gateway (virtual adapters, disconnected Wi-Fi, etc.) are not selected.
    /// </summary>
    public NetworkAdapterInfo? GetActiveAdapter()
    {
        try
        {
            // Ask Windows which local IPv4 address it would route traffic through.
            // UDP Connect selects a route but does not send a payload.
            string? routedLocalAddress = null;
            try
            {
                using var routeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                routeSocket.Connect(new IPEndPoint(IPAddress.Parse("1.1.1.1"), 53));
                routedLocalAddress = (routeSocket.LocalEndPoint as IPEndPoint)?.Address.ToString();
            }
            catch (SocketException) { }

            var candidates = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .Select(n =>
                {
                    try
                    {
                        var p = n.GetIPProperties();
                        var address = p.UnicastAddresses.FirstOrDefault(a =>
                            a.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(a.Address) &&
                            !a.Address.ToString().StartsWith("169.254.", StringComparison.Ordinal));
                        var gateway = p.GatewayAddresses.FirstOrDefault(g =>
                            g.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.Any.Equals(g.Address));
                        if (address is null || gateway is null) return null;
                        var stats = n.GetIPStatistics();
                        return new NetworkAdapterInfo(n.Name, n.Description, n.NetworkInterfaceType.ToString(),
                            n.OperationalStatus.ToString(), address.Address.ToString(), gateway.Address.ToString(),
                            p.DnsAddresses.Select(d => d.ToString()).ToArray(), stats.BytesReceived, stats.BytesSent);
                    }
                    catch (NetworkInformationException) { return null; }
                    catch (UnauthorizedAccessException) { return null; }
                })
                .Where(x => x is not null)
                .Cast<NetworkAdapterInfo>()
                .ToList();

            // Ethernet/Wi-Fi interfaces with a gateway are preferred; if multiple exist,
            // Windows' route metrics are not exposed by NetworkInterface, so prefer the
            // interface that currently reports the most recent traffic.
            if (!string.IsNullOrWhiteSpace(routedLocalAddress))
            {
                var routed = candidates.FirstOrDefault(x =>
                    x.IPv4.Equals(routedLocalAddress, StringComparison.OrdinalIgnoreCase));
                if (routed is not null) return routed;
            }

            return candidates
                .OrderByDescending(x => x.ReceivedBytes + x.SentBytes)
                .FirstOrDefault();
        }
        catch (NetworkInformationException) { return null; }
    }

    public async Task<(bool Success, long? RoundtripMs, string Message)> PingAsync(string host, int count, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253 || host.Any(char.IsWhiteSpace))
            return (false, null, "مقصد واردشده معتبر نیست.");
        count = Math.Clamp(count, 1, 20);
        try
        {
            using var ping = new Ping();
            long total = 0; int received = 0;
            for (int i = 0; i < count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var reply = await ping.SendPingAsync(host, 2500).WaitAsync(ct);
                if (reply.Status == IPStatus.Success) { total += reply.RoundtripTime; received++; }
            }
            return received > 0
                ? (true, total / received, $"پاسخ موفق: {received} از {count}")
                : (false, null, "هیچ پاسخی دریافت نشد؛ مقصد یا مسیر شبکه در دسترس نیست.");
        }
        catch (OperationCanceledException) { return (false, null, "آزمایش لغو شد."); }
        catch (Exception ex) { return (false, null, $"خطا: {ex.Message}"); }
    }

    public async Task<(bool Success, long ElapsedMs, string Addresses, string Message)> LookupAsync(string host, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253 || host.Any(char.IsWhiteSpace))
            return (false, 0, "", "نام دامنه معتبر نیست.");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host, ct);
            stopwatch.Stop();
            return (addresses.Length > 0, stopwatch.ElapsedMilliseconds,
                string.Join("\n", addresses.Select(a => a.ToString())),
                addresses.Length > 0 ? "پاسخ دریافت شد." : "رکوردی پیدا نشد.");
        }
        catch (Exception ex) { stopwatch.Stop(); return (false, stopwatch.ElapsedMilliseconds, "", ex.Message); }
    }
}
