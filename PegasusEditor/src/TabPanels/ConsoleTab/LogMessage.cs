using Serilog.Events;

namespace PegasusEditor.TabPanels;

public struct LogMessage
{
    public LogEventLevel Level;
    public string Message;
    public string Time;

    public LogMessage(LogEventLevel level, string message)
    {
        Level = level;
        Message = message;
        Time = DateTime.Now.ToString("HH:mm:ss");
    }
}