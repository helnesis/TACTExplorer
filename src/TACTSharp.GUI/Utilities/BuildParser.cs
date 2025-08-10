using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TACTSharp.GUI.Utilities;

public readonly record struct BuildRecord
{
    public readonly string Active { get; init; }
    public readonly string BuildKey { get; init; }
    public readonly string CdnKey { get; init; }
    public readonly string InstallKey { get; init; }
    public readonly string ImSize { get; init; }
    public readonly string CdnPath { get; init; }
    public readonly string CdnHosts { get; init; }
    public readonly string CdnServers { get; init; }
    public readonly string Tags { get; init; }
    public readonly string Armadillo { get; init; }
    public readonly string LastActivated { get; init; }
    public readonly string Version { get; init; }
    public readonly string KeyRing { get; init; }
    public readonly string Product { get; init; }
    public readonly string BuildId { get; init; }
    public readonly string ProductConfig { get; init; } 
    public readonly string ConfigPath { get; init; }

}
public static class BuildParser
{
    private static readonly Dictionary<string, BuildRecord> Configs = [];       
    public static BuildRecord GetRecord(string region)
    {
        if (Configs.Count <= 0) throw new IndexOutOfRangeException(nameof(region));

        if (Configs.TryGetValue(region, out var record))
            return record;
        
        var f = Configs.Keys.First();
        return GetRecord(f);
    }
    public static async Task Parse(Stream stream)
    {
        Configs.Clear();

        using var reader = new StreamReader(stream);

        var header = await reader.ReadLineAsync()
            ?? throw new InvalidDataException("Malformatted file, parsing aborted.");

        var indexMap = header.Split('|')
            .Select(k => k.Split('!')[0]).Index()
            .ToFrozenDictionary(k => k.Item, v => v.Index);
        
        while (await reader.ReadLineAsync() is { } rawRecord)
        {
            if (rawRecord.StartsWith('#')) continue;
            var records = rawRecord.Split("|");
            var regionKey = records[0];
            
            Configs
                 .TryAdd(regionKey, new BuildRecord()
                 {
                     Active = Get(records, indexMap, "Active"),
                     BuildKey = Get(records, indexMap, "Build Key", "BuildConfig"),
                     CdnKey = Get(records, indexMap, "CDN Key", "CDNConfig"),
                     InstallKey = Get(records, indexMap, "Install Key"),
                     ImSize = Get(records, indexMap, "IM Size"),
                     CdnPath = Get(records, indexMap, "CDN Path"),
                     CdnHosts = Get(records, indexMap, "CDN Hosts"),
                     CdnServers = Get(records, indexMap, "CDN Servers"),
                     Tags = Get(records, indexMap, "Tags"),
                     Armadillo = Get(records, indexMap, "Armadillo"),
                     LastActivated = Get(records, indexMap, "Last Activated"),
                     Version = Get(records, indexMap, "Versions", "VersionsName"),
                     KeyRing = Get(records, indexMap, "KeyRing"),
                     Product = Get(records, indexMap, "Product"),
                     BuildId = Get(records, indexMap, "BuildId"),
                     ConfigPath = Get(records, indexMap, "ConfigPath"),
                     ProductConfig = Get(records, indexMap, "ProductConfig")
                 });
        }
        
        return;
        
        static string Get(string[] records, IReadOnlyDictionary<string, int> map, string primary, string? alias = null)
        {
            var output = string.Empty;
            
            if (map.TryGetValue(primary, out var index) && index < records.Length) output = records[index]; 
            if (alias is not null && map.TryGetValue(alias, out index) && index < records.Length) output = records[index];
            
            return output;
        }
    }

}