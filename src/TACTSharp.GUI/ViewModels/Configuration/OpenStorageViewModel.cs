using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using TACTSharp.GUI.Enums;
using TACTSharp.GUI.Messages;
using TACTSharp.GUI.Models;
using TACTSharp.GUI.Services.Listfile;
using TACTSharp.GUI.Services.Tact;
using TACTSharp.GUI.Utilities;

namespace TACTSharp.GUI.ViewModels.Configuration;

public sealed record ServerInfo(string Host, string Version, string BuildKey, string CdnKey, bool IsReachable);
public partial class OpenStorageViewModel(ITactService storageService, IConfiguration configuration, IListfileService listfile) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<ServerInfo> _availableServers = [];
    [ObservableProperty] private StorageType _storageType;
    [ObservableProperty] private string? _windowTitle;
    [ObservableProperty] private ServerInfo? _selectedServer;
    [ObservableProperty] private bool _isLoading;
    
    [RelayCommand]
    private async Task LoadStorage()
    {
        IsLoading = true;
        if (StorageType == StorageType.Online && SelectedServer is not null)
        {
            await storageService.LoadOnlineStorage(SelectedServer);
        }

        storageService.Entry = await LoadRoot();
        IsLoading = false;
        WeakReferenceMessenger.Default.Send(new CloseStorageMessage());
    }
    
    /// <summary>
    /// Loads available servers.
    /// </summary>
    [RelayCommand]
    private async Task LoadAvailableBuilds()
    {
        if (StorageType != StorageType.Online) return;
        await storageService.Initialize();
        
        using var client = new HttpClient();
        
        var official = await LoadRetailInfoAsync(client);

        var customServers = configuration.GetSection("Storage:Servers")
            .Get<ServerSettings[]>();

        AvailableServers.Add(official);
        
        if (customServers is { Length: > 0 })
        {
            foreach (var server in customServers)
            {
                try
                {
                    var stream = await client.GetStreamAsync(server.VersionsUri);
                    await BuildParser.Parse(stream);
                    var buildData = BuildParser.GetRecord(storageService.Instance!.Settings.Region);
                    AvailableServers.Add(new ServerInfo(server.Host, buildData.Version, buildData.BuildKey, buildData.CdnKey, true));
                }
                catch
                {
                    AvailableServers.Add(new ServerInfo(server.Host, "N/A", "N/A", "N/A", false));
                }
            }
        }
    }
    
    private async Task<TactEntry?> LoadRoot()
    {
        if (storageService.Instance?.Root is null
            || storageService.Instance?.Encoding is null
            || storageService.Instance?.FileIndex is null) return null;
        await listfile.LoadListfileAsync();

        var root = new TactEntryBuilder("Root", EntryType.Directory, null);
        
        foreach (var (key, value) in listfile.Listfile)
        {
            if (!storageService.Instance.Root.FileExists(key)) continue;
            
            var segments = value.Split('/');
            var node = root;

            var rootEntry = storageService.Instance.Root.GetEntriesByFDID(key);
            var encodingResult = storageService.Instance.Encoding.FindContentKey(rootEntry[0].md5.AsSpan());
            
            var meta = new FileMetaData
            {
                Size = SizeUnit.GetSize(encodingResult.DecodedFileSize),
                FileDataId = rootEntry[0].fileDataID,
                LocaleFlags = rootEntry[0].localeFlags,
                ContentFlags = rootEntry[0].contentFlags,
            };
            
            for(int i = 0; i < segments.Length; i++)
            {
                var name = segments[i];
                var type = (i == segments.Length - 1) ? EntryType.File : EntryType.Directory;

                node = node.GetOrAdd(name, type, type == EntryType.File ? meta : null);
            }
        }
        
        return root.ToTactEntry();
    }

    
    private async Task<ServerInfo> LoadRetailInfoAsync(HttpClient client)
    {
        var retailVersionsUri = $"http://{storageService.Instance!.Settings.Region}.patch.battle.net:1119/{storageService.Instance!.Settings.Product}/versions";
        var stream = await client.GetStreamAsync(retailVersionsUri);
        await BuildParser.Parse(stream);
            
        var buildData = BuildParser.GetRecord(storageService.Instance!.Settings.Region);
        return new ServerInfo("Official (Retail)", buildData.Version, buildData.BuildKey, buildData.CdnKey, true);
    }
    
}