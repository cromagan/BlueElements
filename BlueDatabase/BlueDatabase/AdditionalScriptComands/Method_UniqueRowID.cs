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

using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_UniqueRowID : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new();
    public override string Description => "Gibt eine systemweit einzigartige Zeilen-ID aller geladenen Datenbanken aus.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "UniqueRowID()";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "uniquerowid" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        return !string.IsNullOrEmpty(attvar.ErrorMessage)
            ? DoItFeedback.AttributFehler(this, attvar, line)
            : new DoItFeedback(RowCollection.UniqueKeyValue(), string.Empty, line);
    }

    #endregion
}