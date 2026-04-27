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

using BlueBasics.Enums;
using BlueControls.Forms;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


public sealed class Method_MsgBox : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal, StringVal, StringVal];
    public static string Command => "msgbox";
    
    public static string Description => "Zeigt ein Windows-Fenster an und wartet, dass der Nutzer eine Schaltfläche drückt.\r\nEs wird die Nummer (beginnend mit 0) des Knopfes zurückgegeben.\r\nAls Bild kann z.B. 'Information', 'Warnung', 'Kritisch', 'Uhr', etc. benutzt oder leer gelassen werden.";

    public static int LastArgMinCount => 0;
    public static MethodType MethodLevel => MethodType.GUI;

    public static string Returns => VariableDouble.ShortName_Variable;
   
    public static string Syntax => "MsgBox(Text, Bild, Schaltflächenbeschriftung, ...);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);

        var img = attvar.ValueStringGet(1);
        var pic = ImageCode.Information;

        if (Enum.TryParse(img, out ImageCode type)) { pic = type; }

        List<string> buttons = [];
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            buttons.Add(attvar.ValueStringGet(z));
        }

        if (buttons.Count == 0) { buttons.Add("Ok"); }

        var l = MessageBox.Show(txt, pic, true, [.. buttons]);

        return new DoItFeedback(l);
    }

    #endregion
}