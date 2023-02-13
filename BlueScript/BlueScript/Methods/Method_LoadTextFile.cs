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
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_LoadTextFile : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Lädt die angegebene Textdatei aus dem Dateisystem.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableString.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadTextFile(Filename)";

    #endregion

    #region Methods

    public override List<string>Comand(List<Variable>? currentvariables) => new() { "loadtextfile" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos, this, attvar); }

        var ft = ((VariableString)attvar.Attributes[0]).ValueString.FileType();

        if (ft is not FileFormat.Textdocument and not FileFormat.CSV) {
            return new DoItFeedback(infos, "Datei ist kein Textformat: " + ((VariableString)attvar.Attributes[0]).ValueString);
        }

        if (!IO.FileExists(((VariableString)attvar.Attributes[0]).ValueString)) {
            return new DoItFeedback(infos, "Datei nicht gefunden: " + ((VariableString)attvar.Attributes[0]).ValueString);
        }

        try {
            var importText = File.ReadAllText(((VariableString)attvar.Attributes[0]).ValueString, Constants.Win1252);
            //var bmp = (Bitmap)BitmapExt.Image_FromFile(((VariableString)attvar.Attributes[0]).ValueString)!;
            return new DoItFeedback(infos, importText, string.Empty);
        } catch {
            return new DoItFeedback(infos, "Datei konnte nicht geladen werden: " + ((VariableString)attvar.Attributes[0]).ValueString);
        }
    }

    #endregion
}