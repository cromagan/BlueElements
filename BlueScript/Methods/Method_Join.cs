// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Join : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableListString.ShortName_Plain], StringVal];
    public override string Command => "join";
    public override string Description => "Wandelt eine Liste in einen Text um.\r\nEs verbindet den Text dabei mitteles dem angegebenen Verbindungszeichen.\r\nSind leere Einträge am Ende der Liste, werden die Trennzeichen am Ende nicht abgeschnitten.\r\nDas letzte Trennzeichen wird allerdings immer abgeschnitten!\r\n\r\nBeispiel: Eine Liste mit den Werten 'a' und 'b' wird beim Join mit Semikolon das zurück geben: 'a;b'\r\nAber: Wird eine Liste mit ChangeType in String umgewandelt, wäre ein zusätzliches Trennzeichen am Ende.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "Join(VariableListe, Verbindungszeichen)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var tmp = attvar.ValueListStringGet(0);
        return new DoItFeedback(string.Join(attvar.ValueStringGet(1), tmp));
    }

    #endregion
}