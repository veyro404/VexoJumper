namespace VexoJumper.Models;
public sealed record NetworkAdapterInfo(string Name, string Description, string Type, string Status, string IPv4, string Gateway, string[] DnsServers, long ReceivedBytes, long SentBytes);
