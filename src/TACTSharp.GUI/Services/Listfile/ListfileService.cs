using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TACTSharp.GUI.Utilities;

namespace TACTSharp.GUI.Services.Listfile;

public sealed class ListfileService(IConfiguration configuration) : IListfileService
{
    private readonly Dictionary<uint, string> _listfile = [];
    
    public IReadOnlyDictionary<uint, string> Listfile
        => _listfile.ToFrozenDictionary();
    public string this[uint fileDataId]
        => Listfile.TryGetValue(fileDataId, out var value) ? value : $"Unk/{fileDataId}";

    public async Task DownloadListfileAsync()
    {
        var listfileUrl = 
            configuration.GetValue<string>("Listfile:Uri");
        
        var checkForUpdates =
            configuration.GetValue<bool>("Listfile:CheckForUpdates");
        
        using var client = new HttpClient();
        
        if (!File.Exists(Shared.ListfilePath))
        {
            var stream = await client.GetStreamAsync(listfileUrl);
            await using var fileStream = new FileStream(Shared.ListfilePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);
        }
        else
        {
            if (checkForUpdates)
            {
                var lastModified = File.GetLastWriteTimeUtc(Shared.ListfilePath);
                client.DefaultRequestHeaders.IfModifiedSince = lastModified;
            
                var response = await client.GetAsync(listfileUrl);
                if (response.StatusCode == HttpStatusCode.NotModified) return;

                var content = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(Shared.ListfilePath, FileMode.Create, FileAccess.Write);
                await content.CopyToAsync(fileStream);
            }
        }
    }

    public async Task LoadListfileAsync()
    {
        await using var file = File.OpenRead(Shared.ListfilePath);
        using var reader = new StreamReader(file);
  
        while (await reader.ReadLineAsync() is { } line)
        {
            var parts = line.Split(';', StringSplitOptions.TrimEntries);
                
            if (uint.TryParse(parts[0], out var fileDataId))
                _listfile.TryAdd(fileDataId, parts[1]);
        }
    }
}