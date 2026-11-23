<div align="center">

# ⚡ Vexo Jumper VPN

### Your VPN connection and everyday network tools. One focused workspace.

**A modern Persian-first Windows VPN client with sing-box, server health checks, DNS tools and practical diagnostics.**

<p><a href="README.md">🇮🇷 فارسی</a> · <a href="README.en.md">🇬🇧 English</a></p>

<p>
  <img alt="Version" src="https://img.shields.io/badge/version-1.7.0-7C8CFF?style=for-the-badge">
  <img alt="Windows" src="https://img.shields.io/badge/Windows-10%20%7C%2011-1687F8?style=for-the-badge&logo=windows">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet">
  <img alt="Architecture" src="https://img.shields.io/badge/architecture-x64-17B890?style=for-the-badge">
</p>

<p>
  <a href="https://github.com/veyro404/VexoJumper/releases/latest"><strong>⬇️ Download latest release</strong></a> ·
  <a href="https://veyro404.github.io/VexoJumper/"><strong>🌐 Project website</strong></a> ·
  <a href="https://github.com/veyro404/VexoJumper/issues"><strong>🐛 Report an issue</strong></a>
</p>

</div>

---

## ✨ What is Vexo Jumper?

Vexo Jumper VPN is a Windows desktop application that brings VPN connectivity and practical network tools into one interface. Check your connection, manage DNS, run diagnostics and monitor network activity without jumping between multiple utilities.

Version **1.7.0** includes a **sing-box**-based VPN tunnel, a project-managed configuration feed, server reachability ranking, real HTTPS tunnel verification, automatic recovery checks and an in-app update workflow.

> A clearer way to work with your connection—with useful status and diagnostics instead of guesswork.

## 🚀 Download & get started

### Prebuilt Windows release

