using System.Diagnostics;
using TShockAPI;

namespace ProgressTogether;

public class Log
{
    public static string LogPath
    {
        get { return Path.Combine(TShock.SavePath, "progress-together.log"); }
    }
    public static string Name => "Progress Together";

    public static void LogToConsole(string message)
    {
        TShock.Log.ConsoleInfo($"[{Name}]: {message}");
    }
    
    public static void LogToFile(string message)
    {
        var now = DateTime.Now;
        var formattedMessage = $"[{Name}][{now}]: {message}";
        File.AppendAllText(LogPath, formattedMessage);
        TShock.Log.Info(formattedMessage);
    }
}