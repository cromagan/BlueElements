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

public enum EditTypeTable {
    None = -1,
    Textfeld = 1,
    FileHandling_InDateiSystem = 2,

    //Image_Auswahl_Dialog = 3,
    Listbox = 4,

    Farb_Auswahl_Dialog = 5,

    //RelationEditor_InTable = 6,
    Dropdown_Single = 7,

    Font_AuswahlDialog = 8,
    Textfeld_mit_Auswahlknopf = 9,
    WarnungNurFormular = 10
}