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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Xml.Linq;
using BlueScript.Methods;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_Xml : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Erstellt ein XML-Dokument, der für andere Befehle verwendet werden kann.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableXml.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "XML(XMLText)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "xml" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        try {
            var x = XDocument.Parse(((VariableString)attvar.Attributes[0]).ValueString);
            return new DoItFeedback(new VariableXml(x));
        } catch (Exception e) {
            return new DoItFeedback(string.Empty, "XML-Parsen fehlgeschlagen: " + e.Message);
        }
    }

    #endregion
}