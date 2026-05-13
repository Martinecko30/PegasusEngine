using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace PegasusEngine.Debug;

public static class Log
{
    private static ILogger? _engineLogger;
    private static ILogger? _editorLogger;

    public static ILogger EngineLogger => _engineLogger ?? throw new InvalidOperationException("Logger not initialized. Call Log.Init() first.");
    public static ILogger EditorLogger => _editorLogger ?? throw new InvalidOperationException("Logger not initialized. Call Log.Init() first.");

    public static event Action<LogEventLevel, string> OnLogEmitted;
    
    public static void Init()
    {
        // Pattern: "%^[%T] %n: %v%$"
        const string outputTemplate = "[{Timestamp:HH:mm:ss}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

        var eventSink = new PegasusEventSink();
        
        _engineLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.Sink(eventSink)
            .CreateLogger()
            .ForContext("SourceContext", "Core");

        _editorLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.Sink(eventSink)
            .CreateLogger()
            .ForContext("SourceContext", "App");
    }

    // Engine Logging Helpers (Replacing Macros)
    public static void EngineTrace(string message, params object[] args) => EngineLogger.Verbose(message, args);
    public static void EngineInfo(string message, params object[] args) => EngineLogger.Information(message, args);
    public static void EngineWarn(string message, params object[] args) => EngineLogger.Warning(message, args);
    public static void EngineError(string message, params object[] args) => EngineLogger.Error(message, args);
    public static void EngineCritical(string message, params object[] args) => EngineLogger.Fatal(message, args);
    public static void EngineDebug(string message, params object[] args) => EngineLogger.Debug(message, args);
    
    // Editor Logging Helpers (Replacing Macros)
    public static void EditorTrace(string message, params object[] args) => EditorLogger.Verbose(message, args);
    public static void EditorInfo(string message, params object[] args) => EditorLogger.Information(message, args);
    public static void EditorWarn(string message, params object[] args) => EditorLogger.Warning(message, args);
    public static void EditorError(string message, params object[] args) => EditorLogger.Error(message, args);
    public static void EditorCritical(string message, params object[] args) => EditorLogger.Fatal(message, args);
    public static void EditorDebug(string message, params object[] args) => EditorLogger.Debug(message, args);
    
    /// <summary>
    /// A custom Serilog Sink that intercepts formatted log messages and fires our C# event.
    /// Nested inside Log so it can access the OnLogEmitted event safely.
    /// </summary>
    private class PegasusEventSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            string renderedMessage = logEvent.RenderMessage();

            if (logEvent.Exception != null)
            {
                renderedMessage += $"\n{logEvent.Exception.Message}";
            }

            OnLogEmitted?.Invoke(logEvent.Level, renderedMessage);
        }
    }
}