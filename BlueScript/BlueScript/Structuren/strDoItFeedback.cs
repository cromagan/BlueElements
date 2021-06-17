// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueScript;
using Skript.Enums;
using System.Drawing;
using static BlueBasics.modConverter;

public struct strDoItFeedback {

    #region Fields

    public string ErrorMessage;

    //public strDoItFeedback(string value, string errormessage) {
    //    ErrorMessage = errormessage;
    //    Value = value;
    //}
    public string Value;

    #endregion

    #region Constructors

    public strDoItFeedback(string errormessage) {
        ErrorMessage = errormessage;
        Value = string.Empty;
    }

    public strDoItFeedback(string value, enVariableDataType type) {
        Value = Variable.ValueForReplace(value, type);
        ErrorMessage = string.Empty;
    }

    public strDoItFeedback(Bitmap value) {
        Value = Variable.ValueForReplace(BitmapToBase64(value, System.Drawing.Imaging.ImageFormat.Png), enVariableDataType.Bitmap);
        ErrorMessage = string.Empty;
    }

    #endregion

    #region Methods

    public static strDoItFeedback AttributFehler(Method method, strSplittedAttributesFeedback f) => new(f.ErrorMessage + " > " + method.Syntax);

    public static strDoItFeedback Falsch() => new("false", enVariableDataType.Bool);

    public static strDoItFeedback FalscherDatentyp() => new("Falscher Datentyp.");

    public static strDoItFeedback Klammerfehler() => new("Fehler bei der Klammersetzung.");

    public static strDoItFeedback Wahr() => new("true", enVariableDataType.Bool);

    #endregion
}