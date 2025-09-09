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
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_FreeFileName : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "freefilename";
    public override List<string> Constants => [];
    public override string Description => "Gibt einen Dateinamen (ohne Pfad / Suffix) zurück, der im anggebenen Verzeichnis nicht existiert.\r\nWird der bevorzugte Name leergelassen, wird eine zufällige Zeichenfolge generiert.\r\nWird dieser befüllt, wird eine laufende Nummer hinzugefügt\r\nHashtag: #ID #einzigartig #filename";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "FreeFileName(Path, PreferedName, Suffix)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var pf = attvar.ValueStringGet(0);
        var nam = attvar.ValueStringGet(1);
        var suf = attvar.ValueStringGet(2);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true, ld);
        }

        if (!string.IsNullOrEmpty(nam)) {
            return new DoItFeedback(TempFile(pf, nam, suf));
        }

        var zeichen = BlueBasics.Constants.Char_AZ.ToLowerInvariant() + BlueBasics.Constants.Char_Numerals + BlueBasics.Constants.Char_AZ.ToUpperInvariant();
        // Ja, lower und upper macht keinen sinn, sieht aber verrückter aus

        do {
            var p = string.Empty;
            while (p.Length < 20) {
                var pos = BlueBasics.Constants.GlobalRnd.Next(zeichen.Length);
                p += zeichen.Substring(pos, 1);
            }

            if (!FileExists(pf + p + suf)) {
                return new DoItFeedback(p);
            }
        } while (true);
    }

    #endregion
}