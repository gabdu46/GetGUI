namespace GetGUI.Services;

public static class AppServices
{
    public static SettingsService Settings { get; } = new();

    public static QueueService Queue { get; } = new();

    public static AppLogService Log { get; } = new();

    public static WingetService Winget { get; } = new(new ProcessRunner(Log));

    public static PopularPackageCacheService PopularPackageCache { get; } = new();

    public static void Initialize()
    {
        Settings.Load();
        Queue.Load();
        Log.Load();
        Log.AppendInfo("GetGUI demarre.");
    }
}
