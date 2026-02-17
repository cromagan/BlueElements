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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Classes;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_Export : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, FilterVar];

    public override string Command => "export";

    public override List<string> Constants => ["CSV", "BDB"];
    public override string Description => "Exportiert die Tabelle im angegeben Format. Achtung, bei BDB wird immer die gesamte Tabelle exportiert und die angegebenen Attribute ingnoriert.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override bool MustUseReturnValue => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "Export(Filename, CSV/BDB, AnsichtName, Filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyTable(scp) is not { IsDisposed: false } myTb) { return DoItFeedback.InternerFehler(ld); }

        #region  Filter ermitteln (allfi)

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 3, myTb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        #endregion

        var r = allFi.Rows;

        #region  Tabelle prüfen

        if (allFi.Table != myTb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellenfehler!", true, ld);
        }

        allFi.Dispose();

        if (!myTb.LoadTableRows(false, -1)) {
            return new DoItFeedback("Tabelle konnte nicht aktualisiert werden.", true, ld);
        }

        #endregion

        #region  Ansicht ermitteln (cu)

        var tcvc = ColumnViewCollection.ParseAll(myTb);

        var cu = tcvc.GetByKey(attvar.ValueStringGet(2));
        if (string.IsNullOrEmpty(attvar.ValueStringGet(2)) || cu == null) {
            cu = tcvc[0];
        }

        if (cu == null) { return new DoItFeedback("Ansicht-Fehler!", true, ld); }

        #endregion

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }
        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        var pf = filn.PathParent();
        var opr = CanWriteInDirectory(pf);
        if (!string.IsNullOrEmpty(opr)) { return new DoItFeedback(opr, true, ld); }

        if (FileExists(filn)) { return new DoItFeedback("Datei existiert bereits.", true, ld); }

        #endregion

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        try {
            switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
                case "MDB":
                case "BDB": {
                        if (myTb is not TableFile tbf) {
                            return new DoItFeedback("nur bei Dateibasierten Tabellen möglich.", true, ld);
                        }

                        var chunks = TableChunk.GenerateNewChunks(tbf, 100, DateTime.UtcNow, false);

                        if (chunks?.Count != 1 || chunks[0] is not { } mainchunk) { return new DoItFeedback("Fehler beim Erzeugen der Daten.", true, ld); }
                        mainchunk.Save(filn);
                        break;
                    }

                case "CSV":
                    var t = Controls.TableView.Export_CSV(myTb, FirstRow.ColumnInternalName, cu.ListOfUsedColumn(), r);
                    if (string.IsNullOrEmpty(t)) { return new DoItFeedback("Fehler beim Erzeugen der Daten.", true, ld); }
                    if (!WriteAllText(filn, t, BlueBasics.ClassesStatic.Constants.Win1252, false)) { return new DoItFeedback("Fehler beim Erzeugen der Datei.", true, ld); }
                    break;

                //case "HTML":
                //case "HTM":
                //    if (!db.Export_HTML(filn, cu, r, false)) { return new DoItFeedback(ld, "Fehler beim Erzeugen der Datei."); }
                //    break;

                default:
                    return new DoItFeedback("Export-Format unbekannt.", true, ld);
            }
        } catch {
            return new DoItFeedback("Allgemeiner Fehler beim Erzeugen der Daten.", true, ld);
        }

        return DoItFeedback.Null();
    }

    #endregion
}