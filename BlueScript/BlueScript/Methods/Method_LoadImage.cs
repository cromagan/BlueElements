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
using System.Drawing;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadImage : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal };
    public override string Description => "Lädt das angegebene Bild aus dem Dateisystem.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadImage(Filename)";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "loadimage" };

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        // Da es keine Möglichit gibt, eine Bild Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        //if (attvar.ValueString(0).FileType() != FileFormat.Image) {
        //    return new DoItFeedback(infos.Data, "Datei ist kein Bildformat: " + attvar.ValueString(0));
        //}

        //if (!IO.FileExists(attvar.ValueString(0))) {
        //    return new DoItFeedback(infos.Data, "Datei nicht gefunden: " + attvar.ValueString(0));
        //}

        try {
            Generic.CollectGarbage();
            var bmp = (Bitmap)Image_FromFile(attvar.ValueStringGet(0))!;
            return new DoItFeedback(bmp);
        } catch {
            return new DoItFeedback(null as Bitmap);
            //return new DoItFeedback(infos.Data, "Datei konnte nicht geladen werden: " + attvar.ValueString(0));
        }
    }

    #endregion
}