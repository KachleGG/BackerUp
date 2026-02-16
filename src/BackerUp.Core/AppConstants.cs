namespace BackerUp.Core;

public static class AppConstants
{
    public static readonly string AppDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BackerUp");
    public static readonly string ConfigFilePath = Path.Combine(AppDataFolderPath, "BackerUp.conf");
    public static readonly string MetadataFolderPath = Path.Combine(AppDataFolderPath, "JobsMetadata");
}
