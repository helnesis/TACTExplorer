using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TACTSharp.GUI.Extensions;
using TACTSharp.GUI.Services.Listfile;
using TACTSharp.GUI.Services.Tact;
using TACTSharp.GUI.Utilities;
using TACTSharp.GUI.ViewModels;
using TACTSharp.GUI.Views;

namespace TACTSharp.GUI;

public partial class App : Application
{
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Shared.Initialize();
        
        BindingPlugins.DataValidators.RemoveAt(0);
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Shared.SettingsFileName)
            .Build();
        
        var collection = new ServiceCollection();
        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddServices();
        
        var services = collection.BuildServiceProvider();
        var vm = services.GetRequiredService<MainWindowViewModel>();
        var tactService = services.GetRequiredService<ITactService>();
        var listfileService = services.GetRequiredService<IListfileService>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow(tactService, configuration, listfileService)
            {
                DataContext = vm,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}