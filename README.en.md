<div align="center">

# ⚡ Vexo Jumper

### Network management, made refreshingly simple.

**A modern, lightweight Windows desktop tool for network visibility and troubleshooting.**

<p>
  <a href="README.md">🇮🇷 فارسی</a>
  &nbsp;•&nbsp;
  <a href="README.en.md">🇬🇧 English</a>
</p>

<p>
  <a href="https://github.com/veyro404/VexoJumper/releases/latest">
    <img alt="Latest Release" src="https://img.shields.io/github/v/release/veyro404/VexoJumper?style=for-the-badge&color=6C8CFF&label=release">
  </a>
  <a href="https://github.com/veyro404/VexoJumper/releases">
    <img alt="Downloads" src="https://img.shields.io/github/downloads/veyro404/VexoJumper/total?style=for-the-badge&color=19BFA5&label=downloads">
  </a>
  <img alt="Platform" src="https://img.shields.io/badge/Windows-10%20%2F%2011-1687F8?style=for-the-badge&logo=windows">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet">
</p>

<p>
  <a href="https://github.com/veyro404/VexoJumper/releases/latest">
    <strong>⬇️ Download Latest Release</strong>
  </a>
  &nbsp; · &nbsp;
  <a href="https://veyro404.github.io/VexoJumper/">
    <strong>🌐 Project Website</strong>
  </a>
  &nbsp; · &nbsp;
  <a href="https://github.com/veyro404/VexoJumper/issues">
    <strong>🐛 Report an Issue</strong>
  </a>
</p>

---

</div>

## ✨ About Vexo Jumper

Vexo Jumper is a Windows desktop application designed to make **network management and troubleshooting easier**. It brings practical network tools together in one clean, readable interface, so users do not have to navigate long adapter lists or switch between separate utilities.

In version 1.2.0, the app identifies the IPv4 interface associated with the route selected by Windows, displays connection details, and provides tools such as Ping, DNS Lookup, traffic monitoring, and ready-to-use DNS profiles.

> **Less complexity. Essential network tools in one straightforward workspace.**

## 🚀 Download & Run

### Quick start — prebuilt release

