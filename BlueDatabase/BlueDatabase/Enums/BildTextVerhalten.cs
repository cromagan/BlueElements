// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

namespace BlueDatabase.Enums;

public enum BildTextVerhalten {
    Nur_Text = 0,
    Wenn_möglich_Bild_und_immer_Text = 1,
    Nur_Bild = 110,
    Bild_oder_Text = 120,
    Interpretiere_Bool = 200,
    Fehlendes_Bild_zeige_Fragezeichen = 2,
    Fehlendes_Bild_zeige_Häkchen = 3,
    Fehlendes_Bild_zeige_Kreis = 4,
    Fehlendes_Bild_zeige_Kreuz = 5,
    Fehlendes_Bild_zeige_Infozeichen = 6,
    Fehlendes_Bild_zeige_Warnung = 7,
    Fehlendes_Bild_zeige_Kritischzeichen = 8
}