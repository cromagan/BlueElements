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

using BlueScript.Classes;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;


internal sealed class Method_Decrypt : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal, StringVal];
    public static string Command => "decrypt";
    
    public static string Description => "Entschlüsselt einen Text.";

    

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableString.ShortName_Plain;
   
    public static string Syntax => "Decrypt(OriginalString, Schlüssel)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var wert = attvar.ValueStringGet(0).Decrypt(attvar.ValueStringGet(1));

        return wert == null ? new DoItFeedback("Entschlüsselung fehlgeschlagen.", true, ld) : new DoItFeedback(wert);
    }

    #endregion
}