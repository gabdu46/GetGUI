using System.Diagnostics;
using System.Text;

namespace GetGUI.Services;

public sealed record ProcessOutput(bool IsError, string Text);

public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool IsSuccess => ExitCode == 0;

    public string CombinedOutput => string.IsNullOrWhiteSpace(StandardError)
        ? StandardOutput
        : StandardOutput + Environment.NewLine + StandardError;
}

public sealed class ProcessRunner
{
    private readonly AppLogService? _log;

    public ProcessRunner(AppLogService? log = null)
    {
        _log = log;
    }

    public async Task<ProcessResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken = default,
        IProgress<ProcessOutput>? progress = null,
        bool logOutput = true)
    {
        var argumentList = arguments.ToList();
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var argument in argumentList)
        {
            startInfo.ArgumentList.Add(argument);
        }

        _log?.AppendCommand(fileName, argumentList);

        var output = new StringBuilder();
        var error = new StringBuilder();
        var outputLock = new object();

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                return;
            }

            lock (outputLock)
            {
                output.AppendLine(args.Data);
            }

            if (logOutput)
            {
                _log?.AppendOutput(false, args.Data);
            }

            progress?.Report(new ProcessOutput(false, args.Data));
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                return;
            }

            lock (outputLock)
            {
                error.AppendLine(args.Data);
            }

            _log?.AppendOutput(true, args.Data);
            progress?.Report(new ProcessOutput(true, args.Data));
        };

        if (!process.Start())
        {
            _log?.AppendOutput(true, $"Impossible de demarrer {fileName}.");
            throw new InvalidOperationException($"Impossible de demarrer {fileName}.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            _log?.AppendInfo($"{fileName} annule.");
            throw;
        }

        lock (outputLock)
        {
            _log?.AppendInfo($"{fileName} termine avec le code {process.ExitCode}.");
            return new ProcessResult(process.ExitCode, output.ToString(), error.ToString());
        }
    }
}
