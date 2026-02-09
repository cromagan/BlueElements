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

using BlueControls.Classes.ItemCollectionPad;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using System.Collections.Generic;
using System.Linq;

namespace BlueControls.AdditionalScriptMethods;

public class Method_GenerateLayoutImage : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal];
    public override string Command => "generatelayoutimage";
    public override List<string> Constants => [];
    public override string Description => "Generiert ein Layout Bild.\r\nEs wird zuvor das Skript 'Export' ausgeführt und dessen Variablen verwendet.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "GenerateLayoutImage(LayoutName, Skalierung);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        #region  Meine Zeile ermitteln (r)

        var r = BlockedRow(scp);
        if (r?.Table is not { IsDisposed: false }) { return new DoItFeedback("Zeilenfehler!", true, ld); }

        #endregion

        #region  Layout index ermitteln (ind)

        var ind = attvar.ValueStringGet(0);

        if (string.IsNullOrEmpty(ind)) { return new DoItFeedback("Layout nicht gefunden.", true, ld); }

        #endregion

        #region  scale  ermitteln (sc)

        var sc = attvar.ValueNumGet(1);
        if (sc is < 0.1 or > 10) { return new DoItFeedback("Skalierung nur von 0.1 bis 10 erlaubt.", true, ld); }

        #endregion

        using var l = new ItemCollectionPadItem(ind);

        if (!l.Any()) { return new DoItFeedback("Layout nicht gefunden oder fehlerhaft.", true, ld); }

        l.ResetVariables();
        var scx = l.ReplaceVariables(r);

        if (scx.Failed) {
            scx.ChangeFailedReason("Generierung fehlgeschlagen", scx.NeedsScriptFix, ld);
            return scx;
        }

        var bmp = l.ToBitmap((float)sc);

        if (bmp == null) { return new DoItFeedback("Generierung fehlgeschlagen", true, ld); }

        return new DoItFeedback(bmp);
    }

    #endregion
}