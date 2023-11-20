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

using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace BlueDatabase.AdditionalScriptMethods;

public class Method_Xml : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal };
    public override string Command => "xml";
    public override string Description => "Erstellt ein XML-Dokument, der für andere Befehle verwendet werden kann.";
    public override bool EndlessArgs => true;
    
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableXml.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "XML(XMLText)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        try {
            var x = XDocument.Parse(attvar.ValueStringGet(0));
            return new DoItFeedback(new VariableXml(x));
        } catch (Exception e) {
            return new DoItFeedback(infos.Data, "XML-Parsen fehlgeschlagen: " + e.Message);
        }
    }

    #endregion
}