// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueControls.ItemCollection;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_GenerateLayoutImage : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, FloatVal };
    public override string Description => "Generiert ein Layout Bild.\r\nEs wird zuvor das Skript 'Export' ausgeführt und dessen Variablen verwendet.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.MyDatabaseRow | MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "GenerateLayoutImage(LayoutName, Skalierung);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "generatelayoutimage" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        #region  Meine Zeile ermitteln (r)

        var r = MyRow(s.Variables);

        if (r?.Database == null) { return new DoItFeedback(infos.Data, "Zeilenfehler!"); }

        #endregion

        #region  Layout index ermitteln (ind)

        var ca = attvar.ValueStringGet(0);

        var ind = r.Database.Layouts.LayoutCaptionToIndex(ca);

        if (ind < 0) { return new DoItFeedback(infos.Data, "Layout nicht gefunden."); }

        #endregion

        #region  scale  ermitteln (sc)

        var sc = attvar.ValueNumGet(1);
        if (sc is < 0.1 or > 10) { return new DoItFeedback(infos.Data, "Skalierung nur von 0.1 bis 10 erlaubt."); }

        #endregion

        var l = new ItemCollectionPad(r, ind);

        var bmp = l.ToBitmap((float)sc);

        if (bmp == null) { return new DoItFeedback(infos.Data, "Generierung fehlgeschlagen"); }

        return new DoItFeedback(bmp);
    }

    #endregion
}