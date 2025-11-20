namespace ThingsGateway.Gateway.Application;

public static class FileConst
{
    public const string FilePathKey = "FilePath";
    public static string UpgradePath = Path.Combine(AppContext.BaseDirectory, "Upgrade.zip");
    public static string UpgradeBackupPath = Path.Combine(AppContext.BaseDirectory, "..", "Backup.zip");
    public static string UpgradeBackupDirPath = Path.Combine(AppContext.BaseDirectory, "..", "Backup");
    public const string UpdateZipFileServerDir = "UpdateZipFile";
}
