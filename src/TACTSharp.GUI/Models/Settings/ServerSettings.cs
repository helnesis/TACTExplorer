using System.Collections.Generic;

namespace TACTSharp.GUI.Models;

public sealed class ServerSettings
{
    /// <summary>
    /// Gets or set the hostname of the server that hosts the CDN.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the versions URI file.
    /// </summary>
    public string? VersionsUri { get; set; } = null;
    
    /// <summary>
    /// Gets or sets the CDN URI file.
    /// </summary>
    public string? CdnsUri { get; set; } = null;
}