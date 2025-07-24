using Microsoft.Extensions.Logging;

namespace QrHag.Logging;

// This is our own global logger manager
internal static class Log
{
    private static ILogger _globalLogger = default!;
    private static ILoggerFactory _loggerFactory = default!;

    public static void SetLoggerFactory(ILoggerFactory loggerFactory, string categoryName)
    {
        _loggerFactory = loggerFactory;
        _globalLogger = loggerFactory.CreateLogger(categoryName);
    }

    public static ILogger Logger => _globalLogger;

    // standard LoggerFactory caches logger per category so no need to cache in this manager
    public static ILogger<T> GetLogger<T>() where T : class => _loggerFactory.CreateLogger<T>();
    public static ILogger GetLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);
}
