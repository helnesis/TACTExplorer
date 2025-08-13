using System.Threading.Tasks;

namespace TACTSharp.GUI.ViewModels.Controls.Base;


public interface IFileVisualizationViewModel
{
    /// <summary>
    /// Initializes the ViewModel with the specified file bytes.
    /// </summary>
    /// <param name="fileBytes">File, in bytes.</param>
    Task LoadAsync(byte[] fileBytes);
}