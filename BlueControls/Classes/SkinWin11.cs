// Authors:
// Christian Peter
//
// Copyright (c) 2026 Christian Peter
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

using BlueControls.Enums;
using static BlueControls.Enums.Design;
using static BlueControls.Enums.States;
using static BlueControls.Enums.Kontur;
using static BlueControls.Enums.HintergrundArt;
using static BlueControls.Enums.RahmenArt;
using static System.String;
using System.Collections.Generic;

namespace BlueControls.Classes;

public static class SkinWin11 {

    #region Methods

    public static void Load(Dictionary<Design, Dictionary<States, SkinDesign>> design) {
        design.Add(Button, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Button, Standard_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button, Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Button, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_3px, "B6B6B6", Empty, Empty);
        design.Add(Button, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button_CheckBox, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Button_CheckBox, Checked, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "81B8EF", Empty, Empty);
        design.Add(Button_CheckBox, Checked_Disabled, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "DFDFDF", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(Button_CheckBox, Checked_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_3px, "81B8EF", Empty, Empty);
        design.Add(Button_CheckBox, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Button_CheckBox, Checked_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button_CheckBox, Checked_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button_CheckBox, Checked_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button_CheckBox, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_3px, "B6B6B6", Empty, Empty);
        design.Add(Button_CheckBox, Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button_CheckBox, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button_CheckBox, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button_OptionButton, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Button_OptionButton, Checked, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "81B8EF", Empty, Empty);
        design.Add(Button_OptionButton, Checked_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "DFDFDF", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(Button_OptionButton, Checked_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_3px, "81B8EF", Empty, Empty);
        design.Add(Button_OptionButton, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Button_OptionButton, Checked_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button_OptionButton, Checked_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button_OptionButton, Checked_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button_OptionButton, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_3px, "B6B6B6", Empty, Empty);
        design.Add(Button_OptionButton, Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button_OptionButton, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Button_OptionButton, Standard_MouseOver_HasFocus, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_AutoFilter, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Button_AutoFilter, Checked, Empty, Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "FF0000", Empty, Empty);
        design.Add(Button_AutoFilter, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Button_ComboBox, Standard, Empty, Rechteck, -3, -3, -3, -3, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_ComboBox, Standard_HasFocus_MousePressed, Empty, Rechteck, -3, -3, -3, -3, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_ComboBox, Standard_MouseOver_HasFocus_MousePressed, Empty, Rechteck, -3, -3, -3, -3, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_ComboBox, Standard_Disabled, Empty, Rechteck, -3, -3, -3, -3, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_ComboBox, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "EAEAEA", Empty, Solide_3px, "B6B6B6", Empty, Empty);
        design.Add(Button_ComboBox, Standard_HasFocus, Empty, Rechteck, -3, -3, -3, -3, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_ComboBox, Standard_MouseOver_HasFocus, Empty, Rechteck, -3, -3, -3, -3, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);

        design.Add(Button_Slider_Waagerecht, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "CDCDCD", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(Button_Slider_Waagerecht, Standard_MouseOver_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "CECECE", Empty, Solide_3px, "B7B7B7", Empty, Empty);
        design.Add(Button_Slider_Waagerecht, Standard_Disabled, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_Slider_Waagerecht, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "CECECE", Empty, Solide_3px, "B7B7B7", Empty, Empty);
        design.Add(Button_Slider_Senkrecht, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "CDCDCD", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(Button_Slider_Senkrecht, Standard_MouseOver_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "CECECE", Empty, Solide_3px, "B7B7B7", Empty, Empty);
        design.Add(Button_Slider_Senkrecht, Standard_Disabled, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_Slider_Senkrecht, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "CECECE", Empty, Solide_3px, "B7B7B7", Empty, Empty);

        design.Add(Button_SliderDesign, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Button_SliderDesign, Standard_MouseOver_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, Solide_3px, "3399FF", Empty, Empty);
        design.Add(Button_SliderDesign, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "F9F9F9", Empty, RahmenArt.Ohne, "D8D8D8", Empty, Empty);
        design.Add(Button_SliderDesign, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, RahmenArt.Ohne, "B6B6B6", Empty, Empty);

        design.Add(Button_EckpunktSchieber, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Button_EckpunktSchieber, Checked_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Button_EckpunktSchieber_Phantom, Standard, Empty, Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Button_EckpunktSchieber_Joint, Standard, "Windows 11 Blue|5|0", Rechteck, 0, 0, 0, 0, Solide, "FFD800", Empty, Solide_1px, "FF6A00", Empty, Empty);

        design.Add(TabStrip_Head, Standard, "Windows 11|0", Rechteck, 0, -2, 0, 5, Verlauf_Vertical_2, "F0F0F0", "E4E4E4", Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(TabStrip_Head, Checked, "Windows 11|0", Rechteck, 0, 0, 0, 5, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(TabStrip_Head, Checked_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 5, Solide, "DFDFDF", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(TabStrip_Head, Checked_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 5, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(TabStrip_Head, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, -2, 0, 5, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(TabStrip_Head, Standard_MouseOver, "Windows 11|0", Rechteck, 0, -2, 0, 5, Verlauf_Vertical_2, "F0F0F0", "E4E4E4", Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(RibbonBar_Head, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 5, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(RibbonBar_Head, Checked, "Windows 11|0", Rechteck, 0, 0, 0, 5, Solide, "F4F5F6", Empty, Solide_1px, "E5E4E5", Empty, Empty);
        design.Add(RibbonBar_Head, Checked_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 5, Solide, "F4F5F6", Empty, Solide_1px, "E5E4E5", Empty, Empty);
        design.Add(RibbonBar_Head, Checked_MouseOver, "Windows 11 Blue|0|0", Rechteck, 0, 0, 0, 5, Solide, "F4F5F6", Empty, Solide_1px, "E5E4E5", Empty, Empty);
        design.Add(RibbonBar_Head, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 5, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(RibbonBar_Head, Standard_MouseOver, "Windows 11 Blue|0|0", Rechteck, 0, 0, 0, 5, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Caption, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Caption, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(CheckBox_TextStyle, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "CheckBox");
        design.Add(CheckBox_TextStyle, Checked, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "CheckBox_Checked");
        design.Add(CheckBox_TextStyle, Checked_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "CheckBox_Disabled_Checked");
        design.Add(CheckBox_TextStyle, Checked_MouseOver, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "CheckBox_Checked_MouseOver");
        design.Add(CheckBox_TextStyle, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "CheckBox_Disabled");
        design.Add(CheckBox_TextStyle, Checked_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "CheckBox_Checked");
        design.Add(CheckBox_TextStyle, Checked_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "CheckBox_Checked_MouseOver");
        design.Add(CheckBox_TextStyle, Checked_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "CheckBox_Checked");
        design.Add(CheckBox_TextStyle, Standard_MouseOver, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "CheckBox_MouseOver");
        design.Add(CheckBox_TextStyle, Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "CheckBox_Checked_MouseOver");
        design.Add(CheckBox_TextStyle, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "CheckBox");
        design.Add(CheckBox_TextStyle, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "CheckBox_MouseOver");
        design.Add(OptionButton_TextStyle, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "OptionBox");
        design.Add(OptionButton_TextStyle, Checked, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "OptionBox_Checked");
        design.Add(OptionButton_TextStyle, Checked_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "OptionBox_Disabled_Checked");
        design.Add(OptionButton_TextStyle, Checked_MouseOver, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "OptionBox_Checked_MouseOver");
        design.Add(OptionButton_TextStyle, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "OptionBox_Disabled");
        design.Add(OptionButton_TextStyle, Checked_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "OptionBox_Checked");
        design.Add(OptionButton_TextStyle, Checked_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "OptionBox_Checked_MouseOver");
        design.Add(OptionButton_TextStyle, Checked_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "OptionBox");
        design.Add(OptionButton_TextStyle, Standard_MouseOver, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, "OptionBox_MouseOver");
        design.Add(OptionButton_TextStyle, Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "OptionBox_Checked_MouseOver");
        design.Add(OptionButton_TextStyle, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "OptionBox");
        design.Add(OptionButton_TextStyle, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, FocusDotLine, Empty, "000000", "OptionBox_MouseOver");
        design.Add(TextBox, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(TextBox, Checked, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(TextBox, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(TextBox, Checked_HasFocus, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(TextBox, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(ListBox, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(ListBox, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(ComboBox_Textbox, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(ComboBox_Textbox, Standard_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(ComboBox_Textbox, Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(ComboBox_Textbox, Checked, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(ComboBox_Textbox, Checked_MouseOver, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(ComboBox_Textbox, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(ComboBox_Textbox, Checked_HasFocus, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(ComboBox_Textbox, Checked_MouseOver_HasFocus, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(ComboBox_Textbox, Checked_HasFocus_MousePressed, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(ComboBox_Textbox, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(ComboBox_Textbox, Checked_MouseOver_HasFocus_MousePressed, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(ComboBox_Textbox, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(ComboBox_Textbox, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Table_And_Pad, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Table_And_Pad, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Table_And_Pad, Standard_MouseOver_HasFocus, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Table_And_Pad, Standard_MouseOver_HasFocus_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Table_And_Pad, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Table_And_Pad, Standard_HasFocus, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(EasyPic, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(EasyPic, Standard_HasFocus_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(EasyPic, Standard_MouseOver_HasFocus_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(EasyPic, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(EasyPic, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(EasyPic, Standard_HasFocus, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Slider_Hintergrund_Waagerecht, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Slider_Hintergrund_Waagerecht, Standard_MouseOver_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, RahmenArt.Ohne, "B7B7B7", Empty, Empty);
        design.Add(Slider_Hintergrund_Waagerecht, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "F9F9F9", Empty, RahmenArt.Ohne, "D8D8D8", Empty, Empty);
        design.Add(Slider_Hintergrund_Waagerecht, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, RahmenArt.Ohne, "B7B7B7", Empty, Empty);
        design.Add(Slider_Hintergrund_Waagerecht, Standard_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, RahmenArt.Ohne, "B7B7B7", Empty, Empty);
        design.Add(Slider_Hintergrund_Senkrecht, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Slider_Hintergrund_Senkrecht, Standard_MouseOver_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, RahmenArt.Ohne, "B7B7B7", Empty, Empty);
        design.Add(Slider_Hintergrund_Senkrecht, Standard_MouseOver_HasFocus_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, RahmenArt.Ohne, "B7B7B7", Empty, Empty);
        design.Add(Slider_Hintergrund_Senkrecht, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "F9F9F9", Empty, RahmenArt.Ohne, "D8D8D8", Empty, Empty);
        design.Add(Slider_Hintergrund_Senkrecht, Standard_MouseOver, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, RahmenArt.Ohne, "B7B7B7", Empty, Empty);
        design.Add(Slider_Hintergrund_Senkrecht, Standard_MousePressed, Empty, Rechteck, 0, 0, 0, 0, Solide, "EFEFEF", Empty, RahmenArt.Ohne, "B7B7B7", Empty, Empty);
        design.Add(Ribbonbar_Button, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button, Standard_HasFocus_MousePressed, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button, Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Ribbonbar_Button, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Checked, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "81B8EF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Checked_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "DFDFDF", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Checked_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "81B8EF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Checked_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Checked_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Checked_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_CheckBox, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Checked, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "81B8EF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Checked_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "DFDFDF", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Checked_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "81B8EF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Checked_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Checked_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Checked_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Button_OptionButton, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Standard_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Checked, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Checked_MouseOver, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Checked_HasFocus, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Checked_MouseOver_HasFocus, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Checked_HasFocus_MousePressed, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Standard_MouseOver, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Checked_MouseOver_HasFocus_MousePressed, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Standard_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbon_ComboBox_Textbox, Standard_MouseOver_HasFocus, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "3399FF", Empty, Empty);
        design.Add(Ribbonbar_Caption, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Caption, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_Combobox, Standard, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_Combobox, Standard_HasFocus_MousePressed, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_Combobox, Standard_MouseOver_HasFocus_MousePressed, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_Combobox, Standard_Disabled, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_Combobox, Standard_MouseOver, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_Combobox, Standard_HasFocus, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Ribbonbar_Button_Combobox, Standard_MouseOver_HasFocus, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(RibbonBar_Frame, Standard, "Windows 11 Italic|0|0", Rechteck, 1, 1, 0, 1, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "ACACAC", Empty, Empty);
        design.Add(RibbonBar_Frame, Standard_Disabled, "Windows 11 Italic Disabled|0|1", Rechteck, 1, 1, 0, 1, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "ACACAC", Empty, Empty);
        design.Add(Form_Standard, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Form_MsgBox, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Form_QuickInfo, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "4DA1B5", Empty, Empty);
        design.Add(Form_DesktopBenachrichtigung, Standard, "Windows 11|6", Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, Solide_3px, "5D5D5D", Empty, Empty);
        design.Add(Form_BitteWarten, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "4DA1B5", Empty, Empty);
        design.Add(Form_AutoFilter, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Form_AutoFilter, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Form_KontextMenu, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "F0F0F0", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Form_SelectBox_Dropdown, Standard, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Item_DropdownMenu, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_DropdownMenu, Checked, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_DropdownMenu, Checked_Disabled, "Windows 11 Checked Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "5D5D5D", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_DropdownMenu, Checked_MouseOver, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Item_DropdownMenu, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_DropdownMenu, Standard_MouseOver, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_KontextMenu, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_KontextMenu, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_KontextMenu, Standard_MouseOver, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_KontextMenu_Caption, Standard, "Windows 11|7", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_KontextMenu_Caption, Standard_Disabled, "Windows 11 Disabled|7", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_KontextMenu_Caption, Standard_MouseOver, "Windows 11|7", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "4DA1B5", Empty, Empty);
        design.Add(Item_Autofilter, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Autofilter, Standard_MouseOver_HasFocus_MousePressed, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Autofilter, Checked, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Autofilter, Checked_Disabled, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "5D5D5D", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Autofilter, Checked_MouseOver, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, Solide_1px, "000000", Empty, Empty);
        design.Add(Item_Autofilter, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Autofilter, Standard_MouseOver, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Autofilter, Standard_HasFocus, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Autofilter, Standard_MouseOver_HasFocus, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Listbox, Standard, "Windows 11|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Listbox, Standard_MouseOver_MousePressed, "Windows 11 MouseOver|0", Rechteck, 0, 0, 0, 0, Solide, "CCE8FF", Empty, Solide_1px, "99D1FF", Empty, Empty);
        design.Add(Item_Listbox, Checked, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "CCE8FF", Empty, Solide_1px, "99D1FF", Empty, Empty);
        design.Add(Item_Listbox, Checked_Disabled, "Windows 11 Disabled|0", Rechteck, 0, 0, 0, 0, Solide, "DFDFDF", Empty, Solide_1px, "B7B7B7", Empty, Empty);
        design.Add(Item_Listbox, Checked_MouseOver, "Windows 11 MouseOver|0", Rechteck, 0, 0, 0, 0, Solide, "CCE8FF", Empty, Solide_1px, "99D1FF", Empty, Empty);
        design.Add(Item_Listbox, Standard_Disabled, "Windows 11 Disabled|0", Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Listbox, Checked_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "CCE8FF", Empty, Solide_1px, "99D1FF", Empty, Empty);
        design.Add(Item_Listbox, Standard_MouseOver, "Windows 11 MouseOver|0", Rechteck, 0, 0, 0, 0, Solide, "E5F3FF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Listbox, Standard_MousePressed, "Windows 11|0", Rechteck, 0, 0, 0, 0, Solide, "CCE8FF", Empty, Solide_1px, "99D1FF", Empty, Empty);
        design.Add(Item_Listbox_Caption, Standard, "Windows 11|7", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Listbox_Caption, Standard_Disabled, "Windows 11 Checked|0", Rechteck, 0, 0, 0, 0, Solide, "DFDFDF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Item_Listbox_Caption, Standard_MouseOver, "Windows 11|7", Rechteck, 0, 0, 0, 0, Solide, "BFDFFF", Empty, Solide_1px, "4DA1B5", Empty, Empty);
        design.Add(GroupBox, Standard, "Windows 11|0", Rechteck, 0, -7, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "ACACAC", Empty, Empty);
        design.Add(GroupBox, Standard_Disabled, "Windows 11 Disabled|0", Rechteck, 0, -7, 0, 0, HintergrundArt.Ohne, Empty, Empty, Solide_1px, "ACACAC", Empty, Empty);
        design.Add(GroupBoxBold, Standard, "Windows 11 Checked|7", Rechteck, 9, -11, 9, 9, HintergrundArt.Ohne, Empty, Empty, Solide_21px, "40568D", Empty, Empty);
        design.Add(GroupBoxBold, Standard_Disabled, "Windows 11 Disabled|7", Rechteck, 9, -11, 9, 9, HintergrundArt.Ohne, Empty, Empty, Solide_21px, "ACACAC", Empty, Empty);
        design.Add(GroupBox_RoundRect, Standard, "Windows 11|0", Rechteck_R4, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(GroupBox_RoundRect, Standard_Disabled, "Windows 11 Disabled|0", Rechteck_R4, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "D8D8D8", Empty, Empty);
        design.Add(TabStrip_Body, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "ACACAC", Empty, Empty);
        design.Add(TabStrip_Body, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "ACACAC", Empty, Empty);
        design.Add(RibbonBar_Body, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "F4F5F6", Empty, Solide_1px, "E5E4E5", Empty, Empty);
        design.Add(RibbonBar_Body, Standard_Disabled, Empty, Rechteck, 0, 0, 0, 0, Solide, "F4F5F6", Empty, Solide_1px, "E5E4E5", Empty, Empty);
        design.Add(RibbonBar_Back, Standard, Empty, Rechteck, 0, 0, 0, 5, Solide, "FFFFFF", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(TabStrip_Back, Standard, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Progressbar, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "FFFFFF", Empty, Solide_1px, "B6B6B6", Empty, Empty);
        design.Add(Progressbar_Füller, Standard, Empty, Rechteck, 0, 0, 0, 0, Solide, "0072BC", Empty, RahmenArt.Ohne, Empty, Empty, Empty);
        design.Add(Table_Lines_thick, Standard, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, "ACACAC", Empty, Empty);
        design.Add(Table_Lines_thin, Standard, Empty, Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, Empty, Empty, RahmenArt.Ohne, "D8D8D8", Empty, Empty);
        design.Add(Table_Cursor, Standard, Empty, Rechteck, -1, -1, -1, -1, HintergrundArt.Ohne, Empty, Empty, Solide_3px, "ACACAC", Empty, Empty);
        design.Add(Table_Cursor, Standard_HasFocus, Empty, Rechteck, -1, -1, -1, -1, HintergrundArt.Ohne, Empty, Empty, Solide_3px, "3399FF", Empty, Empty);
    }

    #endregion
}