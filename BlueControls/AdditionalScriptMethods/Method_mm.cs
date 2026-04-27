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

using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Converter;

namespace BlueControls.AdditionalScriptMethods;


internal sealed class Method_mm : Method, IMethod {

    #region Properties

    public static List<List<string>> Args => [FloatVal];

    public static string Command => "mm";

    public static List<string> Constants => [];
    public static string Description => "Rechnet mm in Pixel um - bei 300 dpi.";

    public static bool GetCodeBlockAfter => false;
    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.Standard;
    public static bool MustUseReturnValue => true;
    public static string Returns => VariableDouble.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "mm(Number)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(MmToPixel((float)attvar.ValueNumGet(0), ItemCollectionPadItem.Dpi));

    #endregion
}