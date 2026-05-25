using System.Text;

namespace GetGUI.Services;

public sealed class AppLogService
{
    private const int MaxInMemoryCharacters = 200_000;
    private readonly object _syncRoot = new();
    private readonly StringBuilder _text = new();
    private readonly string _logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GetGUI",
        "logs",
        "getgui.log");

    public string LogPath => _logPath;

    public string Text
    {
        get
        {
            lock (_syncRoot)
            {
                return _text.ToString();
            }
        }
    }

    public void Load()
    {
        lock (_syncRoot)
        {
            _text.Clear();
            if (!File.Exists(_logPath))
            {
                return;
            }

            var content = File.ReadAllText(_logPath);
            if (content.Length > MaxInMemoryCharacters)
            {
                content = content[^MaxInMemoryCharacters..];
            }

            _text.Append(content);
        }
    }

    public void AppendInfo(string message)
    {
        Append("INFO", message);
    }

    public void AppendCommand(string fileName, IEnumerable<string> arguments)
    {
        Append("CMD", RenderCommand(fileName, arguments));
    }

    public void AppendOutput(bool isError, string text)
    {
        Append(isError ? "ERR" : "OUT", text);
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _text.Clear();
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            File.WriteAllText(_logPath, string.Empty);
        }
    }

    private void Append(string level, string message)
    {
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

        lock (_syncRoot)
        {
            _text.Append(line);
            if (_text.Length > MaxInMemoryCharacters)
            {
                _text.Remove(0, _text.Length - MaxInMemoryCharacters);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            File.AppendAllText(_logPath, line);
        }
    }

    private static string RenderCommand(string fileName, IEnumerable<string> arguments)
    {
        return fileName + " " + string.Join(" ", arguments.Select(QuoteIfNeeded));
    }

    private static string QuoteIfNeeded(string argument)
    {
        return argument.Any(char.IsWhiteSpace) ? $"\"{argument.Replace("\"", "\\\"")}\"" : argument;
    }
}
