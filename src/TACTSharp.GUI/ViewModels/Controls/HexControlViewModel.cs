using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TACTSharp.GUI.Models.Controls;
using TACTSharp.GUI.ViewModels.Controls.Base;

namespace TACTSharp.GUI.ViewModels.Controls;

public partial class HexControlViewModel : ViewModelBase, IFileVisualizationViewModel
{
    private const int SectionSize = 0x10;
    
    [ObservableProperty] private ObservableCollection<HexSection> _sections = [];
    public Task LoadAsync(byte[] fileBytes)
    {
        Sections.Clear();
        
        for (long ofs = 0; ofs < fileBytes.Length; ofs += SectionSize)
        {
            
            var bytesToRead = ofs + SectionSize > fileBytes.Length ? fileBytes.Length - ofs : SectionSize;
            
            var memory = new ReadOnlyMemory<byte>(fileBytes, (int)ofs, (int)bytesToRead);
            var section = new HexSection(ofs, memory);
            Sections.Add(section);
        }

        return Task.CompletedTask;
    }
}

