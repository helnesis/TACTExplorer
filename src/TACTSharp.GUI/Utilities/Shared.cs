using System.IO;

namespace TACTSharp.GUI.Utilities;

public static class Shared
{
    public const string DepsFolder = "deps";
    public const string SettingsFileName = "settings.json";
    public const string ListfileFileName = "listfile.csv";

    public static string ListfilePath => Path.Combine(DepsFolder, ListfileFileName);

    /// <summary>
    /// Initializes shared resources for the whole application.
    /// </summary>
    public static void Initialize()
    {
        if (Directory.Exists(DepsFolder))
            Directory.CreateDirectory(DepsFolder);
        
    }
}