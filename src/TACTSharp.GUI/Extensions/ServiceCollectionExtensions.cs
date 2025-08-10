using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using TACTSharp.GUI.Services;
using TACTSharp.GUI.Services.Listfile;
using TACTSharp.GUI.Services.Tact;
using TACTSharp.GUI.ViewModels;

namespace TACTSharp.GUI.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddSingleton<IDialogService, DialogService>();
        
        collection.AddSingleton<IListfileService, ListfileService>();
        collection.AddSingleton<ITactService, TactService>();
        collection.AddSingleton<MainWindowViewModel>();
    }
}