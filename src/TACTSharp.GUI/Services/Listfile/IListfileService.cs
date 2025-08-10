using System.Collections.Generic;
using System.Threading.Tasks;

namespace TACTSharp.GUI.Services.Listfile;

public interface IListfileService
{
    const string VerifiedListfileUri =
        "https://github.com/wowdev/wow-listfile/releases/latest/download/verified-listfile.csv";

    IReadOnlyDictionary<uint, string> Listfile { get; }
    
    /// <summary>
    /// Gets the filepath affiliated to the provided fileDataId.
    /// </summary>
    /// <param name="fileDataId">File ID.</param>
    string this[uint fileDataId] { get; }
    
    /// <summary>
    /// Downloads the latest listfile from the verified URI, see <see cref="VerifiedListfileUri"/>
    /// </summary>
    Task DownloadListfileAsync();

    /// <summary>
    /// Loads the listfile.
    /// </summary>
    Task LoadListfileAsync();
}