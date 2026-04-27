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
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


public sealed class Method_Screenshot : Method {

    #region Properties

    public static List<List<string>> Args => [];
    public static string Command => "screenshot";
    public static List<string> Constants => [];
    public static string Description => "Erstellt einen Screenshot und gibgt diesen zurück.";

    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.LongTime;
    public static bool MustUseReturnValue => true;
    public static string Returns => VariableBitmap.ShortName_Variable;
    public static string StartSequence => "(";
    public static string Syntax => "Screenshot()";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(ScreenShot.GrabAllScreens());

    #endregion
}