// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.ScriptCommands;


/// <summary>
/// Gibt einen Dateinamen (ohne Pfad / Suffix) zurück, der im anggebenen Verzeichnis nicht existiert.
/// Wird der bevorzugte Name leergelassen, wird eine zufällige Zeichenfolge generiert.
/// Wird dieser befüllt, wird eine laufende Nummer hinzugefügt
/// Hashtag: #ID #einzigartig #filename
/// </summary>
internal class FreeFileNameScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "freefilename";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "FreeFileName(Path, PreferedName, Suffix)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var pf = attvar.ValueStringGet(0);
        var nam = attvar.ValueStringGet(1);
        var suf = attvar.ValueStringGet(2);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true);
        }

        if (!string.IsNullOrEmpty(nam)) {
            return new DoItFeedback(TempFile(pf, nam, suf));
        }

        var zeichen = Char_AZ.ToLowerInvariant() + Char_Numerals + Char_AZ.ToUpperInvariant();
        // Ja, lower und upper macht keinen sinn, sieht aber verrückter aus

        do {
            Span<char> buffer = stackalloc char[20];
            for (var i = 0; i < 20; i++) {
                buffer[i] = zeichen[GlobalRnd.Next(zeichen.Length)];
            }
            var p = new string(buffer);

            if (!FileExists(pf + p + suf)) {
                return new DoItFeedback(p);
            }
        } while (true);
    }

    #endregion
}
