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

namespace BlueBasics.Enums;

[Flags]
public enum ScriptEventTypes {
    Ohne_Auslöser = 0,

    InitialValues = 1,
    value_changed = 2,

    /// <summary>
    /// Berechnet die Fehler, Variablen für das Formular und Virtuelle Spalten
    /// </summary>
    prepare_formula = 4,

    value_changed_extra_thread = 8,
    loaded = 16,
    export = 32,
    //clipboard_changed = 64,

    //value_changed_large = 128,
    row_deleting = 256,

    correct_changed = 512
}