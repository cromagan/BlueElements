// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_DeleteDirectory : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "deletedirectory";
    public override List<string> Constants => [];
    public override string Description => "Löscht die Verzeichnis und dessn Inhalt aus dem Dateisystem. Gibt TRUE zurück, wenn das Verzeichnis nicht (mehr) existiert.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override bool MustUseReturnValue => false;

    public override string Returns => VariableBool.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "DeleteDirectory(Dir)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filn = attvar.ValueStringGet(0);

        if (!filn.IsFormat(FormatHolder.Filepath)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        if (!IO.DirectoryExists(filn)) {
            return DoItFeedback.Wahr();
        }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        try {
            return new DoItFeedback(IO.DeleteDir(filn, false));
        } catch {
            return new DoItFeedback("Fehler beim Löschen: " + filn, true, ld);
        }
    }

    #endregion
}