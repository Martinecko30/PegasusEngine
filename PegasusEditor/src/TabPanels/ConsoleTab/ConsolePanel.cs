using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Core.Events;
using PegasusEngine.Debug;
using Serilog.Events;

namespace PegasusEditor.TabPanels;

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
    
    public override void Start()
    {
        this.Title = FontAwesomeIcons.Terminal + " Console";

        Log.OnLogEmitted += PushMessage;
    }


    private void PushMessage(LogEventLevel level, string message)
    {
        logQueue.Enqueue(new LogMessage(level, message));
    }

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

    public override void OnEvent(IEvent e)
    {
    }
    
    public override void Dispose()
    {
        Log.OnLogEmitted -= PushMessage;
    }
}
