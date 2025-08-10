using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TACTSharp.GUI.Models;
using TACTSharp.GUI.Services.Listfile;
using TACTSharp.GUI.ViewModels.Configuration;

namespace TACTSharp.GUI.Services.Tact;

public interface ITactService
{
    const string CacheDirectory = "cache";

    TactEntry? Entry { get; set; }

    /// <summary>
    /// Returns the current build instance.
    /// </summary>
    BuildInstance? Instance { get; }
    
    /// <summary>
    /// Returns the local game directory that will be used by the build instance.
    /// </summary>
    string? GameDirectory { get; }
    
    /// <summary>
    /// Returns the cdn config used by the build instance.
    /// </summary>
    string? CdnConfig { get; }
    
    /// <summary>
    /// Returns the build config used by the build instance.
    /// </summary>
    string? BuildConfig { get; }
    
    /// <summary>
    /// Initializes the TACT service, by loading the build instance.
    /// </summary>
    Task Initialize();
    
    /// <summary>
    /// Loads the specified server information into the build instance.
    /// </summary>
    /// <param name="serverInfo"></param>
    /// <returns></returns>
    Task LoadOnlineStorage(ServerInfo serverInfo);
}