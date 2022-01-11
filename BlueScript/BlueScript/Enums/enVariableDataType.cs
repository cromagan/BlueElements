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

namespace Skript.Enums {

    public enum enVariableDataType {
        NotDefinedYet = 0,
        Bool = 1,
        Numeral = 2,
        String = 4,

        //Date = 8, // Werden einfach die Strings manipuliert
        List = 16,

        Bitmap = 32,
        Object = 64,

        /// <summary>
        /// Nur für Attribute
        /// </summary>
        Integer = 256,

        Nummeral_or_String = Numeral | String,
        String_or_List = String | List,
        Bool_Numeral_or_String = Bool | Numeral | String,
        Bool_Numeral_String_or_List = Bool | Numeral | String | List,
        Bool_Numeral_String_List_or_Bitmap = Bool | Numeral | String | List | Bitmap,
        Bool_Numeral_String_List_Bitmap_or_Object = Bool | Numeral | String | List | Bitmap | Object,
        Any = String | List | Numeral | Bool | Error | NotDefinedYet | Object,
        Variable = 1024,
        Error = 2048,
        Null = 4096,

        //VariableBool = 128,
        //VariableNum = 256,
        //VariableString = 512,
        Variable_Numeral = Variable | Numeral,

        Variable_List = Variable | List,
        Variable_String = Variable | String,
        Variable_List_Or_String = Variable | String | List,
        Variable_String_Numeral_or_Bool = Variable | String | Numeral | Bool,
        Variable_List_String_Numeral_or_Bool = Variable | String | List | Numeral | Bool,
        Variable_String_or_Numeral = Variable | String | Numeral,
        Variable_String_Numeral_or_List = Variable | String | List | Numeral,
        Variable_Any = Variable | String | List | Numeral | Bool | Error | NotDefinedYet | Bitmap | Object,
    }
}