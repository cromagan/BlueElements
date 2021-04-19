#region BlueElements - a collection of useful tools, database and controls
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
#endregion

using BlueScript;

public struct strDoItFeedback
{


    public strDoItFeedback(string errormessage)
    {
        ErrorMessage = errormessage;
        Value = string.Empty;
    }

    public strDoItFeedback(string value, string errormessage)
    {
        ErrorMessage = errormessage;
        Value = value;
    }

    public string Value;
    public string ErrorMessage;


    public static strDoItFeedback FalscherDatentyp()
    {
        return new strDoItFeedback("Falscher Datentyp.");
    }

    public static strDoItFeedback AttributFehler(Method method, strSplittedAttributesFeedback f)
    {
        return new strDoItFeedback(f.ErrorMessage + " > " + method.Syntax);
    }

    public static strDoItFeedback VariableNichtGefunden()
    {
        return new strDoItFeedback("Variable nicht gefunden.");
    }

    public static strDoItFeedback Klammerfehler()
    {
        return new strDoItFeedback("Fehler bei der Klammersetzung.");
    }


    public static strDoItFeedback Wahr()
    {
        return new strDoItFeedback("true", string.Empty);
    }

    public static strDoItFeedback Falsch()
    {
        return new strDoItFeedback("false", string.Empty);
    }
}