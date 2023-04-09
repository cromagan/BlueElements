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
using BlueBasics.Interfaces;
using BlueDatabase;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_BackupControl : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal };
    public override string Description => "Durchsucht das Verzeichnis nach Dateien mit dem angegebenen Filter. Löscht Dateien nach bestimmten Datumsangaben.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "BackupControl(filepath, \"table_20*.mdb\");";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "backupcontrol" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var filn = ((VariableString)attvar.Attributes[0]).ValueString;

        if (!filn.IsFormat(FormatHolder.Filepath)) { return new DoItFeedback(infos.Data, "Dateipfad-Fehler!"); }

        if (!IO.DirectoryExists(filn)) {
            return new DoItFeedback(infos.Data, "Dateipfad existiert nicht.");
        }

        var fix = Directory.GetFiles(filn, ((VariableString)attvar.Attributes[1]).ValueString, SearchOption.TopDirectoryOnly);

        if (fix.Length == 0) { return DoItFeedback.Null(); }
        try {
            var bvw = new BackupVerwalter(2, 20);

            foreach (var thisF in fix) {
                var fi = new FileInfo(thisF);
                bvw.AddData(fi.LastWriteTimeUtc, thisF);
            }

            foreach (var thisF in bvw.Deleteable) {
                IO.DeleteFile(thisF, false);
            }

            return DoItFeedback.Null();
        } catch {
            return new DoItFeedback(infos.Data, "Fehler beim Ausführen");
        }
    }

    #endregion
}