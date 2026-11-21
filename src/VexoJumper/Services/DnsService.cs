using System.Diagnostics;
using System.Net.NetworkInformation;
namespace VexoJumper.Services;
public sealed class DnsService
{
    public async Task<(bool Success, string Message)> SetDnsAsync(string adapterName, string? primary, string? secondary, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(adapterName) || !NetworkInterface.GetAllNetworkInterfaces().Any(n => n.Name == adapterName)) return (false, "رابط شبکه انتخاب‌شده معتبر نیست.");
        if (!string.IsNullOrWhiteSpace(primary) && (!System.Net.IPAddress.TryParse(primary, out var primaryIp) || primaryIp.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)) return (false, "در این نسخه فقط DNS از نوع IPv4 پشتیبانی می‌شود.");
        if (!string.IsNullOrWhiteSpace(secondary) && (!System.Net.IPAddress.TryParse(secondary, out var secondaryIp) || secondaryIp.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)) return (false, "در این نسخه فقط DNS جایگزین از نوع IPv4 پشتیبانی می‌شود.");
        if (string.IsNullOrWhiteSpace(primary)) return (false, "برای بازگردانی DHCP از گزینه اختصاصی استفاده کنید.");
        var first = await RunNetshAsync(new[] { "interface", "ip", "set", "dns", $"name={adapterName}", "static", primary, "primary" }, ct);
        if (!first.Success) return first;
        if (!string.IsNullOrWhiteSpace(secondary))
        {
            var second = await RunNetshAsync(new[] { "interface", "ip", "add", "dns", $"name={adapterName}", secondary, "index=2" }, ct);
            if (!second.Success) return (false, "DNS اصلی اعمال شد اما DNS دوم اعمال نشد: " + second.Message);
        }
        return (true, "دستور تغییر DNS با موفقیت اجرا شد. برای اطمینان، رابط شبکه را دوباره بررسی کنید.");
    }
    public Task<(bool Success, string Message)> RestoreDhcpAsync(string adapterName, CancellationToken ct = default)
    {
        if (!NetworkInterface.GetAllNetworkInterfaces().Any(n => n.Name == adapterName)) return Task.FromResult((false, "رابط شبکه معتبر نیست."));
        return RunNetshAsync(new[] { "interface", "ip", "set", "dns", $"name={adapterName}", "dhcp" }, ct);
    }
    private static async Task<(bool Success, string Message)> RunNetshAsync(IEnumerable<string> args, CancellationToken ct)
    {
        try
        {
            using var p = new Process { StartInfo = new ProcessStartInfo("netsh.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true } };
            foreach (var arg in args) p.StartInfo.ArgumentList.Add(arg);
            p.Start(); var stdout = p.StandardOutput.ReadToEndAsync(ct); var stderr = p.StandardError.ReadToEndAsync(ct);
            await p.WaitForExitAsync(ct); var output = await stdout; var error = await stderr;
            if (p.ExitCode == 0) return (true, string.IsNullOrWhiteSpace(output) ? "عملیات انجام شد." : output.Trim());
            return (false, string.IsNullOrWhiteSpace(error) ? $"netsh با کد {p.ExitCode} پایان یافت." : error.Trim());
        }
        catch (Exception ex) { return (false, "اجرای عملیات شبکه ناموفق بود: " + ex.Message + " (برای تغییر DNS ممکن است دسترسی مدیر لازم باشد.)"); }
    }
}
