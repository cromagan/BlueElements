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
using System.IO;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;
using static BlueBasics.IO;

namespace BlueScript.Methods;

internal class Method_DirectoryCreate : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Erstellt ein Verzeichnis, falls dieses nicht existert. Gibt TRUE zurück, erstellt wurde oder bereits existierte.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "DirectoryCreate(Path)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "directorycreate" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }

        var p = ((VariableString)attvar.Attributes[0]).ValueString.TrimEnd("\\");

        if (DirectoryExists(p)) { return DoItFeedback.Wahr(line); }

        try {
            _ = Directory.CreateDirectory(p);
        } catch { }

        return !DirectoryExists(p) ? DoItFeedback.Falsch(line) : DoItFeedback.Wahr(line);
    }

    #endregion
}