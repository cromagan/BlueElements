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

using System.Collections.Generic;
using System.Linq;
using BlueControls.ItemCollectionPad;
using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_GenerateLayoutImage : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal];
    public override string Command => "generatelayoutimage";
    public override List<string> Constants => [];
    public override string Description => "Generiert ein Layout Bild.\r\nEs wird zuvor das Skript 'Export' ausgeführt und dessen Variablen verwendet.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Database | MethodType.MyDatabaseRow | MethodType.DrawOnBitmap;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "GenerateLayoutImage(LayoutName, Skalierung);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        #region  Meine Zeile ermitteln (r)

        var r = MyRow(scp);
        if (r?.Database is not { IsDisposed: false }) { return new DoItFeedback(ld, "Zeilenfehler!"); }

        #endregion

        #region  Layout index ermitteln (ind)

        var ind = attvar.ValueStringGet(0);

        if (string.IsNullOrEmpty(ind)) { return new DoItFeedback(ld, "Layout nicht gefunden."); }

        #endregion

        #region  scale  ermitteln (sc)

        var sc = attvar.ValueNumGet(1);
        if (sc is < 0.1 or > 10) { return new DoItFeedback(ld, "Skalierung nur von 0.1 bis 10 erlaubt."); }

        #endregion

        var l = new ItemCollectionPadItem(ind);

        if (!l.Any()) { return new DoItFeedback(ld, "Layout nicht gefunden oder fehlerhaft."); }

        l.ResetVariables();
        var scx = l.ReplaceVariables(r);

        if (!scx.AllOk) {
            ld.Protocol.AddRange(scx.Protocol);
            return new DoItFeedback(ld, "Generierung fehlgeschlagen");
        }

        var bmp = l.ToBitmap((float)sc);

        if (bmp == null) { return new DoItFeedback(ld, "Generierung fehlgeschlagen"); }

        return new DoItFeedback(bmp);
    }

    #endregion
}