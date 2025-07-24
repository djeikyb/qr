using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Microsoft.Extensions.Logging;
using QrHag.Logging;
using R3;
using SixLabors.ImageSharp.PixelFormats;
using ZLogger;
using ZXing.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using Size = Avalonia.Size;

namespace QrHag.Vm;

public class ViewModel : IDisposable
{
    private DisposableBag _disposable;

    private readonly ILogger<ViewModel> _logger = Log.GetLogger<ViewModel>();

    public ViewModel()
    {
        WindowPosition = new();
        WindowPositionDisplay = WindowPosition
            .Select(pp => $"x:{pp.X} y:{pp.Y}")
            .ToReadOnlyBindableReactiveProperty(string.Empty);

        WindowSize = new();
        WindowSizeDisplay = WindowSize
            .Select(z => $"{z.Width}x{z.Height}")
            .ToReadOnlyBindableReactiveProperty(string.Empty);

        var latestRect = WindowPosition.CombineLatest(WindowSize, (p, z) => $"{p.X},{p.Y},{z.Width},{z.Height}");

        ScreencaptureRect = latestRect.ToReadOnlyBindableReactiveProperty(string.Empty);

        var dir = Directory.CreateTempSubdirectory("merviche.qrpaste.");
        PathScreenshot = Path.Combine(dir.FullName, $"{Ulid.NewUlid()}.png");

        DecodedText = new();

        latestRect.Debounce(TimeSpan.FromMilliseconds(50))
            .SubscribeAwait(async (r, ct) =>
            {
                var p = Process.Start("screencapture", ["-x", "-R", r, PathScreenshot]);
                try
                {
                    await p.WaitForExitAsync(ct);
                }
                finally
                {
                    if (!p.HasExited)
                    {
                        p.Kill(entireProcessTree: true);
                        _logger.ZLogInformation($"Had to kill screencapture. Exit code: {p.ExitCode:@ExitCode}.");
                    }
                    else
                    {
                        _logger.ZLogInformation($"Screencapture exit code: {p.ExitCode:@ExitCode}.");
                    }
                }

                if (p.ExitCode != 0) return;

                var bytes = File.ReadAllBytes(PathScreenshot);
                using (var image = Image.Load<Rgba32>(bytes))
                {
                    var reader = new BarcodeReader<Rgba32>();
                    var result = reader.Decode(image);
                    if (result is null)
                    {
                        DecodedText.Value = null;
                        return;
                    }

                    DecodedText.Value = result.Text;
                }
            }, AwaitOperation.Drop);
    }

    public void Dispose()
    {
        _logger.LogInformation($"Disposing {GetType().Name}.");
        try
        {
            File.Delete(PathScreenshot);
            _logger.ZLogInformation($"Removed: {PathScreenshot:@Path}");
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Failed to remove: {PathScreenshot:@Path}");
        }

        _disposable.Dispose();
    }

    public string PathScreenshot { get; }
    public BindableReactiveProperty<PixelPoint> WindowPosition { get; }
    public BindableReactiveProperty<Size> WindowSize { get; }
    public IReadOnlyBindableReactiveProperty<string> WindowPositionDisplay { get; }
    public IReadOnlyBindableReactiveProperty<string> WindowSizeDisplay { get; }
    public IReadOnlyBindableReactiveProperty<string> ScreencaptureRect { get; }
    public BindableReactiveProperty<string?> DecodedText { get; }
}
