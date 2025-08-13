using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using TACTSharp.GUI.Messages.ActionMessages;
using TACTSharp.GUI.ViewModels.Controls;

namespace TACTSharp.GUI.Controls;

public partial class HexControl : UserControl
{ 
    public HexControl() 
    {
        InitializeComponent();
        DataContext = new HexControlViewModel();
        
        WeakReferenceMessenger.Default.Register<HexControl, FileVisualizationMessage<HexControlViewModel>>(this, async void (window, message) =>
        {
            if (window.DataContext is not HexControlViewModel viewModel) return;
            await viewModel.LoadAsync(message.Payload);
            
            message.Reply(viewModel);
        });
    }
}