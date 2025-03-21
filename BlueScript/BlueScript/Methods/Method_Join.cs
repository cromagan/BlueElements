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
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_Join : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableListString.ShortName_Plain], StringVal];
    public override string Command => "join";
    public override List<string> Constants => [];
    public override string Description => "Wandelt eine Liste in einen Text um.\r\nEs verbindet den Text dabei mitteles dem angegebenen Verbindungszeichen.\r\nSind leere Einträge am Ende der Liste, werden die Trennzeichen am Ende nicht abgeschnitten.\r\nDas letzte Trennzeichen wird allerdings immer abgeschnitten!\r\n\r\nBeispiel: Eine Liste mit den Werten 'a' und 'b' wird beim Join mit Semikolon das zurück geben: 'a;b'\r\nAber: Wird eine Liste mit ChangeType in String umgewandelt, wäre ein zusätzliches Trennzeichen am Ende.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Join(VariableListe, Verbindungszeichen)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var tmp = attvar.ValueListStringGet(0);
        return new DoItFeedback(tmp.JoinWith(attvar.ValueStringGet(1)));
    }

    #endregion
}