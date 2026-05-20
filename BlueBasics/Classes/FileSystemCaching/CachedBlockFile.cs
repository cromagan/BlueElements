// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using System.Text.Json.Nodes;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes.FileSystemCaching;

[FileSuffix(".blk")]
public sealed class CachedBlockFile : CachedFile {

    #region Fields

    private const double EditTimeInMinutes = 10;
    private const double SaveTimeInMinutes = 9;
    private static readonly object _forLock = new();

    #endregion

    #region Constructors

    private CachedBlockFile(string filename) : base(filename) { }

    #endregion

    #region Properties

    public string App { get; private set; } = string.Empty;
    public override bool ExtendedSave => false;
    public string Id { get; private set; } = string.Empty;

    public string MachineName { get; private set; } = string.Empty;

    public override bool MustZipped => false;

    public int ThreadId { get; private set; }

    public DateTime TimeUtc { get; private set; } = DateTime.MinValue;

    public string User { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public static void AcquireWriteAccessFor(string filename) {
        CachedBlockFile? bf;
        lock (_forLock) {
            bf = For(filename, true);
        }
        bf?.Write();
    }

    public static string BlockerMessage(string filename) => For(filename, false)?.BlockerMessage() ?? string.Empty;

    public static CachedBlockFile? For(string filename, bool createIfNotExists) {
        var blkName = GetBlockFilename(filename);

        if (!createIfNotExists) {
            if (!FileExists(blkName)) { return null; }
        }

        lock (_forLock) {
            var existing = CachedFileSystem.Get<CachedBlockFile>(blkName);
            if (existing is { IsDisposed: false }) {
                if (existing.IsStale()) { existing.Invalidate(); }
                return existing;
            }

            return new CachedBlockFile(blkName);
        }
    }

    public static string GetBlockFilename(string filename) =>
        string.IsNullOrEmpty(filename) ? string.Empty :
        filename.FilePath() + filename.FileNameWithoutSuffix() + ".blk";

    public static bool IsExpired(string filename) => For(filename, false)?.IsExpired() ?? true;

    public static bool IsMyLock(string filename) => For(filename, false)?.IsMyLock() ?? false;

    public string BlockerMessage() {
        EnsureLoaded();

        if (IsExpired()) { return string.Empty; }

        return BlockerMessageDirect();
    }

    public string BlockerMessageDirect() {
        var age = DateTime.UtcNow.Subtract(TimeUtc).TotalMinutes;
        if (age is < 0 or > EditTimeInMinutes) { return string.Empty; }

        var t = TimeUtc.AddMinutes(EditTimeInMinutes).ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        if (User != UserName) {
            return $"Aktueller Bearbeiter: {User} noch bis {t}";
        }

        if (App != Develop.AppExe()) {
            return $"Anderes Programm bearbeitet: {App.FileNameWithoutSuffix()} noch bis {t}";
        }

        if (MachineName != Environment.MachineName) {
            return $"Anderer Computer bearbeitet: {MachineName} - {User} noch bis {t}";
        }

        if (Id != MyId) {
            return $"Ein anderer Prozess auf diesem PC bearbeitet noch bis {t}.";
        }

        return string.Empty;
    }

    public override void Invalidate() {
        base.Invalidate();
        User = string.Empty;
        TimeUtc = DateTime.MinValue;
        MachineName = string.Empty;
        App = string.Empty;
        Id = string.Empty;
        ThreadId = 0;
    }

    public bool IsExpired() {
        if (IsDisposed) { return true; }
        if (TimeUtc == DateTime.MinValue) { return true; }

        var age = DateTime.UtcNow.Subtract(TimeUtc).TotalMinutes;
        return age is < 0 or > SaveTimeInMinutes;
    }

    public bool IsMyLock() {
        try {
            if (IsDisposed) { return false; }
            EnsureLoaded();
            if (IsExpired()) { return false; }
            return BlockerMessageDirect().Length == 0;
        } catch {
            return false;
        }
    }

    public override string ReadableText() => $"BlockFile '{Filename}'";

    public void Write() {
        User = UserName;
        TimeUtc = DateTime.UtcNow;
        ThreadId = Environment.CurrentManagedThreadId;
        MachineName = Environment.MachineName;
        App = Develop.AppExe();
        Id = MyId;

        var json = new JsonObject {
            ["user"] = User,
            ["timeUtc"] = TimeUtc,
            ["threadId"] = ThreadId,
            ["machineName"] = MachineName,
            ["app"] = App,
            ["id"] = Id
        }.ToJsonString();

        Content = Encoding.UTF8.GetBytes(json);
        _ = Save().GetAwaiter().GetResult();
    }

    protected override void OnLoaded() {
        User = string.Empty;
        TimeUtc = DateTime.MinValue;
        MachineName = string.Empty;
        App = Develop.AppExe();
        Id = MyId;
        ThreadId = 0;

        var content = Content;
        if (content.Length == 0) { return; }

        try {
            var json = Encoding.UTF8.GetString(content);
            if (JsonNode.Parse(json) is not JsonObject data) { return; }
            User = data["user"]?.GetValue<string>() ?? string.Empty;
            TimeUtc = data["timeUtc"]?.GetValue<DateTime>() ?? DateTime.MinValue;
            MachineName = data["machineName"]?.GetValue<string>() ?? string.Empty;
            App = data["app"]?.GetValue<string>() ?? string.Empty;
            Id = data["id"]?.GetValue<string>() ?? string.Empty;
            ThreadId = data["threadId"]?.GetValue<int>() ?? 0;
        } catch {
            TimeUtc = DateTime.MinValue;
        }
    }

    #endregion
}