﻿// Authors:
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

using System.Collections.Generic;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_SoftMessage : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "softmessage";
    public override string Description => "Gibt in der Statusleiste einen Nachricht aus, wenn ein Steuerelement vorhanden ist, dass diese anzeigen kann.";
    public override bool EndlessArgs => true;

    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "SoftMessage(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var db = MyDatabase(scp);
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        var txt = "<b>Skript:</b> " + attvar.ValueStringGet(0);
        db.OnDropMessage(FehlerArt.Info, txt);

        return DoItFeedback.Null();
    }

    #endregion
}