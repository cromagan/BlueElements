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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadTextFile : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal };
    public override string Description => "Lädt die angegebene Textdatei aus dem Dateisystem.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableString.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadTextFile(Filename)";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "loadtextfile" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var ft = attvar.ValueStringGet(0).FileType();

        if (ft is not FileFormat.Textdocument and not FileFormat.CSV) {
            return new DoItFeedback(infos.Data, "Datei ist kein Textformat: " + attvar.ValueStringGet(0));
        }

        if (!IO.FileExists(attvar.ValueStringGet(0))) {
            return new DoItFeedback(infos.Data, "Datei nicht gefunden: " + attvar.ValueStringGet(0));
        }

        try {
            var importText = File.ReadAllText(attvar.ValueStringGet(0), Constants.Win1252);
            //var bmp = (Bitmap)BitmapExt.Image_FromFile(attvar.ValueString(0))!;
            return new DoItFeedback(importText);
        } catch {
            return new DoItFeedback(infos.Data, "Datei konnte nicht geladen werden: " + attvar.ValueStringGet(0));
        }
    }

    #endregion
}