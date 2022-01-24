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

namespace BlueControls.Enums {

    public enum enDesign {
        Undefiniert = -1, // Braucht man

        // Buttons 1000
        Button = 1000,

        Button_CheckBox = 1001,
        Button_OptionButton = 1002,
        Button_AutoFilter = 1003,
        Button_ComboBox = 1004,
        Button_Slider_Waagerecht = 1010,
        Button_Slider_Senkrecht = 1011,
        Button_SliderDesign = 1012,
        Button_EckpunktSchieber = 1020,
        Button_EckpunktSchieber_Phantom = 1021,
        TabStrip_Head = 1030,
        RibbonBar_Head = 1031,

        // Texte 1100
        Caption = 1100,

        CheckBox_TextStyle = 1101,
        OptionButton_TextStyle = 1102,

        // Caption_inSplashScreen = 1110
        // Boxen 1200
        TextBox = 1200,

        ListBox = 1201,
        ComboBox_Textbox = 1203,

        //  TreeView = 1204
        Table_And_Pad = 1205,

        EasyPic = 1207,
        TextBox_Stufe3 = 1208,
        TextBox_Bold = 1209,
        Slider_Hintergrund_Waagerecht = 1220,
        Slider_Hintergrund_Senkrecht = 1221,

        //TextBox_FürPasswort = 1225,
        //Sidebar = 1230
        // Toolbars 1400
        // ToolBar = 1400
        Ribbonbar_Button = 1401,

        Ribbonbar_Button_CheckBox = 1402,
        Ribbonbar_Button_OptionButton = 1404,
        Ribbon_ComboBox_Textbox = 1405,
        Ribbonbar_Caption = 1406,
        Ribbonbar_Button_Combobox = 1407,
        RibbonBar_Frame = 1408,

        // Forms 2000
        Form_Standard = 2000,

        //Form_WusstenSieSchon = 2001
        //Form_SplashScreen = 2002
        Form_MsgBox = 2010,

        //   Form_SelectBox = 2011
        //   Form_InputBox = 2012
        Form_QuickInfo = 2020,

        Form_DesktopBenachrichtigung = 2021,
        Form_BitteWarten = 2022,

        //Form_Balloon = 2023,
        Form_AutoFilter = 2050,

        Form_KontextMenu = 2051,
        Form_SelectBox_Dropdown = 2052,

        // Zonen 2100
        Item_DropdownMenu = 2100,

        Item_KontextMenu = 2110,
        Item_KontextMenu_Caption = 2111,
        Item_Autofilter = 2120,

        //Zone_Caption = 2130
        //Zone_Toolbar_Caption = 2135
        //  Item_Textbox = 2140
        //Zone_Listbox = 2150
        Item_Listbox = 2150,

        //Item_Listbox_Unterschrift = 2155,
        Item_Listbox_Caption = 2156,

        // Sonstiges 2200
        GroupBox = 2200,

        TabStrip_Body = 2201,
        RibbonBar_Body = 2202,
        RibbonBar_Back = 2203,
        TabStrip_Back = 2204,
        GroupBoxBold = 2205,

        Progressbar = 2210,
        Progressbar_Füller = 2211,

        //Sidebar_Level_0 = 2230
        //Sidebar_Level_1 = 2231
        //TreeView_Level_0 = 2240
        //TreeView_Level_1 = 2241
        // Listbox_Level_1 = 2243
        Table_Lines_thick = 2250,

        Table_Lines_thin = 2251,
        Table_Cursor = 2260,
        Table_Cell = 2261,
        Table_Cell_New = 2262,
        Table_Column = 2263,
        Table_Cell_Chapter = 2264
        //TextBox_Cursor = 2270
        // Unipaint-Verhalten 9800
        // UniPaint_Verhalten_PowerPoint = 9800
        //  UniPaint_Verhalten_Menukarte = 9801
        //UniPaint_Verhalten_EinzelBild = 9802
        //UniPaint_Verhalten_ListView = 9803
        //UniPaint_Verhalten_FlußDiagramm = 9804
        //UniPaint_Verhalten_CreativePad = 9805
        //UniPaint_Verhalten_ZoneObjektEditor = 9806
        //UniPaint_Verhalten_Kalender = 9820
        //UniPaint_Verhalten_WochenKalender = 9821
        // Unipaint-Items 9900
        // UniPaint_Item_DateiKästchen = 9900
        // UniPaint_Item_TransparentZoneObject = 9901
        //  UniPaint_Item_Person = 9902
        //  UniPaint_Item_Anwendung = 9903
        //  UniPaint_Item_WhiteZoneObject = 9904
        //  UniPaint_Item_Bild = 9905
        //UniPaint_Item_Miniaturansicht = 9906
        //UniPaint_Item_MenuPunkt = 9910
        //UniPaint_Item_MenuPunktAufgeklappt = 9911
        //UniPaint_Item_MenuPunktTMP = 9912
        // UniPaint_Item_Überschift = 9913
        //UniPaint_Item_MenuFilter = 9914
        //UniPaint_Item_Kalender_Feiertag = 9950
        //UniPaint_Item_Kalender_Samstag = 9951
        //UniPaint_Item_Kalender_Sonntag = 9952
        //UniPaint_Item_Kalender_Heute = 9953
        //UniPaint_Item_Kalender_JahrBox = 9960
        //UniPaint_Item_Kalender_TagBox = 9961
        //UniPaint_Item_Kalender_MonatBox = 9962
        //UniPaint_Item_Kalender_Wochentag = 9963
        //UniPaint_Item_Kalender_KWBox = 9964
        //UniPaint_Item_Kalender_DayOfWeek = 9965
    }
}