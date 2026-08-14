// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.ScriptCommands;

internal class FreeDirectoryNameScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "freedirectoryname";
    public override string Description => "Gibt einen zufälligen Ordnernamen (ohne Pfad) zurück, der im anggebenen Verzeichnis nicht existiert.";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "FreeDirectoryName(Path)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var pf = attvar.ValueStringGet(0);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true);
        }

        var zeichen = Char_AZ.ToLowerInvariant() + Char_Numerals + Char_AZ.ToUpperInvariant();
        // Ja, lower und upper macht keinen sinn, sieht aber verrückter aus

        do {
            Span<char> buffer = stackalloc char[20];
            for (var i = 0; i < 20; i++) {
                buffer[i] = zeichen[GlobalRnd.Next(zeichen.Length)];
            }
            var p = new string(buffer);

            if (!DirectoryExists(pf + p)) {
                return new DoItFeedback(p);
            }
        } while (true);
    }

    #endregion
}