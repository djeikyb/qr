using System;
using System.Linq;
using Avalonia.Input.Platform;
using ObservableCollections;
using R3;
using Serilog;
using Serilog.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;

namespace QrPaste.Vm;

public class ViewModel
{
    public ViewModel(IClipboard clipboard)
    {
        var logger = Log.ForContext<ViewModel>();

        var formats = new ObservableList<string>();
        Formats = formats.ToNotifyCollectionChangedSlim();

        DecodedText = new BindableReactiveProperty<string?>();
        DecodedBase64 = new BindableReactiveProperty<string?>();
        DecodedHex = new BindableReactiveProperty<string?>();
        DecodedRaw = new BindableReactiveProperty<byte[]?>();

        Observable.Interval(TimeSpan.FromMilliseconds(500))
            .SubscribeAwait(async (_, ct) =>
            {
                var got = await clipboard.GetFormatsAsync();

                if (!formats.SequenceEqual(got))
                {
                    formats.Clear();
                    formats.AddRange(got);
                }

                if (formats.Contains("public.tiff"))
                {
                    var data = await clipboard.GetDataAsync("public.tiff");
                    if (data is null)
                    {
                        NothingDecoded();
                    }

                    var bytes = data as byte[]
                                ?? throw new Exception($"Expected byte array, but got: {data?.GetType()}");
                    using (var image = Image.Load<Rgba32>(bytes))
                    {
                        var reader = new BarcodeReader<Rgba32>();
                        var result = reader.Decode(image);
                        if (result is null)
                        {
                            return;
                        }

                        DecodedBase64.Value = Convert.ToBase64String(result.RawBytes);
                        DecodedHex.Value = Convert.ToHexString(result.RawBytes);
                        DecodedText.Value = result.Text;
                        DecodedRaw.Value = result.RawBytes;
                    }
                }
                else
                {
                    NothingDecoded();
                }
            }, AwaitOperation.Switch);

        View = App.LogsSink.Logs.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        TintOpacity = new(1m);
        MaterialOpacity = new(1m);
    }

    private void NothingDecoded()
    {
        DecodedText.Value = null;
        DecodedBase64.Value = null;
        DecodedHex.Value = null;
        DecodedRaw.Value = null;
    }

    public BindableReactiveProperty<decimal> TintOpacity { get; }
    public BindableReactiveProperty<decimal> MaterialOpacity { get; }

    public BindableReactiveProperty<byte[]?> DecodedRaw { get; }
    public BindableReactiveProperty<string?> DecodedText { get; }
    public BindableReactiveProperty<string?> DecodedBase64 { get; }
    public BindableReactiveProperty<string?> DecodedHex { get; }

    public INotifyCollectionChangedSynchronizedViewList<string> Formats { get; }

    public ReactiveCommand<Unit> Click { get; }

    public INotifyCollectionChangedSynchronizedViewList<LogEvent> View { get; }
}
