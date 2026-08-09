// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes;

/// <summary>
/// Verwaltet .blk-Sperrdateien für Multi-User-Zugriff.
/// Jede .blk-Datei enthält ein JSON mit User, Machine, App, Prozess-ID und Zeitstempel.
/// Hat sich jemand innerhalb von <see cref="EditTimeInMinutes"/> Minuten eingetragen,
/// gilt die zugehörige Datei als gesperrt für andere.
/// Lesezugriffe erfolgen über den transparenten Read-Cache von <see cref="IO"/>.
/// </summary>
public sealed class BlockFile {

    #region Fields

    /// <summary>Zeitfenster, in dem die Sperre nachrichtlich als aktiv gemeldet wird.</summary>
    private const double EditTimeInMinutes = 10;

    /// <summary>Ab diesem Alter gilt die Sperre als abgelaufen und kann überschrieben werden.</summary>
    private const double SaveTimeInMinutes = 9;

    private readonly string _myId = string.Empty;

    #endregion

    #region Constructors

    private BlockFile(string id) { _myId = id; }

    #endregion

    #region Properties

    public string App { get; private set; } = string.Empty;

    public string Id { get; private set; } = string.Empty;

    public string MachineName { get; private set; } = string.Empty;

    public DateTime TimeUtc { get; private set; } = DateTime.MinValue;

    public string User { get; private set; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>Schreibt die .blk-Sperre für die angegebene Datei mit den aktuellen Prozessdaten.</summary>
    /// <param name="myId">Prozess-spezifische ID des Aufrufers (<see cref="BlockableFile.MyId"/>).</param>
    public static void AcquireWriteAccessFor(string filename, string myId) {
        var blkName = BlockFilename(filename);
        if (Read(blkName, myId) is { } bf && !bf.IsExpired() && bf.MessageForOther().Length > 0) { return; }
        WriteBlockFile(blkName, myId);
    }

    /// <summary>Liefert eine Blocker-Meldung falls ein anderer die Datei sperrt, sonst <see cref="string.Empty"/>.</summary>
    /// <param name="filename"></param>
    /// <param name="myId">Prozess-spezifische ID des Aufrufers (<see cref="BlockableFile.MyId"/>).</param>
    public static string BlockerMessage(string filename, string myId) => Read(BlockFilename(filename), myId)?.MessageForOther() ?? string.Empty;

    /// <summary>Prüft, ob der aktuelle Prozess die aktive Sperre für die angegebene Datei hält.</summary>
    /// <param name="myId">Prozess-spezifische ID des Aufrufers (<see cref="BlockableFile.MyId"/>).</param>
    public static bool IsMyLockFor(string filename, string myId) => Read(BlockFilename(filename), myId) is { } bf && !bf.IsExpired() && bf.MessageForOther().Length == 0;

    /// <summary>
    /// Entfernt die Sperre, wenn sie dem aktuellen Prozess gehört.
    /// <paramref name="onReleasing"/> wird nach erfolgreichem Löschen aufgerufen.
    /// </summary>
    /// <param name="myId">Prozess-spezifische ID des Aufrufers (<see cref="BlockableFile.MyId"/>).</param>
    public static void RevokeFor(string filename, string myId, Action? onReleasing = null) {
        var blkName = BlockFilename(filename);
        if (Read(blkName, myId) is not { } bf || bf.IsExpired() || bf.MessageForOther().Length > 0) { return; }
        if (!DeleteFile(blkName, false)) { return; }

        onReleasing?.Invoke();
    }

    /// <summary>True, wenn die Sperre abgelaufen (<see cref="SaveTimeInMinutes"/>) oder nie gesetzt wurde.</summary>
    public bool IsExpired() {
        if (TimeUtc == DateTime.MinValue) { return true; }

        var age = DateTime.UtcNow.Subtract(TimeUtc).TotalMinutes;
        return age is < 0 or > SaveTimeInMinutes;
    }

    public override string ToString() => $"BlockFile: User={User}, Machine={MachineName}, App={App.FileNameWithoutSuffix()}";

    /// <summary>"daten.csv" → "daten.blk". Leer bei leerer Eingabe.</summary>
    private static string BlockFilename(string filename) =>
        !string.IsNullOrWhiteSpace(filename)
            ? filename.FilePath() + filename.FileNameWithoutSuffix() + ".blk"
            : string.Empty;

    private static BlockFile? Read(string blkName, string myId) {
        var result = ReadAllBytes(blkName, 5);
        if (result.IsFailed || result.Value is not byte[] content || content.Length == 0) { return null; }

        try {
            if (JsonNode.Parse(Encoding.UTF8.GetString(content)) is JsonObject data) {
                return new BlockFile(myId) {
                    User = data["user"]?.GetValue<string>() ?? string.Empty,
                    TimeUtc = data["timeutc"]?.GetValue<DateTime>() ?? DateTime.MinValue,
                    MachineName = data["machinename"]?.GetValue<string>() ?? string.Empty,
                    App = data["app"]?.GetValue<string>() ?? string.Empty,
                    Id = data["id"]?.GetValue<string>() ?? string.Empty,
                };
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Lesen der Blockdatei: " + blkName, ex);
            DeleteFile(blkName, false);
        }
        return null;
    }

    private static void WriteBlockFile(string blkName, string myId) {
        JsonObject json = new();
        json.Set("user", UserName);
        json.Set("timeutc", DateTime.UtcNow);
        json.Set("machinename", Environment.MachineName);
        json.Set("app", Develop.AppExe());
        json.Set("id", myId);

        WriteAllBytes(blkName, Encoding.UTF8.GetBytes(json.ToJsonString()));
    }

    /// <summary>
    /// Meldung für andere, falls sie nicht selbst die Sperre halten.
    /// Berücksichtigt den Ablauf; leer, wenn die Sperre abgelaufen ist.
    /// </summary>
    private string MessageForOther() => IsExpired() ? string.Empty : MessageForOtherCore();

    /// <summary>
    /// Meldung für andere ohne Ablauf-Prüfung.
    /// Leer, wenn der aktuelle Prozess selbst die Sperre hält.
    /// </summary>
    private string MessageForOtherCore() {
        var age = DateTime.UtcNow.Subtract(TimeUtc).TotalMinutes;
        if (age is < 0 or > EditTimeInMinutes) { return string.Empty; }

        var deadline = TimeUtc.AddMinutes(EditTimeInMinutes).ToLocalTime()
            .ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        if (User != UserName) { return $"Aktueller Bearbeiter: {User} noch bis {deadline}"; }
        if (!string.Equals(App, Develop.AppExe(), StringComparison.OrdinalIgnoreCase)) { return $"Anderes Programm bearbeitet: {App.FileNameWithoutSuffix()} noch bis {deadline}"; }
        if (MachineName != Environment.MachineName) { return $"Anderer Computer bearbeitet: {MachineName} - {User} noch bis {deadline}"; }
        if (Id != _myId) { return $"Ein anderer Prozess auf diesem PC bearbeitet noch bis {deadline}."; }
        return string.Empty;
    }

    #endregion
}