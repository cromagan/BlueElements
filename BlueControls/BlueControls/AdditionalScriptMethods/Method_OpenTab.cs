// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using BlueScript.Methods;

using BlueControls;
using BlueTable;

namespace BlueControls;

// ReSharper disable once UnusedMember.Global
internal class Method_OpenTab : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "opentab";
    public override List<string> Constants => [];
    public override string Description => "Öffent einen neuen Tab in allen TableViews.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "OpenTab(FileName);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var pf = attvar.ValueStringGet(0);

        if (!pf.IsFormat(FormatHolder.FilepathAndName)) {
            return new DoItFeedback("Datei ungültig: " + pf, true, ld);
        }

        if (!IO.FileExists(pf)) {
            return new DoItFeedback("Datei nicht vorhanden: " + pf, true, ld);
        }

        var tb = Table.Get(pf, null);

        if (tb is not Table { IsDisposed: false }) {
            return new DoItFeedback("Datei konnte nicht geladen werden: " + pf, true, ld);
        }

        foreach (var thisForm in FormManager.Forms) {
            if (thisForm is Forms.TableViewForm tbf && tbf.TabExists(pf) == null) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }
                tbf.AddTabPage(pf);
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}