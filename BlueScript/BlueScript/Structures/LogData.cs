// Authors:
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

using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueScript.Structures;

public class LogData {

    #region Fields

    public readonly List<string> Protocol = [];

    /// <summary>
    ///  In welcher Sub wir uns gerade befinden
    /// </summary>
    public readonly string Subname;

    #endregion

    #region Constructors

    public LogData(string subname, int linestart) {
        Subname = subname;
        Line = linestart;
    }

    #endregion

    #region Properties

    public int Line { get; private set; }

    #endregion

    #region Methods

    public void AddMessage(string errormessage) => Protocol.Add("[" + Subname + ", Zeile: " + Line + "]@" + errormessage);

    public void LineAdd(int c) {
        if (c < 0) {
            Develop.DebugPrint(FehlerArt.Fehler, "Wert unter null nicht erlaubt!");
        }

        Line += c;
    }

    #endregion
}