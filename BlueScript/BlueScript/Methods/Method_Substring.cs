﻿// Authors:
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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_Substring : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal, FloatVal];
    public override string Command => "substring";
    public override List<string> Constants => [];
    public override string Description => "Gibt einen Teilstring zurück. Ist der Start oder das Ende keine gültige Position, wird das bestmögliche zurückgegeben und kein Fehler ausgelöst. Subrtring(\"Hallo\", 2,2) gibt ll zurück.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Substring(String, Start, Anzahl)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var st = attvar.ValueIntGet(1);
        var en = attvar.ValueIntGet(2);
        if (st < 0) {
            en += st;
            st = 0;
        }

        var t = attvar.ValueStringGet(0);

        if (st > t.Length) { return DoItFeedback.Null(); }

        if (st + en > t.Length) { en = t.Length - st; }
        return new DoItFeedback(t.Substring(st, en));
    }

    #endregion
}