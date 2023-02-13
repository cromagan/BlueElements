﻿// Authors:
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
using BlueBasics.Enums;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_SoftMessage : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Gibt in der Statusleiste einen Nachricht aus, wenn ein Steuerelement vorhanden ist, dass diese anzeigen kann.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ");";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "SoftMessage(Text);";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "softmessage" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos, this, attvar); }

        var db = MyDatabase(s);
        if (db == null) { return new DoItFeedback(infos, "Datenbankfehler!"); }

        var txt = "<b>Skript:</b> " + ((VariableString)attvar.Attributes[0]).ValueString;
        db.OnDropMessage(FehlerArt.Info, txt);

        return DoItFeedback.Null(infos);
    }

    #endregion
}