1. Open [GitHub Releases](https://github.com/veyro404/VexoJumper/releases/latest).
2. Download the Windows ZIP from **Assets** and extract it. If a Setup installer is attached to the release, you can use the x64 installer instead.
3. Run `VexoJumperVPN.exe`.
4. Approve the Windows Administrator prompt required for VPN tunnel and network operations.
5. Open the VPN page and press the central power control.

The published application is self-contained and does not require a separate .NET Runtime installation. On the first VPN connection, the app may download the sing-box engine and Wintun driver from their official sources. Internet access is therefore required for initial VPN setup.

<div align="center">
<a href="https://github.com/veyro404/VexoJumper/releases/latest"><img src="https://img.shields.io/badge/Download%20Vexo%20Jumper-Free%20on%20GitHub-596CFF?style=for-the-badge&logo=windows&logoColor=white" alt="Download Vexo Jumper"></a>
</div>

## 💎 Features

### 🛡️ sing-box VPN client

- Creates a Windows TUN tunnel using the sing-box engine.
- The configuration parser handles common link formats including VLESS, VMess, Shadowsocks, Trojan, Hysteria2 and TUIC. Actual usability depends on the protocol details and completeness of each supplied configuration.
- Retrieves server entries from the project's configuration-management service and keeps a local cache for fallback use.
- Checks TCP reachability and ranks reachable servers by response time before attempting the top candidates.
- Reports a successful connection only after the engine starts and an HTTPS request succeeds through the tunnel.
- Monitors tunnel health and attempts automatic reconnection if the engine exits or the tunnel becomes unreachable.
- Uses a Windows Job Object to help terminate the VPN child process if the app is forcibly closed.
- Displays connection state, latency, traffic rates and connection duration.

### 🌐 DNS & network management

- Automatically identifies the network interface selected by the Windows route table.
- Shows interface name, connection type, IPv4 address, gateway and configured DNS servers.
- Provides selectable DNS profiles and editable primary/secondary DNS fields.
- Applies DNS settings after user confirmation and can restore automatic DHCP DNS.
- Shows response latency for built-in and custom DNS addresses on the Network page.
- Includes Electro, Shecan, Begzar, Google Public DNS, Cloudflare, Quad9, OpenDNS, AdGuard, Safe DNS and NTT profiles.

### 🧰 Diagnostics toolkit

| Tool | What it does |
|---|---|
| Ping | Sends ICMP requests to a host or IP and reports response times |
| DNS Lookup | Resolves a hostname using the DNS resolver configured in Windows |
| Smart Diagnosis | Checks the active interface, gateway, internet reachability and DNS |
| DNS Benchmark | Compares DNS response times on the current network |
| Quick Fix | Runs selected Windows network repair actions, such as flushing DNS cache and resetting Winsock/IP |
| Live Traffic Monitor | Displays receive/send rates and interface traffic counters |
| Connection History | Records VPN, connection and network-operation events |

### ✨ User experience

- Dark, right-to-left Persian UI with glass-style panels and neon accents.
- Animated central power control for disconnected, connecting and connected states.
- System Tray operation with reopen, VPN disconnect and full-exit actions.
- In-app updates with resumable downloads, SHA-256 verification and support for mandatory releases.

## 🔄 Version 1.7.0 update workflow

The app checks the project's update service for newer releases. When an update is available, users can review its changelog and download it in-app. The downloaded package and executable are checked with SHA-256. The updater stages the new executable, waits for the running app to exit, replaces the installed file and attempts rollback if replacement fails. The service can also mark a release as mandatory.

## 🖥️ Requirements

| Item | Requirement |
|---|---|
| Operating system | Windows 10 or Windows 11 |
| Main release architecture | x64 |
| Framework | .NET 10 / WPF |
| Permissions | Administrator for VPN tunnel and selected network operations |
| Internet | Required for initial engine/driver download, server feed and update checks |

## 🧱 Build from source

Prerequisite: Windows and **.NET SDK 10.0.404** (the SDK version expected by the build script).

```powershell
git clone https://github.com/veyro404/VexoJumper.git
cd VexoJumper
Set-ExecutionPolicy -Scope Process Bypass -Force
.\build-and-publish.ps1 -Runtime win-x64 -SkipRun
```

The published executable is written to:

```text
artifacts\win-x64\VexoJumperVPN.exe
```

The helper script restores dependencies, builds Release with warnings treated as errors, and publishes a self-contained single-file application. To create a Setup installer quickly, first publish the app and run:

```powershell
.\build-installer.ps1
```

The helper locates Inno Setup 6 and creates `artifacts\installer\VexoJumperVPN-Setup-1.7.0-x64.exe`. You can also open `installer\VexoJumperVPN.iss` in Inno Setup Compiler and compile it manually.

## 🔐 Privacy & security notes

- DNS changes require explicit user confirmation.
- Administrator access is required by the application manifest for VPN and privileged network operations.
- Server configuration and update metadata are retrieved from the project's online services.
- The engine and driver are downloaded from upstream sources; the Wintun archive is SHA-256 checked in code.
- Operational and error logs are written under `%LOCALAPPDATA%\VexoJumper\logs`. Review logs for sensitive information before sharing them.
- A VPN does not guarantee complete anonymity, device security, a specific speed or permanent access to any website. Performance and availability depend on the server, network, ISP and destination.

## ⚠️ Scope & limitations

- Ping relies on ICMP; some destinations block ICMP even when their services are reachable.
- DNS latency and benchmark results are snapshots of the current network and can change over time.
- Traffic rates use Windows network-interface counters and may include LAN traffic.
- A successful TCP probe alone does not prove a working VPN. The app also checks HTTPS connectivity through the tunnel after startup.
- Availability of servers supplied by the project's feed is outside the application's control.
- DNS changes and Winsock/IP repair operations can temporarily interrupt connectivity and may require elevation.

## 🧩 Repository layout

```text
VexoJumper/
├─ src/VexoJumper/       # WPF application source
├─ docs/                 # Documentation and release notes
├─ installer/            # Inno Setup script
├─ index.html            # Project landing page
├─ README.md             # Persian documentation
├─ README.en.md          # English documentation
├─ CHANGELOG.md          # Release history
└─ build-and-publish.ps1 # Windows build and publish helper
```

## 🤝 Issues & contributions

Please use [GitHub Issues](https://github.com/veyro404/VexoJumper/issues) for bug reports and suggestions. Include the app version, Windows version, reproduction steps and the exact error text. Logs are stored in `%LOCALAPPDATA%\VexoJumper\logs`; review them for private data before attaching them.

## 👨‍💻 Project links

- Developer: [VEYRO404](https://github.com/veyro404)
- Repository: [github.com/veyro404/VexoJumper](https://github.com/veyro404/VexoJumper)
- Releases: [GitHub Releases](https://github.com/veyro404/VexoJumper/releases)
- Website: [veyro404.github.io/VexoJumper](https://veyro404.github.io/VexoJumper/)

## 📄 License

No `LICENSE` file is included in this source package. Do not assume the project is open source or freely redistributable until an explicit project license is added. Third-party notices are listed in `docs/THIRD-PARTY-NOTICES.md`.
