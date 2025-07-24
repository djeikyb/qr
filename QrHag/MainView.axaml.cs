using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace QrHag;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        // Eye.PointerPressed
        Eye.PointerPressed += OnEyeOnPointerPressed;
        Eye.PointerReleased += OnEyeOnPointerReleased;
    }

    private void OnEyeOnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        // ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.mov(args);
    }

    private void OnEyeOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.BeginMoveDrag(args);
    }

}
