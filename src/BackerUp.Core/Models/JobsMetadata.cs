using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackerUp.Core.Models;

public class JobsMetadata
{
    public int JobId { get; set; }

    public int NextPackageIndex { get; set; } = 0;

    public DateTime? LastPackageTimestampUtc { get; set; }

    public DateTime? LastSnapshotTimestampUtc { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BackupMethod Method { get; set; } = BackupMethod.Full;

    public List<PackageEntry> Packages { get; set; } = new();

    public static void EnsureMetadataFolder() {
        if (!Directory.Exists(AppConstants.MetadataFolderPath)) {
            Directory.CreateDirectory(AppConstants.MetadataFolderPath);
        }
    }

    public void SaveToAppData() {
        EnsureMetadataFolder();
        string file = Path.Combine(AppConstants.MetadataFolderPath, $"job_{JobId}.json");
        try {
            JsonSerializerOptions opts = new() { WriteIndented = true };
            opts.Converters.Add(new JsonStringEnumConverter());
            File.WriteAllText(file, JsonSerializer.Serialize(this, opts));
        } catch {
        }
    }

    public static JobsMetadata? LoadFromAppData(BackupJob job) {
        EnsureMetadataFolder();
        string file = Path.Combine(AppConstants.MetadataFolderPath, $"job_{job.Id}.json");
        try {
            if (!File.Exists(file)) {
                return null;
            }

            JsonSerializerOptions opts = new();
            opts.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Deserialize<JobsMetadata>(File.ReadAllText(file), opts);
        } catch {
            return null;
        }
    }

    public static JobsMetadata LoadOrCreateForJob(BackupJob job) {
        JobsMetadata? info = LoadFromAppData(job);
        if (info != null) {
            return info;
        }

        JobsMetadata created = new() { JobId = job.Id ,NextPackageIndex = 0, Method = job.Method };
        created.SaveToAppData();
        return created;
    }

    public void AddPackage(string packageName, DateTime createdUtc) {
        Packages.Add(new PackageEntry { Name = packageName, CreatedUtc = createdUtc, SnapshotCount = 0 });
        LastPackageTimestampUtc = createdUtc;
    }

    public void IncrementSnapshotCount(string packageName) {
        PackageEntry? p = Packages.LastOrDefault(pn => pn.Name == packageName) ?? Packages.LastOrDefault();
        if (p != null) {
            p.SnapshotCount++;
            LastSnapshotTimestampUtc = DateTime.UtcNow;
        }
    }

    public PackageEntry? GetCurrentPackage() {
        if (Packages == null || Packages.Count == 0) {
            return null;
        }

        return Packages[Packages.Count - 1];
    }

    public List<string> PurgeOldPackages(int keep) {
        if (keep <= 0) {
            return new List<string>();
        }

        if (Packages.Count <= keep) {
            return new List<string>();
        }

        int toRemove = Packages.Count - keep;
        List<string> removed = Packages.Take(toRemove).Select(p => p.Name).ToList();
        Packages = Packages.Skip(toRemove).ToList();
        return removed;
    }
}
