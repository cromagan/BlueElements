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

using System;

namespace BlueTable.Enums;

[Flags]
public enum Reason : byte {

    /// <summary>
    /// Wichtigster Wert, wenn Änderungen permanent geloggt werden sollen.
    /// LogUndo wird das Logbuch aktualisiert und Werte fest in die Datenbank geschreieben.
    /// </summary>
    LogUndo = 1,

    /// <summary>
    /// Typische Anwendung: Daten werden nachgeladen.
    /// Die Zugehörigen Steuerelemente müssen reagieren und sich anpassen,
    /// aber ansonsten werden keine weiteren Schritte (Reparaturn, Logs) erwünscht,
    /// weil das die Laderoutine macht
    /// </summary>
    RaiseEvents = 2,

    /// <summary>
    /// Skripte ausführen, DateChanged, etc setzen.
    /// </summary>
    DoRepair = 4,

    IgnoreFreeze = 8,

    NoUndo_NoInvalidate = IgnoreFreeze,

    SetCommand = LogUndo | RaiseEvents | DoRepair,
}