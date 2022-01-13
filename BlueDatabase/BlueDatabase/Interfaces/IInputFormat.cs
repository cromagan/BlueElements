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

using BlueBasics;
using BlueBasics.Enums;

namespace BlueDatabase.Interfaces {

    public interface IInputFormat {

        #region Properties

        public string AllowedChars { get; set; }
        public string Regex { get; set; }
        public string Suffix { get; set; }

        #endregion
    }

    public static class IInputFormatExtensions {

        #region Methods

        public static void SetFormat(this IInputFormat t, enVarType type) {
            switch (type) {
                case enVarType.Text:
                    t.AllowedChars = string.Empty;
                    t.Regex = string.Empty;
                    t.Suffix = string.Empty;
                    return;

                case enVarType.Date:
                    t.Regex = @"^(0[1-9]|[12][0-9]|3[01]):(0[1-9]|1[0-2]):\d{4}$";
                    t.AllowedChars = Constants.Char_Numerals + ".";
                    t.Suffix = string.Empty;
                    return;

                case enVarType.Url:
                    //    https://regex101.com/r/S2CbwM/1
                    t.Regex = @"^(https:|http:|www\.)\S*$";
                    t.AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "._/";
                    t.Suffix = string.Empty;
                    return;

                case enVarType.Email:
                    //http://emailregex.com/
                    t.Regex = @"^[a-z0-9A-Z._-]{1,40}[@][a-z0-9A-Z._-]{1,40}[.][a-zA-Z]{1,3}$";
                    t.AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "@._";
                    t.Suffix = string.Empty;
                    return;

                case enVarType.Float:
                    //https://regex101.com/r/onr0NZ/1
                    t.Regex = @"(^-?([1-9]\d*)|^0)([.]\d*[1-9])?$";
                    t.AllowedChars = Constants.Char_Numerals + ",";
                    t.Suffix = string.Empty;
                    return;

                case enVarType.Integer:
                    t.Regex = @"^((-?[1-9]\d*)|0)$";
                    t.AllowedChars = Constants.Char_Numerals;
                    t.Suffix = string.Empty;
                    return;

                case enVarType.PhoneNumber:
                    //https://regex101.com/r/OzJr8j/1
                    t.Regex = @"^[+][1-9][\s0-9]*[0-9]$";
                    t.AllowedChars = Constants.Char_Numerals + "+ ";
                    t.Suffix = string.Empty;
                    return;

                case enVarType.DateTime:
                    t.Regex = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}[ ](0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$";
                    t.AllowedChars = Constants.Char_Numerals + ":. ";
                    t.Suffix = string.Empty;
                    return;

                default:
                    BlueBasics.Develop.DebugPrint(t);
                    break;
            }
        }

        #endregion
    }
}