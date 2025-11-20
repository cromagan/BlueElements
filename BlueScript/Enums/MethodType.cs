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

namespace BlueScript.Enums;

public enum MethodType {

    /// <summary>
    /// Methoden, die eine kurze Laufzeit haben und ohne weitere Bedingen eingesetzt werden können.
    /// </summary>
    Standard = 0,

    ///// <summary>
    ///// Methoden, die längere Laufzeiten haben können. Evtl. Dateizugriffe. Aber Autonom ablaufe, ohne den Bentzer zu stören.
    ///// </summary>
    LongTime = 1,

    /// <summary>
    /// Der Befehl stört den Benutzer aktiv. Z.B. wird das Clipboard verändert der ein Programm gestartet.
    /// Aber die Ausführung des Skriptes wird nicht unterbrochen.
    /// </summary>
    ManipulatesUser = 2,

    /// <summary>
    /// Dieser Befehl kann nur aktiv verwendet werden, wenn der Benutzer aktiv vor dem Bildschirm sitzt.
    /// Evtl. werden Skripte gestoppt und warten auf Benutzereingaben.
    /// </summary>
    GUI = 3,

    /// <summary>
    /// Sehr spezielle Befehle, die nur an einer einzigen Position erlaubt sind
    /// </summary>
    Special = 9
}