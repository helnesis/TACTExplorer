using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using TACTSharp.GUI.Messages;

namespace TACTSharp.GUI.Views.Configuration;

public partial class OpenStorageView : Window
{
    public OpenStorageView()
    {
        InitializeComponent();
  
        WeakReferenceMessenger.Default.Register<OpenStorageView, CloseStorageMessage>(this, static async (window, message) =>
        {
            window.Close();
        });

    }
}