// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_BackupControl : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "backupcontrol";
    public override string Description => "Durchsucht das Verzeichnis nach Dateien mit dem angegebenen Filter. Löscht Dateien nach bestimmten Datumsangaben.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override string Syntax => "BackupControl(filepath, \"table_20*.bdb\");";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filn = attvar.ValueStringGet(0);

        if (!filn.IsFormat(FormatHolder.Filepath)) { return new DoItFeedback("Dateipfad-Fehler!", true, ld); }

        if (!IO.DirectoryExists(filn)) {
            return new DoItFeedback("Dateipfad existiert nicht.", true, ld);
        }

        var bvw = new BackupVerwalter(2, 20);
        var m = bvw.CleanUpDirectory(filn, attvar.ValueStringGet(1));
        return string.IsNullOrEmpty(m) ? DoItFeedback.Null() : new DoItFeedback("Fehler beim Ausführen", true, ld);
    }

    #endregion
}