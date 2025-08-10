using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TACTSharp.GUI.Models;
using TACTSharp.GUI.Services.Listfile;
using TACTSharp.GUI.ViewModels.Configuration;

namespace TACTSharp.GUI.Services.Tact;

public sealed class TactService(IConfiguration configuration, IListfileService listfile) : ITactService
{
    public TactEntry? Entry { get; set; }
    public BuildInstance? Instance { get; private set; }
    public string? GameDirectory { get; private set; }
    
    public string? CdnConfig { get; private set; }
    
    public string? BuildConfig { get; private set; }
    public async Task Initialize()
    {
        var storageSection = configuration.GetSection("Storage")
            .Get<StorageSettings>();

        await listfile.DownloadListfileAsync();
        
        Instance = new BuildInstance 
        {
            Settings =
            {
                CacheDir = ITactService.CacheDirectory,
                BaseDir = GameDirectory,
                Region = storageSection?.Region  ?? "eu",
                Product = storageSection?.Product ?? "wow",
                Locale = storageSection?.Locale ?? RootInstance.LocaleFlags.enGB,
                AdditionalCDNs = storageSection?.AdditionalServers.Select(s => s.Host).ToList() ?? []
            }
        };
    }

    public async Task LoadOnlineStorage(ServerInfo serverInfo)
    {
        if (Instance is null) return;
        
        var work = Task.Run(() =>
        {
            Instance.LoadConfigs(serverInfo.BuildKey, serverInfo.CdnKey);
            Instance.Load();
        });
        
        await work;
    }
    
}