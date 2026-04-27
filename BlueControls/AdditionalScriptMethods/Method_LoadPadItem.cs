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
using BlueBasics.Enums;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.AdditionalScriptMethods;


internal sealed class Method_LoadPadItem : Method, IMethod {

    #region Properties

    public static List<List<string>> Args => [StringVal];

    public static string Command => "loadpaditem";

    public static List<string> Constants => [];
    public static string Description => "Lädt ein Pad-Item aus dem Dateisystem. \r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, wird ein Dummy-Symbol erzeugt.";

    public static bool GetCodeBlockAfter => false;
    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.LongTime;
    public static bool MustUseReturnValue => true;
    public static string Returns => VariablePadItem.ShortName_Variable;
    public static string StartSequence => "(";
    public static string Syntax => "LoadPadItem(Filename)";

    #endregion

    #region Methods

    public static TextPadItem DummyItem() {
        var t = new TextPadItem {
            Text = "FEHLER!"
        };
        t.SetCoordinates(new RectangleF(0, 0, 100, 100));

        return t;
    }

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filen = attvar.ValueStringGet(0);

        if (filen.FileType() is not FileFormat.BlueCreativeSymbol) {
            return new DoItFeedback("Datei ist kein Symbol: " + filen, true, ld);
        }

        if (!IO.FileExists(filen)) {
            //return new DoItFeedback(ld, "Datei nicht gefunden: " +filen);

            return new DoItFeedback(new VariablePadItem(DummyItem()));
        }

        try {
            var toparse = IO.ReadAllText(filen, BlueBasics.Constants.Win1252);

            var i = ParseableItem.NewByParsing<AbstractPadItem>(toparse);

            if (i is not { IsDisposed: false } api) {
                return new DoItFeedback("Datei fehlerhaft: " + filen, true, ld);
            }

            api.GetNewIdsForEverything();

            return new DoItFeedback(new VariablePadItem(api));
        } catch {
            //return new DoItFeedback(ld, "Datei konnte nicht geladen werden: " +filen);
            return new DoItFeedback(new VariablePadItem(DummyItem()));
        }
    }

    #endregion
}