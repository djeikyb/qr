using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QrHag.Vm;

namespace QrHag;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new ViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
            desktop.ShutdownRequested += (sender, args) =>
            {
                vm.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void AboutMenuItem_OnClick(object? sender, EventArgs e)
    {
    }
}
