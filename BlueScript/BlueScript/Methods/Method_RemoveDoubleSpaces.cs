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
internal class Method_RemoveDoubleSpaces : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "removedoublespaces";
    public override List<string> Constants => [];
    public override string Description => "Entfernt aus dem Text unnötige Leerzeichen, Tabs etc.\r\nKann dazu verwendet werden, um Code-Dateien (z.B. HTML) zu standardisieren.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "RemoveDoubleSpaces(text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var t = attvar.ValueStringGet(0);
        string ot;
        do {
            ot = t;
            t = t.Replace("\n", "\r");
            t = t.Replace("\t", "\r");
            t = t.Replace("  ", " ");
            t = t.Replace("\r ", "\r");
            t = t.Replace(" \r", "\r");
            t = t.Replace("\r\r", "\r");
            t = t.Replace(">\r", ">");
            t = t.Replace("\r<", "<");
            t = t.Replace(" <", "<");
            t = t.Replace("> ", ">");
        } while (ot != t);

        return new DoItFeedback(t);
    }

    #endregion
}