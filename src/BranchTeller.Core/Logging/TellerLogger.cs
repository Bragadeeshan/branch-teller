namespace BranchTeller.Core.Logging;

public class TellerLogger
{
    private readonly string _logPath = "teller.log";

    public void Log(string message)
    {
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        Console.WriteLine(entry);
        File.AppendAllText(_logPath, entry + Environment.NewLine);
    }
}