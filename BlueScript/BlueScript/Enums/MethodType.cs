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

#nullable enable

using System;

namespace BlueScript.Enums;

[Flags]
public enum MethodType {

    /// <summary>
    /// Methoden, die eine Variable modifizieren oder für den normalen Betrieb benötigt werden.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Methoden, die in irgend einer Weiße auf das System zugreifen. z.B. Dateisystem, Andere Datenbank oder Clipboard
    /// </summary>
    IO = 2,

    /// <summary>
    /// Methoden, die über einer Datenbank aufgerufen werden müssen.
    /// </summary>
    Database = 4,

    /// <summary>
    /// Methoden, die sich innerhalb der aktuellen Zeile der Datenbank bewegen
    /// </summary>
    MyDatabaseRow = 8,

    /// <summary>
    /// Methoden, die andere Zeilen - egal in welcher Datenbank - ändern können.
    /// </summary>
    ChangeAnyDatabaseOrRow = 16,

    /// <summary>
    /// Methoden, die an sich harmlos sind, aber lang zum Ausführen benötigen. Unpassend für z.B. RowCheck
    /// </summary>
    NeedLongTime = 32,

    /// <summary>
    /// Der Befehl stört den Benutzer aktiv. Z.B. wird das Clipbard verändert der ein Programm gestartet.
    /// Dateioperationen fallen nicht darunter, dafür ist IO zu verwenden.
    /// </summary>
    ManipulatesUser = 64,

    Break = 128,

    AllDefault = Standard | IO | Database | MyDatabaseRow | ChangeAnyDatabaseOrRow | NeedLongTime | ManipulatesUser
}