using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Core.Events;
using PegasusEngine.Debug;
using Serilog.Events;

namespace PegasusEditor.TabPanels;

/// <summary>
/// Represents an editor console panel that displays log messages emitted by the engine.
/// </summary>
public class ConsolePanel : TabPanel
{
    private readonly List<LogMessage> logHistory = new();
    private readonly ConcurrentQueue<LogMessage> logQueue = new();

    private bool autoScroll = true;
    private bool scrollToBottom = false;
    
    private bool showInfo = true;
    private bool showWarnings = true;
    private bool showErrors = true;
    private bool showDebugs = false;

    private const int MaxLogCount = 3000;
    
    /// <summary>
    /// Initializes the console panel and subscribes to log events.
    /// </summary>
    public override void Start()
    {
        this.Title = FontAwesomeIcons.Terminal + " Console";

        Log.OnLogEmitted += PushMessage;
    }

    /// <summary>
    /// Queues a log message to be displayed by the console panel.
    /// </summary>
    /// <param name="level">The severity level of the log message.</param>
    /// <param name="message">The message text to display.</param>
    private void PushMessage(LogEventLevel level, string message)
    {
        logQueue.Enqueue(new LogMessage(level, message));
    }

    /// <summary>
    /// Renders the console panel UI, including filtering controls and visible log messages.
    /// </summary>
    public override void Render()
    {
        ImGui.Begin(Title);

        if (ImGui.Button(FontAwesomeIcons.Trash + " Clear"))
        {
            logHistory.Clear();
        }
        
        ImGui.SameLine();
        ImGui.Checkbox("Auto-scroll", ref autoScroll);
        
        ImGui.SameLine(ImGui.GetWindowWidth() - 250);
        ImGui.Checkbox("Info", ref showInfo);
        ImGui.SameLine();
        ImGui.Checkbox("Warn", ref showWarnings);
        ImGui.SameLine();
        ImGui.Checkbox("Error", ref showErrors);
        ImGui.SameLine();
        ImGui.Checkbox("Debug", ref showDebugs);
        
        ImGui.Separator();
        
        ImGui.BeginChild("ScrollingRegion", new Vector2(0, 0));

        // Show messages
        int logIndex = 0;
        foreach (var msg in logHistory)
        {
            if (!ShouldShow(msg.Level))
                continue;
            
            Vector4 color = GetColorForLevel(msg.Level);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            
            ImGui.PushID(logIndex++);
            string formattedMsg = $"[{msg.Time}] [{msg.Level}] {msg.Message}";
            ImGui.PushTextWrapPos(0.0f);
            ImGui.TextUnformatted(formattedMsg);

            if (ImGui.IsItemClicked())
                ImGui.SetClipboardText(formattedMsg);
            
            ImGui.PopStyleColor();

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Click to copy to clipboard");
            
            ImGui.PopTextWrapPos();
            ImGui.PopID();
        }

        if (scrollToBottom)
        {
            ImGui.SetScrollHereY(1.0f);
            scrollToBottom = false;
        }
        else if (autoScroll && ImGui.GetScrollY() < ImGui.GetScrollMaxY())
        {
            autoScroll = false;
        }
        
        ImGui.EndChild();
        ImGui.End();
    }

    /// <summary>
    /// Determines whether a log message with the specified level should be displayed.
    /// </summary>
    /// <param name="level">The log level to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if messages with the specified level should be shown;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private bool ShouldShow(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Information => showInfo,
            LogEventLevel.Warning => showWarnings,
            LogEventLevel.Error => showErrors,
            LogEventLevel.Debug => showDebugs,
            _ => true
        };
    }
    
    /// <summary>
    /// Gets the display color used for a specific log level.
    /// </summary>
    /// <param name="level">The log level to get a color for.</param>
    /// <returns>The color used when rendering log messages of the specified level.</returns>
    private Vector4 GetColorForLevel(LogEventLevel level)
    {
        // TODO: Make this a theme setting
        return level switch
        {
            LogEventLevel.Information => new Vector4(1.0f, 1.0f, 1.0f, 1.0f),   // White
            LogEventLevel.Warning => new Vector4(1.0f, 0.8f, 0.2f, 1.0f),       // Yellow
            LogEventLevel.Error => new Vector4(1.0f, 0.3f, 0.3f, 1.0f),         // Red
            LogEventLevel.Fatal => new Vector4(1.0f, 0.0f, 1.0f, 1.0f),         // Magenta
            LogEventLevel.Debug => new Vector4(0.24f, 1.0f, 0.29f, 1.0f),       // Uranium Green
            _ => new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
        };
    }

    /// <summary>
    /// Processes queued log messages and updates the console history.
    /// </summary>
    public override void Update()
    {
        bool addedNew = false;
        while (logQueue.TryDequeue(out var logMessage))
        {
            logHistory.Add(logMessage);
            addedNew = true;
        }

        if (addedNew)
        {
            if (logHistory.Count > MaxLogCount)
                logHistory.RemoveRange(0, logHistory.Count - MaxLogCount);
            
            if (autoScroll)
            {
                scrollToBottom = true;
            }
        }
    }

    /// <summary>
    /// Handles incoming editor or engine events.
    /// </summary>
    /// <param name="e">The event to handle.</param>
    public override void OnEvent(IEvent e)
    {
    }
    
    /// <summary>
    /// Releases resources used by the console panel and unsubscribes from log events.
    /// </summary>
    public override void Dispose()
    {
        Log.OnLogEmitted -= PushMessage;
    }
}
