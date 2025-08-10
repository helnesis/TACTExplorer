using Avalonia;
using System;
using Avalonia.Media.Fonts;

namespace TACTSharp.GUI;

internal sealed class JetBrainsMonoFontCollection() : EmbeddedFontCollection(new Uri("fonts:JetBrainsMono", UriKind.Absolute),
    new Uri("avares://TACTSharp.GUI/Assets/Fonts/JetBrainsMono/", UriKind.Absolute));

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureFonts(fontManager =>
            {
                fontManager.AddFontCollection(new JetBrainsMonoFontCollection());
            })
            .LogToTrace();
}