﻿// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_DownloadImage : Method {

    #region Fields

    private static readonly VariableCollection Last = [];

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "downloadimage";
    public override string Description => "Lädt das angegebene Bild aus dem Internet.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.IO;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "DownloadImage(url, username, password)";

    #endregion

    #region Methods

   public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Da es keine Möglichkeit gibt, eine Bild Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        var url = attvar.ValueStringGet(0);
        var varn = "X" + url.ReduceToChars(Constants.AllowedCharsVariableName);

        if (Last.Get(varn) is VariableBitmap vb) {
            return new DoItFeedback(vb.ValueBitmap);
        }

        try {
            Generic.CollectGarbage();
            var img = Generic.DownloadImage(url);
            Bitmap? bmp = null;
            if (img is Bitmap bmp2) { bmp = bmp2; }

            Last.Add(new VariableBitmap(varn, bmp, true, string.Empty));
            return new DoItFeedback(bmp);
        } catch {
            return new DoItFeedback(null as Bitmap);
        }
    }

    #endregion
}