// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.Methods;


internal class Method_FreeFileName : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "freefilename";
    public override string Description => "Gibt einen Dateinamen (ohne Pfad / Suffix) zurück, der im anggebenen Verzeichnis nicht existiert.\r\nWird der bevorzugte Name leergelassen, wird eine zufällige Zeichenfolge generiert.\r\nWird dieser befüllt, wird eine laufende Nummer hinzugefügt\r\nHashtag: #ID #einzigartig #filename";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "FreeFileName(Path, PreferedName, Suffix)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var pf = attvar.ValueStringGet(0);
        var nam = attvar.ValueStringGet(1);
        var suf = attvar.ValueStringGet(2);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true, ld);
        }

        if (!string.IsNullOrEmpty(nam)) {
            return new DoItFeedback(TempFile(pf, nam, suf));
        }

        var zeichen = BlueBasics.ClassesStatic.Constants.Char_AZ.ToLowerInvariant() + BlueBasics.ClassesStatic.Constants.Char_Numerals + BlueBasics.ClassesStatic.Constants.Char_AZ.ToUpperInvariant();
        // Ja, lower und upper macht keinen sinn, sieht aber verrückter aus

        do {
            Span<char> buffer = stackalloc char[20];
            for (var i = 0; i < 20; i++) {
                buffer[i] = zeichen[BlueBasics.ClassesStatic.Constants.GlobalRnd.Next(zeichen.Length)];
            }
            var p = new string(buffer);

            if (!FileExists(pf + p + suf)) {
                return new DoItFeedback(p);
            }
        } while (true);
    }

    #endregion
}