// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Variables;
using System;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal sealed class Method_ExtractFirstText : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal, StringVal, StringVal];
    public static string Command => "extractfirsttext";
    public static List<string> Constants => [];

    public static string Description => "Extrahiert aus dem gegebenen String Textstellen und gibt einen String mit dem ersten Fund zurück.\r\n" +
                                              "Wird kein Text gefunden, wird der Defaultwert zurück gegeben.\r\n" +
                                          "Beispiel: Extract(\"Ein guter Tag\", \"Ein * Tag\"); gibt den Text \"guter\" zurück.";


    public static int LastArgMinCount => -1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableString.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "ExtractFirstText(String, SearchPattern, Default);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var tags = attvar.ValueStringGet(0).ReduceToMulti(attvar.ValueStringGet(1), StringComparison.OrdinalIgnoreCase);

        return tags is not { Count: not 0 } ? new DoItFeedback(attvar.ValueStringGet(2)) : new DoItFeedback(tags[0]);
    }

    #endregion
}