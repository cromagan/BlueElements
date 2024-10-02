// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueBasics.Enums;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueControls.ItemCollectionPad.Abstract;
using static BlueBasics.IO;
using BlueControls.ItemCollectionPad;


namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadPadItem : BlueScript.Methods.Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "loadpaditem";

    public override List<string> Constants => [];
    public override string Description => "Lädt ein Pad-Item aus dem Dateisystem. \r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, wird ein Dummy-Symbol erzeugt.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariablePadItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadPadItem(Filename)";

    #endregion

    #region Methods

    public static TextPadItem DummyItem() {
        var t = new TextPadItem();
        t.Text = "FEHLER!";
        t.SetCoordinates(new System.Drawing.RectangleF(0, 0, 100, 100));

        return t;
}

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filen = attvar.ValueStringGet(0);


        if (filen.FileType() is not FileFormat.BlueCreativeSymbol) {
            return new DoItFeedback(ld, "Datei ist kein Symbol: " +filen);
        }

        if (!IO.FileExists(filen)) {
            //return new DoItFeedback(ld, "Datei nicht gefunden: " +filen);

            return new DoItFeedback(new VariablePadItem(DummyItem()));

        }

        try {

            var toparse = System.IO.File.ReadAllText(filen, BlueBasics.Constants.Win1252);

            var i = ParsebleItem.NewByParsing<AbstractPadItem>(toparse);
  

            if (i is not AbstractPadItem api) {
                return new DoItFeedback(ld, "Datei fehlerhaft: " + filen);

            }

            api.GetNewIdsForEverything();


            return new DoItFeedback(new VariablePadItem( api));
        } catch {
            //return new DoItFeedback(ld, "Datei konnte nicht geladen werden: " +filen);
            return new DoItFeedback(new VariablePadItem(DummyItem()));
        }


    }

    #endregion
}