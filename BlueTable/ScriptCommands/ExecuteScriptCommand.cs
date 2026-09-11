// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Führt einen Befehl oder eine Datei über die Windows-Shell aus (UseShellExecute).
/// Startet Programme mit Argumenten, öffnet Dokumente mit dem verknüpften Programm,
/// URLs im Browser und Ordner im Windows Explorer.
/// Beispiele:
/// Execute("C:\Windows\System32\notepad.exe", "C:\Temp\Notizen.txt"); — Programm mit Argument
/// Execute("C:\Temp", ""); — Ordner im Windows Explorer öffnen
/// Execute("https://example.org", ""); — URL im Standardbrowser öffnen
/// Execute("", "C:\Temp"); — nur Argumente angegeben, Windows entscheidet anhand der Argumente
/// </summary>
internal class ExecuteScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];

    public override string Command => "execute";

    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;

    public override string Syntax => "Execute(Command, Attribut);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        IO.ExecuteFile(attvar.ValueStringGet(0), attvar.ValueStringGet(1), false, false);

        return DoItFeedback.Null();
    }

    #endregion
}
