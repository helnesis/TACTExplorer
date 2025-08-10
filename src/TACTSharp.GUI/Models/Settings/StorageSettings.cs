using System.Collections.Generic;

namespace TACTSharp.GUI.Models;

public sealed class StorageSettings
{
    public string Region { get; set; } = "eu";
    public string Product { get; set; } = "wow";
    public RootInstance.LocaleFlags Locale { get; set; } = RootInstance.LocaleFlags.enGB;
    public List<ServerSettings> AdditionalServers { get; set; } = [];
}