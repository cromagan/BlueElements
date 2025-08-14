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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Windows;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_ClipboardText : Method {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "clipboardtext";
    public override List<string> Constants => [];
    public override string Description => "Gibt den Inhalt des Windows Clipboards als Text zurück. Falls kein Text im Clipboard enthalten ist, wird ein leerer String zurückgegeben.\r\nMit SetClipoard kann ein Wert in das Clipboard geschrieben werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "ClipboardText()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, CanDoFeedback ld)=> Clipboard.ContainsText() ? new DoItFeedback(Clipboard.GetText(), ld.EndPosition()) : DoItFeedback.Null(ld.EndPosition());

    #endregion
}