// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

namespace BlueControls.Enums;

public enum ButtonStyle {
    Button = 0,
    Button_Big = 32768,
    Button_Big_Borderless = Button_Big | Borderless,

    SliderButton = 1,

    Checkbox = 2,
    Checkbox_Big_Borderless = Checkbox | Button_Big_Borderless,
    Checkbox_Text = Checkbox | Text,
    Yes_or_No = 6, // = Checkbox | 4,

    Optionbox = 16,
    Optionbox_Big_Borderless = Optionbox | Button_Big_Borderless,
    Optionbox_Text = Optionbox | Text,

    ComboBoxButton = 32,
    ComboBoxButton_Borderless = ComboBoxButton | Borderless,

    Text = 131072,

    Borderless = 65536
}