1. Open the [Latest Releases](https://github.com/veyro404/VexoJumper/releases/latest) page.
2. Download `VexoJumper-win-x64.zip` from **Assets**.
3. Extract the ZIP archive to a folder.
4. Run `VexoJumper.exe`.

The release is published as a self-contained application, so a separate .NET Runtime installation is generally not required to run the release package. Administrator privileges may be required to change DNS settings.

<div align="center">

<a href="https://github.com/veyro404/VexoJumper/releases/latest">
  <img src="https://img.shields.io/badge/Download%20Vexo%20Jumper-Latest%20Release-586FFF?style=for-the-badge&logo=windows&logoColor=white" alt="Download Vexo Jumper">
</a>

</div>

## 🧩 Features

| Feature | Description |
|---|---|
| 🧭 Automatic network detection | Identifies the IPv4 interface associated with the route selected by Windows, without requiring manual selection from a long adapter list |
| 🌐 Connection details | Displays interface name, connection type, IPv4 address, gateway, and configured DNS servers |
| 📡 Ping | Sends ICMP requests to an IP address or hostname and displays successful replies and average response time |
| 🔎 DNS Lookup | Resolves host addresses using the DNS resolver configured in Windows |
| ⚙️ DNS profiles | Quickly selects predefined DNS providers and fills primary and secondary addresses |
| ↩️ Restore DNS | Allows the interface DNS configuration to be restored to automatic DHCP |
| 📊 Traffic monitoring | Displays receive and send rates using Windows network interface counters |
| 🎨 User interface | Dark, Persian, right-to-left interface with readable result cards and subtle animations |

## ⚡ Included DNS Profiles

| Provider | Primary DNS | Secondary DNS |
|---|---|---|
| Electro | `78.157.42.100` | `78.157.42.101` |
| Shecan | `178.22.122.100` | `185.51.200.2` |
| Begzar | `185.55.226.26` | `185.55.225.25` |
| Google Public DNS | `8.8.8.8` | `8.8.4.4` |
| Cloudflare | `1.1.1.1` | `1.0.0.1` |
| Quad9 | `9.9.9.9` | `149.112.112.112` |
| OpenDNS | `208.67.222.222` | `208.67.220.220` |
| AdGuard DNS | `94.140.14.14` | `94.140.15.15` |
| Safe DNS | `195.46.39.39` | `195.46.39.40` |
| NTT | `129.250.35.250` | `129.250.35.251` |

Select a profile to populate the DNS fields, then confirm before applying the settings.

**Note:** DNS performance, availability, and filtering behavior vary by network and geographic location. A listed profile does not guarantee faster access, security, or availability of any particular website.

## 🖥️ Requirements

| Item | Requirement |
|---|---|
| Operating system | Windows 10 or Windows 11 |
| Current release architecture | Windows x64 (`win-x64`) |
| Development framework | .NET 10 |
| UI framework | WPF |
| Administrator access | May be required to apply DNS changes |

## 🛠️ Build from Source

### 1. Clone the repository

```powershell
git clone https://github.com/veyro404/VexoJumper.git
cd VexoJumper
```

### 2. Check your development environment

Build on Windows with the .NET 10 SDK. The current build script expects SDK version `10.0.404`.

```powershell
dotnet --info
```

### 3. Build and publish

The PowerShell script restores dependencies, builds Release with warnings treated as errors, and publishes a self-contained Windows x64 application:

```powershell
Set-ExecutionPolicy -Scope Process Bypass -Force
.\build-and-publish.ps1 -SkipRun
```

The published application is created at:

```text
artifacts/win-x64/VexoJumper.exe
```

To build without the helper script:

```powershell
dotnet restore .\VexoJumper.sln
dotnet build .\VexoJumper.sln -c Release
```

## 🧪 Current Scope & Limitations

The following details help set accurate expectations:

- Ping uses ICMP. Some destinations block ICMP requests.
- DNS Lookup uses the resolver configured in Windows; it is not an independent benchmark of multiple DNS providers.
- Traffic monitoring uses Windows network interface counters and may include LAN traffic. It may not match the traffic totals reported by an ISP.
- Active interface detection uses the local address selected by Windows for a UDP route. On systems with multiple routes, VPNs, or custom policy routing, the result depends on the Windows routing table.
- A real download/upload speed test server, jitter measurement, and packet-loss diagnostics are not implemented in the current version.
- Traceroute, reverse DNS, TCP/TLS diagnostics, public IP lookup, per-process traffic breakdown, System Tray support, and a dedicated installer are outside the current release scope.
- Changing DNS may require Administrator privileges. If the operation fails, run the app with appropriate permissions and try again.

## 🔐 Security & Privacy

- DNS changes require explicit user confirmation.
- The application is designed to run normally with standard Windows privileges; Administrator access may be needed for privileged operations.
- Network tools read the Windows network configuration and interface information required for their operation.
- Review the source code and download releases only from the official repository.

## 🗺️ Roadmap

The following are potential future improvements and are **not** claims about features available in the current release:

- [ ] Real download and upload speed testing
- [ ] DNS provider latency comparison
- [ ] Network usage charts and history
- [ ] Traceroute and route diagnostics
- [ ] Public IP information
- [ ] Windows installer
- [ ] Automatic updates
- [ ] Automated builds with GitHub Actions

## 🤝 Contributing & Bug Reports

Suggestions, bug reports, and contributions are welcome.

1. Check [Issues](https://github.com/veyro404/VexoJumper/issues) for existing reports.
2. When reporting a bug, include your Windows version, reproduction steps, and the exact error message.
3. For larger changes, open an Issue first so the approach can be discussed.

## 👨‍💻 Developer

Created by **[VEYRO404](https://github.com/veyro404)**

- 🌐 Website: [Vexo Jumper](https://veyro404.github.io/VexoJumper/)
- 📦 Releases: [GitHub Releases](https://github.com/veyro404/VexoJumper/releases)
- 🐛 Support and bug reports: [GitHub Issues](https://github.com/veyro404/VexoJumper/issues)

## 📄 License

Until a `LICENSE` file is added to the repository, the project remains subject to the default copyright terms. If you intend to distribute it as an open-source project, choose an appropriate license and add its full text to the repository.

---

<div align="center">

**Vexo Jumper — Take control of your network, simply.**

If you find this project useful, consider giving the repository a ⭐.

<sub>Made with care by VEYRO404</sub>

</div>
