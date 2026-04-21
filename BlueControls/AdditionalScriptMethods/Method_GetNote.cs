// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueControls.Classes;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;

public class Method_GetNote : Method_TableGeneric {

    #region Fields

    public static readonly Method Method = new Method_GetNote();

    #endregion

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable]];
    public override string Command => "getnote";
    public override List<string> Constants => [];

    public override string Description => "Kann nur im Skript \"Formular vorbereiten\" benutzt werden.\r\n" +
                                          "Gibt die Texte der privaten Notizen der gewählten Spalten zurück.\r\n" +
                                          "Wird keine Spalte angegeben, werden die gesamten Notizen der ganzen Zeile zurückgegeben.\r\n" +
                                          "Die Spaltennamen müssen als Variablen übergeben werden (Spaltennamen in Anführungszeichen).";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 0;
    public override MethodType MethodLevel => MethodType.Special;
    public override bool MustUseReturnValue => true;

    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "GetNote(Column1, Column2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (scp.AdditionalInfo is not RowItem { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        var result = new List<string>();

        if (attvar.Attributes.Count == 0) {
            foreach (var col in tb.Column) {
                if (col is not { IsDisposed: false }) { continue; }
                var key = CellCollection.KeyOfCellWithTable(col, row);
                var note = PrivateNotesManager.GetNote(key);
                if (note != null && !string.IsNullOrEmpty(note.Note)) {
                    result.Add(note.Note);
                }
            }
        } else {
            for (var z = 0; z < attvar.Attributes.Count; z++) {
                var column = Column(scp, attvar, z);
                if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(z), true, ld); }
                var key = CellCollection.KeyOfCellWithTable(column, row);
                var note = PrivateNotesManager.GetNote(key);
                if (note != null && !string.IsNullOrEmpty(note.Note)) {
                    result.Add(note.Note);
                }
            }
        }

        if (result.Count == 0) { return DoItFeedback.Null(); }

        return new DoItFeedback(result);
    }

    #endregion
}