﻿// Authors:
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

namespace BlueControls.Enums;

public enum ContextMenuCommands {
    Nothing = 0,
    Ausführen = 1,
    DateiÖffnen = 2,
    Löschen = 3,

    DateiPfadÖffnen = 4,
    Bearbeiten = 5,

    InhaltLöschen = 6,
    ZeileLöschen = 7,
    Kopieren = 8,
    Ausschneiden = 9,
    SpaltenSortierungZA = 10,
    SpaltenSortierungAZ = 11,
    Information = 12,
    ZellenInhaltKopieren = 13,
    ZellenInhaltPaste = 14,

    // UserDef1 = 100
    SpaltenEigenschaftenBearbeiten = 15,

    Speichern = 16,
    Umbenennen = 17,
    SuchenUndErsetzen = 18,
    Einfügen = 19,
    VorherigenInhaltWiederherstellen = 23,
    SpaltenSortierungDefault = 25,
    Verschieben = 26,
    Abbruch = 999
}