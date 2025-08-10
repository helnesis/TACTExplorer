using System;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using TACTSharp.GUI.Enums;
using TACTSharp.GUI.Messages;
using TACTSharp.GUI.Services.Listfile;
using TACTSharp.GUI.Services.Tact;
using TACTSharp.GUI.ViewModels;
using TACTSharp.GUI.ViewModels.Configuration;
using TACTSharp.GUI.Views.Configuration;

namespace TACTSharp.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow(ITactService storageService, IConfiguration configuration, IListfileService listfile)
    {
        InitializeComponent();
        
        WeakReferenceMessenger.Default.Register<MainWindow, CloseStorageMessage>(this, (window, message) =>
        {
            if (window.DataContext is not MainWindowViewModel vm || storageService.Entry is null) return;
            
            vm.Root = storageService.Entry;
            vm.CreateHierarchicalTreeDataGridSource(vm.Root);
        });
        
        
        // Register the message handler for opening storage configuration window.
        WeakReferenceMessenger.Default.Register<MainWindow, OpenStorageMessage>(this, (window, message) =>
        {
            var viewModel = new OpenStorageViewModel(storageService, configuration, listfile)
            {
                StorageType = message.StorageType,
                WindowTitle = message.StorageType switch
                {
                    StorageType.Online => "Open online storage",
                    StorageType.Local => "Open local storage",
                    _ => throw new NotSupportedException($"Unsupported storage type ({message.StorageType})")
                }
            };
            
            var dialog = new OpenStorageView()
            {
                DataContext = viewModel
            };

            if (message.StorageType == StorageType.Online)
            {
                if (viewModel.LoadAvailableBuildsCommand.CanExecute(null))
                {
                    viewModel.LoadAvailableBuildsCommand.Execute(null);
                }
            }
            
            message.Reply(dialog.ShowDialog<OpenStorageViewModel>(window));
        });
        
    }
}