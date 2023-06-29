// Authors:
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

using System;
using System.Collections.Generic;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_ExtractText : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal };

    public override string Description => "Extrahiert aus dem gegebenen String Textstellen und gibt eine List mit allen Funden zurück.\r\n" +
                                          "Beispiel: Extract(\"Ein guter Tag\", \"Ein * Tag\"); erstellt liste mit dem Inhalt \"guter\"";

    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "ExtractText(String, SearchPattern);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "extracttext" };

    public override DoItFeedback DoIt(VariableCollection vs, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(vs, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var tags = attvar.ValueStringGet(0).ReduceToMulti(attvar.ValueStringGet(1), StringComparison.OrdinalIgnoreCase);

        if (tags == null) {
            return new DoItFeedback(infos.Data, "Nichts extrahiert - Searchpattern fehlerhaft?");
        }

        return new DoItFeedback(tags);
    }

    #endregion
}