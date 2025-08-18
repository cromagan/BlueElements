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

using System;

namespace BlueScript.Enums;

[Flags]
public enum MethodType {

    /// <summary>
    /// Methoden, die eine Variable modifizieren oder für den normalen Betrieb benötigt werden.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Einfache Berechnungen
    /// </summary>
    Math = 2,

    /// <summary>
    /// Methoden, die über einer Tabelle aufgerufen werden müssen.
    /// Z.B. weil sie die eigene Tabelle brauchen, um andere Tabellen finden zu können.
    /// </summary>
    Database = 4,

    /// <summary>
    /// Methoden, die sich innerhalb der aktuellen Zeile der Tabelle bewegen. Muss immer gemeinsam mit dem Attribut 'Database' verwendet werden.
    /// </summary>
    MyDatabaseRow = 8,

    DrawOnBitmap = 16,

    /// <summary>
    /// Der Befehl stört den Benutzer aktiv. Z.B. wird das Clipbard verändert der ein Programm gestartet.
    /// Dateioperationen fallen nicht darunter, dafür ist IO zu verwenden.
    /// </summary>
    ManipulatesUser = 64,

    /// <summary>
    /// Dieser Befehl generiert abseits des normalen Weges neue Variablen. Oder, erstellt einen speziellen Rückgabewert, der nicht für Abfragen gedacht ist.
    /// Z.B. kann dieser Befehl dann nicht in If-Abfragen benutzt werden.
    /// </summary>
    SpecialVariables = 128,

    /// <summary>
    ///  Sehr spezielle Befehle, die nur an einer einzigen Position erlaubt sind
    /// </summary>
    Special = 256,
}