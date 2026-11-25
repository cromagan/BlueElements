// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Reflection;

namespace BlueTable.Tests;

/// <summary>
/// Einheitlicher Zugriff auf Laden, erzwungenes Speichern und Speichern-mit-Warten für alle Tabellen-Typen.
/// </summary>
public static class TableAccess {

    #region Methods

    /// <summary>
    /// Lädt eine bestehende Tabelle inkl. aller Zeilen und prüft Freezes; bei Problemen wird aufgeräumt und die Liste gefüllt.
    /// </summary>
    public static Table? LoadExisting(string path, List<string> problems) {
        Table? tbl;

        try {
            tbl = Table.Get(path);
        } catch (Exception ex) {
            problems.Add("Laden fehlgeschlagen: " + ex.GetType().Name + " " + ex.Message);
            return null;
        }

        if (tbl is not { IsDisposed: false }) {
            problems.Add("Tabelle nicht geladen: " + path);
            return null;
        }

        if (tbl.IsFreezed) {
            problems.Add("Tabelle eingefroren: " + tbl.FreezedReason);
            tbl.Dispose();
            return null;
        }

        if (tbl is TableFileNew tfn && tfn.Saveable.IsFreezed) {
            problems.Add("Datei-Verwaltung eingefroren: " + tfn.Saveable.FrozenReason);
            tfn.Dispose();
            return null;
        }

        // Hintergrund-Operationen der Erstanlage abwarten: Läuft noch der initiale
        // Save aus Table.Create, meldet BeSureToBeUpToDate 'Tabelle wird gerade gespeichert'.
        switch (tbl) {
            case TableFileNew waitTfn:
                waitTfn.Saveable.WaitDiskOperationFinished();
                break;
            case TableFile waitTbf:
                waitTbf.WaitDiskOperationFinished();
                break;
        }

        if (!tbl.BeSureToBeUpToDate(false)) {
            problems.Add("BeSureToBeUpToDate fehlgeschlagen: " + path);
            tbl.Dispose();
            return null;
        }

        _ = tbl.LoadTableRows(false, -1);
        return tbl;
    }

    /// <summary>
    /// Erzwingt die nächste Speicherung: beim Fragment-Format über die Datei-Verwaltung, bei den klassischen
    /// Formaten über den SaveRequired-Marker (geschützter Setter, deshalb per Reflexion — bestehender Code bleibt unangetastet).
    /// </summary>
    public static void ForceSaveRequired(Table tbl) {
        switch (tbl) {
            case TableFileNew tfn:
                tfn.Saveable.MarkChanged();
                break;
            case TableFile tbf:
                var property = typeof(TableFile).GetProperty("SaveRequired", BindingFlags.Public | BindingFlags.Instance);

                if (property is null) { throw new InvalidOperationException("Property TableFile.SaveRequired nicht gefunden — Reflexion anpassen."); }

                property.SetValue(tbf, true, null);
                break;
            default:
                throw new InvalidOperationException("ForceSaveRequired unterstützt " + tbl.GetType().Name + " nicht.");
        }
    }

    /// <summary>
    /// Speichert und wartet auf den Hintergrund-Task; liefert leer bei Erfolg, sonst den Fehlertext inkl. unvollständiger Speicherung.
    /// </summary>
    public static string SaveAndWait(Table tbl) {
        OperationResult opr;

        switch (tbl) {
            case TableFileNew tfn:
                opr = tfn.Save();
                break;
            case TableFile tbf:
                opr = tbf.Save();
                break;
            default:
                return "Unbekannter Tabellen-Typ: " + tbl.GetType().Name;
        }

        if (opr.IsFailed) { return "Speichern fehlgeschlagen: " + opr.FailedReason; }

        switch (tbl) {
            case TableFileNew tfnw:
                tfnw.Saveable.WaitDiskOperationFinished();

                if (tfnw.Saveable.IsFreezed) { return "Datei-Verwaltung nach dem Speichern eingefroren: " + tfnw.Saveable.FrozenReason; }

                if (tfnw.Saveable.SaveRequired) { return "Speichern unvollständig: Es bleiben Änderungen ausständig"; }

                break;
            case TableFile tfw:
                tfw.WaitDiskOperationFinished();

                if (tfw.IsFreezed) { return "Tabelle nach dem Speichern eingefroren: " + tfw.FreezedReason; }

                if (tfw.SaveRequired) { return "Speichern unvollständig: Es bleiben Änderungen ausständig"; }

                break;
        }

        return string.Empty;
    }

    #endregion
}
