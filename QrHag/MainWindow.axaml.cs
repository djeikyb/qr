using Avalonia.Controls;
using QrHag.Vm;

namespace QrHag;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.DataContextChanged += (sender, args) =>
        {
            var mw = (MainWindow)sender!;
            var vm = (ViewModel)mw.DataContext!;

            vm.WindowPosition.OnNext(mw.Position);

            mw.PositionChanged += (o, eventArgs) =>
            {
                vm.WindowPosition.OnNext(eventArgs.Point);
            };

            mw.SizeChanged += (o, eventArgs) =>
            {
                vm.WindowSize.OnNext(eventArgs.NewSize);
            };

            // Observable.FromEvent<EventHandler, PixelPointEventArgs>(
            //     h => (_, e) => h(e),
            //     e => mw.PositionChanged += e,
            //     e => mw.PositionChanged -= e);
        };
    }
}
