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

using System;
using System.Collections.Generic;
using BlueBasics.Enums;
using BlueControls.Forms;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_MsgBox : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "msgbox";
    public override List<string> Constants => [];
    public override string Description => "Zeigt ein Windows-Fenster an und wartet, dass der Nutzer eine Schaltfläche drückt.\r\nEs wird die Nummer (beginnend mit 0) des Knopfes zurückgegeben.\r\nAls Bild kann z.B. 'Information', 'Warnung', 'Kritisch', 'Uhr', etc. benutzt oder leer gelassen werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 0;
    public override MethodType MethodType => MethodType.ManipulatesUser;
    public override bool MustUseReturnValue => false;
    public override string Returns => VariableFloat.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "MsgBox(Text, Bild, Schaltflächenbeschriftung, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);

        var img = attvar.ValueStringGet(1);
        var pic = ImageCode.Information;

        if (Enum.TryParse(img, out ImageCode type)) { pic = type; }

        List<string> buttons = [];
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            buttons.Add(attvar.ValueStringGet(z));
        }

        if (buttons.Count == 0) { buttons.Add("Ok"); }

        var l = MessageBox.Show(txt, pic, true, buttons.ToArray());

        return new DoItFeedback(l);
    }

    #endregion
}