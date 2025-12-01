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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using static BlueBasics.Polygons;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

//  = A3 & ".Design.GenerateAndAdd(enStates."&B3&", enKontur."& C3 & ", " &D3&", "&E3&", "&F3&","&G3&", enHintergrundArt."&H3&","&I3&",'"&J3&"','"&K3&"','"&L3&"',enRahmenArt."&M3&",'"&N3&"','"&O3&"','"&P3&"','"&Q3&"','"&R3&"');"

//	Control	Status	Kontur	X1	X2	Y1	Y2	Draw Back	Verlauf Mitte	Color Back 1	Color Back 2	Color Back 3	Border Style	Color Border 1	Color Border 2	Color Border 3	Schrift	StandardPic
//	Button	Standard	Rechteck					Solide		EAEAEA			Solide_1px	B6B6B6			Windows 11|0|0
//	Button	Standard_HasFocus_MousePressed	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			Windows 11|0|0
//	Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		EAEAEA			Solide_3px	3399FF			Windows 11|0|0
//	Button	Standard_Disabled	Rechteck					Solide		EFEFEF			Solide_1px	D8D8D8			Win11 Disabled/X10006
//	Button	Standard_MouseOver	Rechteck					Solide		EAEAEA			Solide_3px	B6B6B6			Windows 11|0|0
//	Button	Standard_HasFocus	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			Windows 11|0|0
//	Button	Standard_MouseOver_HasFocus	Rechteck					Solide		EAEAEA			Solide_3px	3399FF			Windows 11|0|0
//	Button_CheckBox	Standard	Rechteck					Solide		EAEAEA			Solide_1px	B6B6B6			Windows 11|0|0
//	Button_CheckBox	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			Windows 11|0|0
//	Button_CheckBox	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			Win11 Checked/X10006
//	Button_CheckBox	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_3px	81B8EF			Windows 11|0|0
//	Button_CheckBox	Standard_Disabled	Rechteck					Solide		EFEFEF			Solide_1px	D8D8D8			Win11 Disabled/X10006
//	Button_CheckBox	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			Windows 11|0|0
//	Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Button_CheckBox	Standard_MouseOver	Rechteck					Solide		EAEAEA			Solide_3px	B6B6B6			Windows 11|0|0
//	Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			Windows 11|0|0
//	Button_CheckBox	Standard_HasFocus	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			Windows 11|0|0
//	Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck					Solide		EAEAEA			Solide_3px	3399FF			Windows 11|0|0
//	Button_OptionButton	Standard	Rechteck					Solide		EAEAEA			Solide_1px	B6B6B6			Windows 11|0|0
//	Button_OptionButton	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			Windows 11|0|0
//	Button_OptionButton	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			Win11 Checked/X10006
//	Button_OptionButton	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_3px	81B8EF			Windows 11|0|0
//	Button_OptionButton	Standard_Disabled	Rechteck					Solide		EFEFEF			Solide_1px	D8D8D8			Win11 Disabled/X10006
//	Button_OptionButton	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			Windows 11|0|0
//	Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Button_OptionButton	Standard_MouseOver	Rechteck					Solide		EAEAEA			Solide_3px	B6B6B6			Windows 11|0|0
//	Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			Windows 11|0|0
//	Button_OptionButton	Standard_HasFocus	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			Windows 11|0|0
//	Button_OptionButton	Standard_MouseOver_HasFocus															Windows 11|0|0
//	Button_AutoFilter	Standard	Rechteck					Solide		EAEAEA			Solide_1px	B6B6B6
//	Button_AutoFilter	Checked	Rechteck					Solide		BFDFFF			Solide_1px	FF0000
//	Button_AutoFilter	Standard_Disabled	Rechteck					Solide		EFEFEF			Solide_1px	D8D8D8
//	Button_ComboBox	Standard	Rechteck	-3	-3	-3	-3	Solide		FFFFFF			Ohne
//	Button_ComboBox	Standard_HasFocus_MousePressed	Rechteck	-3	-3	-3	-3	Solide		FFFFFF			Ohne
//	Button_ComboBox	Standard_MouseOver_HasFocus_MousePressed	Rechteck	-3	-3	-3	-3	Solide		FFFFFF			Ohne
//	Button_ComboBox	Standard_Disabled	Rechteck	-3	-3	-3	-3	Solide		FFFFFF			Ohne
//	Button_ComboBox	Standard_MouseOver	Rechteck					Solide		EAEAEA			Solide_3px	B6B6B6
//	Button_ComboBox	Standard_HasFocus	Rechteck	-3	-3	-3	-3	Solide		FFFFFF			Ohne
//	Button_ComboBox	Standard_MouseOver_HasFocus	Rechteck	-3	-3	-3	-3	Solide		FFFFFF			Ohne
//	Button_Slider_Waagerecht	Standard	Rechteck					Solide		CDCDCD			Solide_1px	B7B7B7
//	Button_Slider_Waagerecht	Standard_MouseOver_MousePressed	Rechteck					Solide		CECECE			Solide_3px	B7B7B7
//	Button_Slider_Waagerecht	Standard_Disabled	Ohne					Ohne					Ohne
//	Button_Slider_Waagerecht	Standard_MouseOver	Rechteck					Solide		CECECE			Solide_3px	B7B7B7
//	Button_Slider_Senkrecht	Standard	Rechteck					Solide		CDCDCD			Solide_1px	B7B7B7
//	Button_Slider_Senkrecht	Standard_MouseOver_MousePressed	Rechteck					Solide		CECECE			Solide_3px	B7B7B7
//	Button_Slider_Senkrecht	Standard_Disabled	Ohne					Ohne					Ohne
//	Button_Slider_Senkrecht	Standard_MouseOver	Rechteck					Solide		CECECE			Solide_3px	B7B7B7
//	Button_SliderDesign	Standard	Rechteck					Solide		F0F0F0			Ohne
//	Button_SliderDesign	Standard_HasFocus_MousePressed	Rechteck					Solide		EFEFEF			Solide_1px	3399FF
//	Button_SliderDesign	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		F0F0F0			Solide_3px	3399FF
//	Button_SliderDesign	Standard_Disabled	Rechteck					Solide		F9F9F9			Ohne	D8D8D8
//	Button_SliderDesign	Standard_MouseOver	Rechteck					Solide		F0F0F0			Ohne	B6B6B6
//	Button_SliderDesign	Standard_HasFocus	Rechteck					Solide		F0F0F0			Solide_1px	3399FF
//	Button_SliderDesign	Standard_MouseOver_HasFocus	Rechteck					Solide		F0F0F0			Solide_3px	3399FF
//	Button_EckpunktSchieber	Standard	Rechteck					Solide		FFFFFF			Solide_1px	0
//	Button_EckpunktSchieber	Checked_MousePressed	Rechteck					Solide		0072BC			Solide_1px	0
//	Button_EckpunktSchieber_Phantom	Standard	Rechteck					Ohne					Solide_1px	D8D8D8
//	TabStrip_Head	Standard	Rechteck			-2	5	Verlauf_Vertical_Solide	0,5	F0F0F0	E4E4E4		Solide_1px	B6B6B6			Windows 11|0|0
//	TabStrip_Head	Checked	Rechteck				5	Solide		FFFFFF			Solide_1px	B6B6B6			Windows 11|0|0
//	TabStrip_Head	Checked_Disabled	Rechteck				5	Solide		DFDFDF			Solide_1px	B7B7B7			Win11 Checked/X10006
//	TabStrip_Head	Checked_MouseOver	Rechteck				5	Solide		FFFFFF			Solide_1px	B6B6B6			Windows 11|0|0
//	TabStrip_Head	Standard_Disabled	Rechteck			-2	5	Solide		FFFFFF			Solide_1px	D8D8D8			Win11 Disabled/X10006
//	TabStrip_Head	Standard_MouseOver	Rechteck			-2	5	Verlauf_Vertical_Solide	0,5	F0F0F0	E4E4E4		Solide_1px	B6B6B6			Windows 11|0|0
//	RibbonBar_Head	Standard	Rechteck				5	Solide		FFFFFF			Ohne				Windows 11|0|0
//	RibbonBar_Head	Checked	Rechteck				5	Solide		F4F5F6			Solide_1px	E5E4E5			Windows 11|0|0
//	RibbonBar_Head	Checked_Disabled	Rechteck				5	Solide		F4F5F6			Solide_1px	E5E4E5			Win11 Disabled/X10006
//	RibbonBar_Head	Checked_MouseOver	Rechteck				5	Solide		F4F5F6			Solide_1px	E5E4E5			Win11 Blue/X10006
//	RibbonBar_Head	Standard_Disabled	Rechteck				5	Solide		FFFFFF			Ohne				Win11 Disabled/X10006
//	RibbonBar_Head	Standard_MouseOver	Rechteck				5	Solide		FFFFFF			Ohne				Win11 Blue/X10006
//	Caption	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Caption	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	CheckBox_TextStyle	Standard	Ohne					Ohne					Ohne				Windows 11|0|0	CheckBox
//	CheckBox_TextStyle	Checked	Ohne					Ohne					Ohne				Windows 11|0|0	CheckBox_Checked
//	CheckBox_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006	CheckBox_Disabled_Checked
//	CheckBox_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				Windows 11|0|0	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006	CheckBox_Disabled
//	CheckBox_TextStyle	Checked_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	CheckBox_Checked
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Checked_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	CheckBox_Checked
//	CheckBox_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				Windows 11|0|0	CheckBox_MouseOver
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	CheckBox
//	CheckBox_TextStyle	Standard_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	CheckBox_MouseOver
//	OptionButton_TextStyle	Standard	Ohne					Ohne					Ohne				Windows 11|0|0	OptionBox
//	OptionButton_TextStyle	Checked	Ohne					Ohne					Ohne				Windows 11|0|0	OptionBox_Checked
//	OptionButton_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006	OptionBox_Disabled_Checked
//	OptionButton_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				Windows 11|0|0	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006	OptionBox_Disabled
//	OptionButton_TextStyle	Checked_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	OptionBox_Checked
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Checked_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				Windows 11|0|0	OptionBox_MouseOver
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	Windows 11|0|0	OptionBox_MouseOver
//	TextBox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			Windows 11|0|0
//	TextBox	Checked	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	TextBox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			Win11 Disabled/X10006
//	TextBox	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	TextBox	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	ListBox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	ListBox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8
//	ComboBox_Textbox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			Windows 11|0|0
//	ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	ComboBox_Textbox	Checked	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	ComboBox_Textbox	Checked_MouseOver	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	ComboBox_Textbox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			Win11 Disabled/X10006
//	ComboBox_Textbox	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	ComboBox_Textbox	Standard_MouseOver	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			Windows 11|0|0
//	ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	ComboBox_Textbox	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	Table_And_Pad	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	Table_And_Pad	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8
//	Table_And_Pad	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	EasyPic	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	EasyPic	Standard_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	EasyPic	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	EasyPic	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8
//	EasyPic	Standard_MouseOver	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	EasyPic	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	TextBox_Stufe3	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			Win11/X10001
//	TextBox_Stufe3	Checked	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10001
//	TextBox_Stufe3	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			Win11 Disabled/X10001
//	TextBox_Stufe3	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10001
//	TextBox_Stufe3	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Win11/X10001
//	TextBox_Bold	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			Win11/X10007
//	TextBox_Bold	Checked	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10007
//	TextBox_Bold	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, CanvasSize=10[K]15, Bold=True, Color=9d9d9d}
//	TextBox_Bold	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10007
//	TextBox_Bold	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Win11/X10007
//	Slider_Hintergrund_Waagerecht	Standard	Rechteck					Solide		F0F0F0			Ohne
//	Slider_Hintergrund_Waagerecht	Standard_MouseOver_MousePressed	Rechteck					Solide		EFEFEF			Ohne	B7B7B7
//	Slider_Hintergrund_Waagerecht	Standard_Disabled	Rechteck					Solide		F9F9F9			Ohne	D8D8D8
//	Slider_Hintergrund_Waagerecht	Standard_MouseOver	Rechteck					Solide		EFEFEF			Ohne	B7B7B7
//	Slider_Hintergrund_Waagerecht	Standard_MousePressed	Rechteck					Solide		EFEFEF			Ohne	B7B7B7
//	Slider_Hintergrund_Senkrecht	Standard	Rechteck					Solide		F0F0F0			Ohne
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver_MousePressed	Rechteck					Solide		EFEFEF			Ohne	B7B7B7
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		EFEFEF			Ohne	B7B7B7
//	Slider_Hintergrund_Senkrecht	Standard_Disabled	Rechteck					Solide		F9F9F9			Ohne	D8D8D8
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver	Rechteck					Solide		EFEFEF			Ohne	B7B7B7
//	Slider_Hintergrund_Senkrecht	Standard_MousePressed	Rechteck					Solide		EFEFEF			Ohne	B7B7B7
//	Ribbonbar_Button	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Ribbonbar_Button	Standard_HasFocus_MousePressed	Ohne					Ohne					Ohne				Windows 11|0|0
//	Ribbonbar_Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Ohne					Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Ribbonbar_Button	Standard_MouseOver	Rechteck					Ohne					Solide_1px	D8D8D8			Windows 11|0|0
//	Ribbonbar_Button	Standard_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button	Standard_MouseOver_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			Win11 Checked/X10006
//	Ribbonbar_Button_CheckBox	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Ribbonbar_Button_CheckBox	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Standard_MouseOver	Rechteck					Ohne					Solide_1px	D8D8D8			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Standard_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			Win11 Checked/X10006
//	Ribbonbar_Button_OptionButton	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Ribbonbar_Button_OptionButton	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Standard_MouseOver	Rechteck					Ohne					Solide_1px	D8D8D8			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Standard_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			Windows 11|0|0
//	Ribbon_ComboBox_Textbox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			Windows 11|0|0
//	Ribbon_ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbon_ComboBox_Textbox	Checked	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Ribbon_ComboBox_Textbox	Checked_MouseOver	Rechteck					Solide		0072BC			Solide_1px	0			Win11 Checked/X10006
//	Ribbon_ComboBox_Textbox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			Win11 Disabled/X10006
//	Ribbon_ComboBox_Textbox	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck					Solide		0072BC			Solide_1px	0			Win11 Checked/X10006
//	Ribbon_ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Ribbon_ComboBox_Textbox	Standard_MouseOver	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			Windows 11|0|0
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		0072BC			Solide_1px	0			Win11 Checked/X10006
//	Ribbon_ComboBox_Textbox	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			Windows 11|0|0
//	Ribbonbar_Caption	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Ribbonbar_Caption	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Ribbonbar_Button_Combobox	Standard	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_Disabled	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus	Ohne					Ohne					Ohne
//	RibbonBar_Frame	Standard	Rechteck	1	0	1	1	Ohne					Solide_1px	ACACAC			{Name=Calibri, CanvasSize=10[K]15, Italic=True}
//	RibbonBar_Frame	Standard_Disabled	Rechteck	1	0	1	1	Ohne					Solide_1px	ACACAC			Win11 Disabled/X10006
//	Form_Standard	Standard	Rechteck					Solide		F0F0F0			Ohne
//	Form_MsgBox	Standard	Rechteck					Solide		F0F0F0			Ohne				Windows 11|0|0
//	Form_QuickInfo	Standard	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			Windows 11|0|0
//	Form_DesktopBenachrichtigung	Standard	Rechteck					Solide		1F1F1F			Solide_3px	484848			{Name=Calibri, CanvasSize=12[K]5, Color=ffffff}
//	Form_BitteWarten	Standard	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			Windows 11|0|0
//	Form_AutoFilter	Standard	Rechteck					Solide		FFFFFF			Solide_1px	0			Windows 11|0|0
//	Form_AutoFilter	Standard_Disabled															Win11 Disabled/X10006
//	Form_KontextMenu	Standard	Rechteck					Solide		F0F0F0			Solide_1px	0
//	Form_SelectBox_Dropdown	Standard	Rechteck					Solide		FFFFFF			Solide_1px	0			Windows 11|0|0
//	Item_DropdownMenu	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Item_DropdownMenu	Checked	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Item_DropdownMenu	Checked_Disabled	Rechteck					Solide		5D5D5D			Ohne				Win11 Checked/X10006
//	Item_DropdownMenu	Checked_MouseOver	Rechteck					Solide		0072BC			Solide_1px	0			Win11 Checked/X10006
//	Item_DropdownMenu	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Item_DropdownMenu	Standard_MouseOver	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Item_KontextMenu	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Item_KontextMenu	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Item_KontextMenu	Standard_MouseOver	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Item_KontextMenu_Caption	Standard	Rechteck					Solide		BFDFFF			Ohne				Win11/X10007
//	Item_KontextMenu_Caption	Standard_Disabled	Rechteck					Solide		BFDFFF			Ohne				Win11/X10007
//	Item_KontextMenu_Caption	Standard_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			Win11/X10007
//	Item_Autofilter	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Item_Autofilter	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Item_Autofilter	Checked	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Item_Autofilter	Checked_Disabled	Rechteck					Solide		5D5D5D			Ohne				Win11 Checked/X10006
//	Item_Autofilter	Checked_MouseOver	Rechteck					Solide		0072BC			Solide_1px	0			Win11 Checked/X10006
//	Item_Autofilter	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Item_Autofilter	Standard_MouseOver	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Item_Autofilter	Standard_HasFocus	Ohne					Ohne					Ohne				Windows 11|0|0
//	Item_Autofilter	Standard_MouseOver_HasFocus	Rechteck					Solide		0072BC			Ohne				Win11 Checked/X10006
//	Item_Listbox	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Item_Listbox	Standard_MouseOver_MousePressed	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			"Windows 11 MouseOver|0"
//	Item_Listbox	Checked	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			Windows 11|0|0
//	Item_Listbox	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			Win11 Checked/X10006
//	Item_Listbox	Checked_MouseOver	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			"Windows 11 MouseOver|0"
//	Item_Listbox	Standard_Disabled	Ohne					Ohne					Ohne				Win11 Disabled/X10006
//	Item_Listbox	Checked_MousePressed	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			Windows 11|0|0
//	Item_Listbox	Standard_MouseOver	Rechteck					Solide		E5F3FF			Ohne				"Windows 11 MouseOver|0"
//	Item_Listbox	Standard_MousePressed	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			Windows 11|0|0
//	Item_Listbox_Caption	Standard	Rechteck					Solide		BFDFFF			Ohne				Win11/X10007
//	Item_Listbox_Caption	Standard_Disabled	Rechteck					Solide		DFDFDF			Ohne				Win11 Checked/X10007
//	Item_Listbox_Caption	Standard_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			Win11/X10007
//	Frame	Standard	Rechteck			-7		Ohne					Solide_1px	ACACAC			Windows 11|0|0
//	Frame	Standard_Disabled	Rechteck			-7		Ohne					Solide_1px	ACACAC			Win11 Disabled/X10006
//	TabStrip_Body	Standard	Rechteck					Solide		FFFFFF			Solide_1px	ACACAC
//	TabStrip_Body	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	ACACAC
//	RibbonBar_Body	Standard	Rechteck					Solide		F4F5F6			Solide_1px	E5E4E5
//	RibbonBar_Body	Standard_Disabled	Rechteck					Solide		F4F5F6			Solide_1px	E5E4E5
//	RibbonBar_Back	Standard	Rechteck				5	Solide		FFFFFF			Ohne
//	TabStrip_Back	Standard	Ohne					Ohne					Ohne
//	Progressbar	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	Progressbar_Füller	Standard	Rechteck					Solide		0072BC			Ohne
//	Table_Lines_thick	Standard	Ohne					Ohne					Ohne	ACACAC
//	Table_Lines_thin	Standard	Ohne					Ohne					Ohne	D8D8D8
//	Table_Cursor	Standard	Rechteck	-1	-1	-1	-1	Ohne					Solide_3px	ACACAC
//	Table_Cursor	Standard_HasFocus	Rechteck	-1	-1	-1	-1	Ohne					Solide_3px	3399FF
//	Table_Cell	Standard	Ohne					Ohne					Ohne				Windows 11|0|0
//	Table_Cell_New	Standard	Ohne					Ohne					Ohne				{Name=Calibri, CanvasSize=10[K]15, Italic=True}
//	Table_Column	Standard	Ohne					Ohne					Ohne				Win11/X10007
//	Table_Cell_Chapter	Standard															Win11/X10003

//	Control	Status	Kontur	X1	X2	Y1	Y2	Draw Back	Verlauf Mitte	Color Back 1	Color Back 2	Color Back 3	Border Style	Color Border 1	Color Border 2	Color Border 3	Schrift	StandardPic
//	Button	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	003C74			{Name=Arial, CanvasSize=9, Color=000000}
//	Button	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Button	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button	Standard_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, CanvasSize=9, Color=000000}
//	Button	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	003C74			{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	003C74			{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_Disabled	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Button_CheckBox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	003C74			{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	003C74			{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked_Disabled	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Button_OptionButton	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Button_OptionButton	Checked_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Button_AutoFilter	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	003C74
//	Button_AutoFilter	Checked	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0	D1CCC1	E3E2DA	DCD7CB	Solide_1px	003C74
//	Button_AutoFilter	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		F5F4EA			Solide_1px	C9C7BA
//	Button_ComboBox	Standard	Rechteck_R4	-3	-3	-3	-3	Verlauf_Vertical_3	0,7	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_ComboBox	Standard_HasFocus_MousePressed	Rechteck_R4	-3	-3	-3	-3	Verlauf_Diagonal_3	0,7	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2
//	Button_ComboBox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	-3	-3	-3	-3	Verlauf_Diagonal_3	0,7	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2
//	Button_ComboBox	Standard_Disabled	Rechteck_R4	-3	-3	-3	-3	Verlauf_Vertical_3	0,7	F7F7F4	EDEDE6	E6E6DD	Solide_1px	E8E8DF
//	Button_ComboBox	Standard_MouseOver	Rechteck_R4	-3	-3	-3	-3	Verlauf_Diagonal_3	0,7	FDFFFF	D2EAFE	B9DDFB	Solide_1px	97AEE2
//	Button_ComboBox	Standard_HasFocus	Rechteck_R4	-3	-3	-3	-3	Verlauf_Vertical_3	0,7	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_ComboBox	Standard_MouseOver_HasFocus	Rechteck_R4	-3	-3	-3	-3	Verlauf_Diagonal_3	0,7	FDFFFF	D2EAFE	B9DDFB	Solide_1px	97AEE2
//	Button_Slider_Waagerecht	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,7	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_Slider_Waagerecht	Standard_MouseOver_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,5	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2
//	Button_Slider_Waagerecht	Standard_Disabled	Ohne					Ohne					Ohne
//	Button_Slider_Waagerecht	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,7	FDFFFF	D2EAFE	B9DDFB	Solide_1px	97AEE2
//	Button_Slider_Senkrecht	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,7	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_Slider_Senkrecht	Standard_MouseOver_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,5	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2
//	Button_Slider_Senkrecht	Standard_Disabled	Ohne					Ohne					Ohne
//	Button_Slider_Senkrecht	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,7	FDFFFF	D2EAFE	B9DDFB	Solide_1px	97AEE2
//	Button_SliderDesign	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,7	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_SliderDesign	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,7	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2
//	Button_SliderDesign	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,7	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2
//	Button_SliderDesign	Standard_Disabled	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,7	F7F7F4	EDEDE6	E6E6DD	Solide_1px	E8E8DF
//	Button_SliderDesign	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,7	FDFFFF	D2EAFE	B9DDFB	Solide_1px	97AEE2
//	Button_SliderDesign	Standard_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,7	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_SliderDesign	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Diagonal_3	0,7	FDFFFF	D2EAFE	B9DDFB	Solide_1px	97AEE2
//	Button_EckpunktSchieber	Standard	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,7	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_EckpunktSchieber	Checked_MousePressed	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,7	AEC8F7	C1D3FB	AEC8F7	Solide_1px	97AEE2
//	Button_EckpunktSchieber_Phantom	Standard	Rechteck	0	0	0	0	Ohne					Solide_1px	97AEE2
//	TabStrip_Head	Standard	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	003C74			{Name=Arial, CanvasSize=9, Color=000000}
//	TabStrip_Head	Checked	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	919B9C			{Name=Arial, CanvasSize=9, Color=000000}
//	TabStrip_Head	Checked_Disabled	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	TabStrip_Head	Checked_MouseOver	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px_DuoColor_NurOben	919B9C	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	TabStrip_Head	Standard_Disabled	Rechteck_R4	0	0	0	5	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	TabStrip_Head	Standard_MouseOver	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px_DuoColor_NurOben	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	RibbonBar_Head	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	RibbonBar_Head	Checked	Rechteck	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	919B9C			{Name=Arial, CanvasSize=9, Color=000000}
//	RibbonBar_Head	Checked_Disabled	Rechteck	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	RibbonBar_Head	Checked_MouseOver	Rechteck	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	919B9C			{Name=Arial, CanvasSize=9, Color=0000ff}
//	RibbonBar_Head	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	RibbonBar_Head	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=0000ff}
//	Caption	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	CheckBox_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	CheckBox
//	CheckBox_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}	CheckBox_Disabled_Checked
//	CheckBox_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}	CheckBox_Disabled
//	CheckBox_TextStyle	Checked_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_Checked
//	CheckBox_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_MouseOver
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	CheckBox
//	CheckBox_TextStyle	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	CheckBox_MouseOver
//	OptionButton_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	OptionBox
//	OptionButton_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}	OptionBox_Disabled_Checked
//	OptionButton_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}	OptionBox_Disabled
//	OptionButton_TextStyle	Checked_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}	OptionBox_MouseOver
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}	OptionBox_MouseOver
//	TextBox	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	TextBox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	TextBox	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	TextBox	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	TextBox	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	ListBox	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	ListBox	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	ComboBox_Textbox	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	ComboBox_Textbox	Checked_MouseOver	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	ComboBox_Textbox	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	ComboBox_Textbox	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	ComboBox_Textbox	Standard_MouseOver	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	ComboBox_Textbox	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, CanvasSize=9, Color=000000}
//	Table_And_Pad	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	Table_And_Pad	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	Table_And_Pad	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	003C74
//	EasyPic	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	EasyPic	Standard_MouseOver	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	TextBox_Stufe3	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=12, Bold=True, Underline=True, Color=000000}
//	TextBox_Stufe3	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=12, Bold=True, Underline=True, Color=ffffff}
//	TextBox_Stufe3	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=12, Bold=True, Underline=True, Color=a6a6a6}
//	TextBox_Stufe3	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=12, Bold=True, Underline=True, Color=ffffff}
//	TextBox_Stufe3	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=12, Bold=True, Underline=True, Color=000000}
//	TextBox_Bold	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Bold=True, Color=000000}
//	TextBox_Bold	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Bold=True, Color=ffffff}
//	TextBox_Bold	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Bold=True, Color=a6a6a6}
//	TextBox_Bold	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Bold=True, Color=ffffff}
//	TextBox_Bold	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Bold=True, Color=000000}
//	Slider_Hintergrund_Waagerecht	Standard	Rechteck	3	3	0	0	Verlauf_Vertical_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Waagerecht	Standard_MouseOver_MousePressed	Rechteck	3	3	0	0	Verlauf_Vertical_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Waagerecht	Standard_Disabled	Rechteck	3	3	0	0	Verlauf_Vertical_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Waagerecht	Standard_MouseOver	Rechteck	3	3	0	0	Verlauf_Vertical_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Waagerecht	Standard_MousePressed	Rechteck	3	3	0	0	Verlauf_Vertical_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Senkrecht	Standard	Rechteck	0	0	3	3	Verlauf_Horizontal_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver_MousePressed	Rechteck	0	0	3	3	Verlauf_Horizontal_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	3	3	Verlauf_Horizontal_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Senkrecht	Standard_Disabled	Rechteck	0	0	3	3	Verlauf_Horizontal_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver	Rechteck	0	0	3	3	Verlauf_Horizontal_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Slider_Hintergrund_Senkrecht	Standard_MousePressed	Rechteck	0	0	3	3	Verlauf_Horizontal_Solide		F3F3EC	FEFEFB		Solide_1px	EEEDE5
//	Ribbonbar_Button	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button	Standard_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button	Standard_HasFocus	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_Disabled	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	BDBAA2	ECE9D8	FFFFFF	Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_HasFocus	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_Disabled	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	BDBAA2	ECE9D8	FFFFFF	Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_HasFocus	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px_FocusDotLine	0		404040	{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	-3	-3	-3	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_Disabled	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver	Rechteck_R4	0	-3	-3	-3	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck_R4	0	-3	-3	-3	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Caption	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Ribbonbar_Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_Combobox	Standard	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_Disabled	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	RibbonBar_Frame	Standard	Rechteck	1	0	1	1	Ohne					Solide_1px	0			{Name=Arial, CanvasSize=9, Color=0000ff}
//	RibbonBar_Frame	Standard_Disabled	Rechteck	1	0	1	1	Ohne					Solide_1px	C9C7BA			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Form_Standard	Standard	Rechteck	0	0	0	0	Solide		ECE9D8			Ohne
//	Form_MsgBox	Standard	Rechteck	0	0	0	0	Solide		ECE9D8			Ohne	7F9DB9			{Name=Arial, CanvasSize=9, Color=000000}
//	Form_QuickInfo	Standard	Rechteck	0	0	0	0	Solide		FFFFEE			Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Form_DesktopBenachrichtigung	Standard	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Form_BitteWarten	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFEE			Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Form_AutoFilter	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Form_AutoFilter	Standard_Disabled															{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Form_KontextMenu	Standard	Rechteck	0	0	0	0	Solide		F4F3EE			Solide_1px	0
//	Form_SelectBox_Dropdown	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	0			{Name=Arial, CanvasSize=9, Color=000000}
//	Item_DropdownMenu	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Item_DropdownMenu	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_DropdownMenu	Checked_Disabled	Rechteck	0	0	0	0	Solide		BDBAA2			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_DropdownMenu	Checked_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	316AC5	C1D3FB	316AC5	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_DropdownMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Item_DropdownMenu	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=000000}
//	Item_KontextMenu	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Item_KontextMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Item_KontextMenu	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=000000}
//	Item_KontextMenu_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Ohne	97AEE2			{Name=Arial, CanvasSize=9, Bold=True, Color=000000}
//	Item_KontextMenu_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	BDBAA2	F4F3EE	F4F3EE	Ohne				{Name=Arial, CanvasSize=9, Bold=True, Color=a6a6a6}
//	Item_KontextMenu_Caption	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Bold=True, Color=000000}
//	Item_Autofilter	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Item_Autofilter	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=000000}
//	Item_Autofilter	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Autofilter	Checked_Disabled	Rechteck	0	0	0	0	Solide		BDBAA2			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Autofilter	Checked_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	316AC5	C1D3FB	316AC5	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Autofilter	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Item_Autofilter	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=000000}
//	Item_Autofilter	Standard_HasFocus	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Item_Autofilter	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=000000}
//	Item_Listbox	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Item_Listbox	Standard_MouseOver_MousePressed	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Listbox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Listbox	Checked_Disabled	Rechteck	0	0	0	0	Solide		BDBAA2			Ohne				{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Listbox	Checked_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	316AC5	C1D3FB	316AC5	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Listbox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	Item_Listbox	Checked_MousePressed	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Listbox	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Color=ffffff}
//	Item_Listbox	Standard_MousePressed	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Item_Listbox_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Bold=True, Color=000000}
//	Item_Listbox_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	BDBAA2	F4F3EE	F4F3EE	Ohne				{Name=Arial, CanvasSize=9, Bold=True, Color=a6a6a6}
//	Item_Listbox_Caption	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Solide_1px	97AEE2			{Name=Arial, CanvasSize=9, Bold=True, Color=000000}
//	Frame	Standard	Rechteck_RRechteckRechteck	0	0	-6	0	Ohne					Solide_1px	919B9C			{Name=Arial, CanvasSize=9, Color=0000ff}
//	Frame	Standard_Disabled	Rechteck_RRechteckRechteck	0	0	-6	0	Ohne					Solide_1px	919B9C			{Name=Arial, CanvasSize=9, Color=a6a6a6}
//	TabStrip_Body	Standard	Rechteck	0	0	0	0	Solide		F4F3EE			ShadowBox	919B9C	D0CEBF	E3E0D0
//	TabStrip_Body	Standard_Disabled	Rechteck	0	0	0	0	Solide		F4F3EE			ShadowBox	919B9C	D0CEBF	E3E0D0
//	RibbonBar_Body	Standard	Rechteck	0	0	0	0	Verlauf_Vertical_Solide	0,5	F4F3EE	FFFFFF		Solide_1px	003C74
//	RibbonBar_Body	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Vertical_Solide	0,5	F4F3EE	FFFFFF		Solide_1px	003C74
//	RibbonBar_Back	Standard	Rechteck	0	0	0	5	Solide		ECE9D8			Ohne
//	TabStrip_Back	Standard						Ohne					Ohne
//	Progressbar	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	Progressbar_Füller	Standard	Rechteck	0	0	0	0	Verlauf_Vertical_Solide		FFFFFF	0000FF		Solide_1px	7F9DB9
//	Table_Lines_thick	Standard	Ohne					Ohne					Ohne	003C74
//	Table_Lines_thin	Standard	Ohne					Ohne					Ohne	C9C7BA
//	Table_Cursor	Standard	Rechteck	-1	-1	-1	-1	Ohne					Solide_3px	C9C7BA
//	Table_Cursor	Standard_HasFocus	Rechteck	-1	-1	-1	-1	Ohne					Solide_3px	0
//	Table_Cell	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Color=000000}
//	Table_Cell_New	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Italic=True, Color=a6a6a6}
//	Table_Column	Standard	Ohne					Ohne					Ohne				{Name=Arial, CanvasSize=9, Bold=True, Color=ffffff}
//	Table_Cell_Chapter	Standard															{Name=Arial, CanvasSize=14, Bold=True, Underline=True, Color=000000}

//	Control	Status	Kontur	X1	X2	Y1	Y2	Draw Back	Verlauf Mitte	Color Back 1	Color Back 2	Color Back 3	Border Style	Color Border 1	Color Border 2	Color Border 3	Schrift	StandardPic
//	Button	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button	Standard_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	Glossy		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button	Standard_Disabled	Rechteck_R2Ohne	0	0	0	0	Glossy		87B7CD			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Button	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_CheckBox	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_CheckBox	Checked	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_Disabled	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Button_CheckBox	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard_Disabled	Rechteck_R2Ohne	0	0	0	0	Glossy		87B7CD			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Button_CheckBox	Checked_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_CheckBox	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_OptionButton	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_OptionButton	Checked	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked_Disabled	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Button_OptionButton	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard_Disabled	Rechteck_R2Ohne	0	0	0	0	Glossy		87B7CD			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Button_OptionButton	Checked_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Button_OptionButton	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Button_AutoFilter	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF
//	Button_AutoFilter	Checked	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px	0
//	Button_AutoFilter	Standard_Disabled	Rechteck_R2Ohne	0	0	0	0	Glossy		87B7CD			Solide_1px	C9C7BA
//	Button_ComboBox	Standard	Rechteck_R2Ohne	-2	-2	-2	-2	Glossy		0095DD			Solide_1px	FFFFFF
//	Button_ComboBox	Standard_HasFocus_MousePressed	Rechteck_R2Ohne	-4	-4	-4	-4	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_ComboBox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-4	-4	-4	-4	Glossy		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_ComboBox	Standard_Disabled	Rechteck_R2Ohne	-2	-2	-2	-2	Glossy		87B7CD			Solide_1px	C9C7BA
//	Button_ComboBox	Standard_MouseOver	Rechteck_R2Ohne	-2	-2	-2	-2	Glossy		2FBBFF			Solide_1px	FFFFFF
//	Button_ComboBox	Standard_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_ComboBox	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_Slider_Waagerecht	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF
//	Button_Slider_Waagerecht	Standard_MouseOver_MousePressed	Rechteck_R2Ohne	-1	-1	-1	-1	Glossy		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_Slider_Waagerecht	Standard_Disabled	Ohne					Ohne					Ohne
//	Button_Slider_Waagerecht	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF
//	Button_Slider_Senkrecht	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF
//	Button_Slider_Senkrecht	Standard_MouseOver_MousePressed	Rechteck_R2Ohne	-1	-1	-1	-1	Glossy		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_Slider_Senkrecht	Standard_Disabled	Ohne					Ohne					Ohne
//	Button_Slider_Senkrecht	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF
//	Button_SliderDesign	Standard	Rechteck_R4	0	0	0	0	Solide		0095DD			Solide_1px	FFFFFF
//	Button_SliderDesign	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_SliderDesign	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_SliderDesign	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	Button_SliderDesign	Standard_MouseOver	Rechteck_R4	0	0	0	0	Solide		2FBBFF			Solide_1px	FFFFFF
//	Button_SliderDesign	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_SliderDesign	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Solide		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF
//	Button_EckpunktSchieber	Standard	Rechteck_R4					Solide		FFB834			Solide_1px	0
//	Button_EckpunktSchieber	Checked_MousePressed	Rechteck_R4					Solide		FFE696			Solide_1px	C9C7BA
//	Button_EckpunktSchieber_Phantom	Standard	Rechteck_R4					Ohne					Solide_1px	0
//	TabStrip_Head	Standard	Rechteck_R2Ohne	0	0	0	15	Glossy		0095DD			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	TabStrip_Head	Checked	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFB834			Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	TabStrip_Head	Checked_Disabled	Rechteck_R2Ohne	0	0	0	8	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	TabStrip_Head	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFDA8C			Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	TabStrip_Head	Standard_Disabled	Rechteck_R2Ohne	0	0	0	15	Glossy		87B7CD			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	TabStrip_Head	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	15	Glossy		2FBBFF			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	RibbonBar_Head	Standard	Rechteck_R2Ohne	0	0	0	15	Glossy		BCDFED			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	RibbonBar_Head	Checked	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFE696			Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	RibbonBar_Head	Checked_Disabled	Rechteck_R2Ohne	0	0	0	8	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	F8DFB1			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	RibbonBar_Head	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFE696			Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	RibbonBar_Head	Standard_Disabled	Rechteck_R2Ohne	0	0	0	15	Glossy		ECE9D8			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	RibbonBar_Head	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	15	Glossy		BCDFED			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Color=ffffff}
//	Caption	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	CheckBox_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox
//	CheckBox_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}	CheckBox_Disabled_Checked
//	CheckBox_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}	CheckBox_Disabled
//	CheckBox_TextStyle	Checked_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_MouseOver
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox
//	CheckBox_TextStyle	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	CheckBox_MouseOver
//	OptionButton_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox
//	OptionButton_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}	OptionBox_Disabled_Checked
//	OptionButton_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}	OptionBox_Disabled
//	OptionButton_TextStyle	Checked_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox_MouseOver
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}	OptionBox_MouseOver
//	TextBox	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	TextBox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	TextBox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	TextBox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	TextBox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	ListBox	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	ListBox	Standard_Disabled	Rechteck_RRechteckRechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	ComboBox_Textbox	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	ComboBox_Textbox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	ComboBox_Textbox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	ComboBox_Textbox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Table_And_Pad	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	Table_And_Pad	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	Table_And_Pad	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	EasyPic	Standard_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	TextBox_Stufe3	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=12, Underline=True, Color=0046d5}
//	TextBox_Stufe3	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=12, Underline=True, Color=000000}
//	TextBox_Stufe3	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=12, Underline=True, Color=a6a6a6}
//	TextBox_Stufe3	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=12, Underline=True, Color=000000}
//	TextBox_Stufe3	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=12, Underline=True, Color=0046d5}
//	TextBox_Bold	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Bold=True, Color=0046d5}
//	TextBox_Bold	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Bold=True, Color=000000}
//	TextBox_Bold	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Bold=True, Color=a6a6a6}
//	TextBox_Bold	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Bold=True, Color=000000}
//	TextBox_Bold	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, CanvasSize=9, Bold=True, Color=0046d5}
//	Slider_Hintergrund_Waagerecht	Standard	Rechteck	5	5	0	0	Verlauf_Horizontal_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Waagerecht	Standard_MouseOver_MousePressed	Rechteck	5	5	0	0	Verlauf_Horizontal_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Waagerecht	Standard_Disabled	Rechteck	5	5	0	0	Solide		FFFFFF			Solide_1px	EEEDE5
//	Slider_Hintergrund_Waagerecht	Standard_MouseOver	Rechteck	5	5	0	0	Verlauf_Horizontal_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Waagerecht	Standard_MousePressed	Rechteck	5	5	0	0	Verlauf_Horizontal_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Senkrecht	Standard	Rechteck	0	0	5	5	Verlauf_Vertical_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver_MousePressed	Rechteck	0	0	5	5	Verlauf_Vertical_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	5	5	Verlauf_Vertical_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Senkrecht	Standard_Disabled	Rechteck	0	0	5	5	Solide		FFFFFF			Solide_1px	EEEDE5
//	Slider_Hintergrund_Senkrecht	Standard_MouseOver	Rechteck	0	0	5	5	Verlauf_Vertical_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Slider_Hintergrund_Senkrecht	Standard_MousePressed	Rechteck	0	0	5	5	Verlauf_Vertical_3	0,5	BCDFED	FEFEFB	BCDFED	Solide_1px	97AEE2
//	Ribbonbar_Button	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button	Standard_HasFocus_MousePressed	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	F7FAFB	E4EFF3	F7FAFB	Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Ohne	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Checked	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_Disabled	Rechteck_R2Ohne	-2	-2	-2	-2	Verlauf_Vertical_Solide	0,5	ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFE696			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Ohne	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Checked	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_Disabled	Rechteck_R2Ohne	-2	-2	-2	-2	Verlauf_Vertical_Solide	0,5	ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFE696			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Ohne	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Caption	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Ribbonbar_Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Ribbonbar_Button_Combobox	Standard	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_Disabled	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus	Ohne					Ohne					Ohne
//	RibbonBar_Frame	Standard	Rechteck	1	0	1	1	Ohne					Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	RibbonBar_Frame	Standard_Disabled	Rechteck	1	0	1	1	Ohne					Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Form_Standard	Standard	Rechteck	0	0	0	0	Solide		D7E7EE			Ohne
//	Form_MsgBox	Standard	Rechteck	0	0	0	0	Solide		D7E7EE			Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Form_QuickInfo	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Form_DesktopBenachrichtigung	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Form_BitteWarten	Standard	Rechteck_R2Ohne	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Form_AutoFilter	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Form_AutoFilter	Standard_Disabled															{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Form_KontextMenu	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB
//	Form_SelectBox_Dropdown	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_DropdownMenu	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_DropdownMenu	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_DropdownMenu	Checked_Disabled	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Item_DropdownMenu	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_DropdownMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Item_DropdownMenu	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_KontextMenu	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_KontextMenu	Standard_Disabled	Rechteck	0	0	0	0	Solide		F7FAFB			Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Item_KontextMenu	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_KontextMenu_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_KontextMenu_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	ECE9D8	F7FAFB		Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Item_KontextMenu_Caption	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Color=000000}
//	Item_Autofilter	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_Autofilter	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_Autofilter	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Autofilter	Checked_Disabled	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	C9C7BA			{Name=Comic Sans MS, CanvasSize=9, Color=dbdbdb}
//	Item_Autofilter	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Autofilter	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=dbdbdb}
//	Item_Autofilter	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Autofilter	Standard_HasFocus	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_Autofilter	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Listbox	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_Listbox	Standard_MouseOver_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Listbox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Listbox	Checked_Disabled	Rechteck_R4	0	0	0	0	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Item_Listbox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Listbox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Item_Listbox	Checked_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Listbox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=000000}
//	Item_Listbox	Standard_MousePressed	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_Listbox_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Item_Listbox_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	ECE9D8	F7FAFB		Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	Item_Listbox_Caption	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Frame	Standard	Rechteck_R2Ohne	-2	-2	-10	-2	Ohne					Solide_3px	FFB834			{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Frame	Standard_Disabled	Rechteck_R2Ohne	-1	-1	-10	-1	Ohne					Solide_1px	A6A6A6			{Name=Comic Sans MS, CanvasSize=9, Color=a6a6a6}
//	TabStrip_Body	Standard	Rechteck_RRechteckRechteck	-1	-1	-1	-1	Solide		E4EFF3			Solide_3px	FFB834	69A5BE	A2C7D7
//	TabStrip_Body	Standard_Disabled	Rechteck_RRechteckRechteck	-1	-1	-1	-1	Solide		E4EFF3			Solide_3px	498DAB	69A5BE	A2C7D7
//	RibbonBar_Body	Standard	Rechteck	0	0	0	0	Solide		E4EFF3			Solide_1px	FFB834
//	RibbonBar_Body	Standard_Disabled	Rechteck	0	0	0	0	Solide		E4EFF3			Solide_1px	C9C7BA
//	RibbonBar_Back	Standard	Rechteck	0	0	0	8	Solide		D7E7EE
//	TabStrip_Back	Standard	Ohne					Ohne					Ohne
//	Progressbar	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	Progressbar_Füller	Standard	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	498DAB
//	Table_Lines_thick	Standard	Ohne					Ohne					Ohne	0046D5
//	Table_Lines_thin	Standard	Ohne					Ohne					Ohne	97AEE2
//	Table_Cursor	Standard	Rechteck_RRechteckRechteck	-1	-1	-1	-1	Ohne					Solide_3px	F8DFB1
//	Table_Cursor	Standard_HasFocus	Rechteck_RRechteckRechteck	-1	-1	-1	-1	Ohne					Solide_3px	FFB834
//	Table_Cell	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Color=0046d5}
//	Table_Cell_New	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9, Italic=True, Color=0046d5}
//	Table_Column	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=9,4, Bold=True, Color=0046d5}
//	Table_Cell_Chapter	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, CanvasSize=13, Bold=True, Underline=True, Color=0046d5}

namespace BlueControls;

public static class Skin {

    #region Fields

    public const int Padding = 9;
    public const int PaddingSmal = 3;
    public static readonly float Scale = (float)Math.Round(GetDpiScale(), 2, MidpointRounding.AwayFromZero);
    public static Table? StyleTb;
    public static ColumnItem? StyleTb_Font;
    public static ColumnItem? StyleTb_Name;
    public static ColumnItem? StyleTb_Style;
    internal static Pen PenLinieDick = Pens.Red;
    internal static Pen PenLinieDünn = Pens.Red;
    internal static Pen PenLinieKräftig = Pens.Red;
    private static readonly Dictionary<Design, Dictionary<States, SkinDesign>> Design = [];
    private static readonly ImageCodeEffect[] St = new ImageCodeEffect[1];

    #endregion

    #region Properties

    public static bool Inited { get; private set; }

    public static Color RandomColor =>
        Color.FromArgb((byte)Constants.GlobalRnd.Next(0, 255),
            (byte)Constants.GlobalRnd.Next(0, 255),
            (byte)Constants.GlobalRnd.Next(0, 255));

    #endregion

    #region Methods

    public static ImageCodeEffect AdditionalState(States state) => state.HasFlag(States.Standard_Disabled) ? St[0] : ImageCodeEffect.Ohne;

    public static List<string>? AllStyles() {
        InitStyles();
        return StyleTb?.Column.First?.Contents();
    }

    public static void ChangeDesign(Design ds, States status, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, string bc1, string bc2, RahmenArt rahm, string boc1, string boc2, string f, string pic) {
        Design.Remove(ds, status);
        Design.Add(ds, status, f, enKontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic);
    }

    public static Color Color_Back(Design vDesign, States vState) => DesignOf(vDesign, vState).BackColor1;

    public static SkinDesign DesignOf(Design design, States state) {
        try {
            return Design[design][state];
        } catch {
            SkinDesign d = new() {
                BackColor1 = Color.White,
                BorderColor1 = Color.Red,
                Font = BlueFont.DefaultFont,
                HintergrundArt = HintergrundArt.Solide,
                RahmenArt = RahmenArt.Solide_1px,
                Kontur = Enums.Kontur.Rechteck
            };
            return d;
        }
    }

    public static void Draw_Back(Graphics gr, Design design, States state, Rectangle r, Control? control, bool needTransparenz) => Draw_Back(gr, DesignOf(design, state), r, control, needTransparenz);

    public static void Draw_Back(Graphics gr, SkinDesign design, Rectangle r, Control? control, bool needTransparenz) {
        try {
            if (design.Need) {
                if (!needTransparenz) { design.Need = false; }
                if (design.Kontur != Enums.Kontur.Ohne) {
                    if (design.HintergrundArt != HintergrundArt.Ohne) {
                        if (design.Kontur == Enums.Kontur.Rechteck && design is { X1: >= 0, X2: >= 0 } and { Y1: >= 0, Y2: >= 0 }) { design.Need = false; }
                        if (design.Kontur == Enums.Kontur.Rechteck_R4 && design is { X1: >= 1, X2: >= 1 } and { Y1: >= 1, Y2: >= 1 }) { design.Need = false; }
                    }
                }
            }
            if (design.Need) { Draw_Back_Transparent(gr, r, control); }
            if (design.HintergrundArt == HintergrundArt.Ohne || design.Kontur == Enums.Kontur.Ohne) { return; }
            r.X -= design.X1;
            r.Y -= design.Y1;
            r.Width += design.X1 + design.X2;
            r.Height += design.Y1 + design.Y2;
            if (r.Width < 1 || r.Height < 1) { return; }// Durchaus möglich, Creative-Pad, usereingabe
            switch (design.HintergrundArt) {
                case HintergrundArt.Ohne:
                    break;

                case HintergrundArt.Solide:
                    gr.FillPath(new SolidBrush(design.BackColor1), Kontur(design.Kontur, r));
                    break;

                case HintergrundArt.Verlauf_Vertical_2:
                    LinearGradientBrush lgb = new(r, design.BackColor1, design.BackColor2,
                        LinearGradientMode.Vertical);
                    gr.FillPath(lgb, Kontur(design.Kontur, r));
                    break;
                //case enHintergrundArt.Verlauf_Vertical_3:
                //    Draw_Back_Verlauf_Vertical_3(gr, row, r);
                //    break;
                //case enHintergrundArt.Verlauf_Horizontal_2:
                //    Draw_Back_Verlauf_Horizontal_2(gr, row, r);
                //    break;
                //case enHintergrundArt.Verlauf_Horizontal_3:
                //    Draw_Back_Verlauf_Horizontal_3(gr, row, r);
                //    break;
                //case enHintergrundArt.Verlauf_Diagonal_3:
                //    PathX = Kontur(Kon, r);
                //    var cx1 = Color.FromArgb(Value(row, col_Color_Back_1, 0));
                //    var cx2 = Color.FromArgb(Value(row, col_Color_Back_2, 0));
                //    var cx3 = Color.FromArgb(Value(row, col_Color_Back_3, 0));
                //    var PR = Value(row, col_Verlauf_Mitte, 0.7f);
                //    var lgb2 = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Right, r.Bottom), cx1, cx3);
                //    var cb = new ColorBlend {
                //        Colors = new[] { cx1, cx2, cx3 },
                //        Positions = new[] { 0.0F, PR, 1.0F }
                //    };
                //    lgb2.InterpolationColors = cb;
                //    lgb2.GammaCorrection = true;
                //    gr.FillPath(lgb2, PathX);
                //    break;
                //case enHintergrundArt.Glossy:
                //    Draw_Back_Glossy(gr, row, r);
                //    break;
                //case enHintergrundArt.GlossyPressed:
                //    Draw_Back_GlossyPressed(gr, row, r);
                //    break;
                //case enHintergrundArt.Verlauf_Vertikal_Glanzpunkt:
                //    Draw_Back_Verlauf_Vertical_Glanzpunkt(gr, row, r);
                //    break;
                case HintergrundArt.Unbekannt:
                    break;

                default:
                    Develop.DebugPrint(design.HintergrundArt);
                    break;
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Zeichnen des Skins:" + design, ex);
        }
    }

    public static void Draw_Back_Transparent(Graphics gr, Rectangle r, Control? control) {
        if (control?.Parent == null) { return; }
        switch (control.Parent) {
            case IBackgroundNone:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case GenericControl trb:
                if (trb.BitmapOfControl() == null) {
                    gr.FillRectangle(new SolidBrush(control.Parent.BackColor), r);
                    return;
                }
                gr.DrawImage(trb.BitmapOfControl(), r, r with { X = control.Left + r.Left, Y = control.Top + r.Top }, GraphicsUnit.Pixel);
                break;

            case Form frm:
                gr.FillRectangle(new SolidBrush(frm.BackColor), r);
                break;

            case SplitContainer:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case SplitterPanel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case TableLayoutPanel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case TabPage: // TabPage leitet sich von Panel ab!
                gr.FillRectangle(new SolidBrush(control.Parent.BackColor), r);
                break;

            case Panel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            default:
                gr.FillRectangle(new SolidBrush(control.Parent.BackColor), r);
                break;
        }
    }

    public static void Draw_Border(Graphics gr, Design vDesign, States vState, Rectangle r) => Draw_Border(gr, DesignOf(vDesign, vState), r);

    public static void Draw_Border(Graphics gr, SkinDesign design, Rectangle r) {
        if (design.Kontur == Enums.Kontur.Ohne || design.RahmenArt == RahmenArt.Ohne) { return; }

        if (design.Kontur == Enums.Kontur.Unbekannt) {
            design.Kontur = Enums.Kontur.Rechteck;
            r.Width--;
            r.Height--;
        } else {
            r.X -= design.X1;
            r.Y -= design.Y1;
            r.Width += design.X1 + design.X2 - 1;
            r.Height += design.Y1 + design.Y2 - 1;
        }
        if (r.Width < 1 || r.Height < 1) { return; }

        // PathX kann durch die ganzen Expand mal zu klein werden, dann wird nothing zurückgegeben
        try {
            Pen penX;
            GraphicsPath? pathX;
            switch (design.RahmenArt) {
                case RahmenArt.Solide_1px:
                    pathX = Kontur(design.Kontur, r);
                    penX = new Pen(design.BorderColor1);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case RahmenArt.Solide_1px_FocusDotLine:
                    pathX = Kontur(design.Kontur, r);
                    penX = new Pen(design.BorderColor1);
                    gr.DrawPath(penX, pathX);
                    r.Inflate(-3, -3);
                    pathX = Kontur(design.Kontur, r);
                    penX = new Pen(design.BorderColor2) {
                        DashStyle = DashStyle.Dot
                    };
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case RahmenArt.FocusDotLine:
                    penX = new Pen(design.BorderColor2) {
                        DashStyle = DashStyle.Dot
                    };
                    r.Inflate(-3, -3);
                    pathX = Kontur(design.Kontur, r);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case RahmenArt.Solide_3px:
                    pathX = Kontur(design.Kontur, r);
                    penX = new Pen(design.BorderColor1, 3);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case RahmenArt.Solide_21px:
                    pathX = Kontur(design.Kontur, r);
                    penX = new Pen(design.BorderColor1, 21);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                default:
                    pathX = Kontur(design.Kontur, r);
                    penX = new Pen(Color.Red);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    Develop.DebugPrint(design.RahmenArt);
                    break;
            }
        } catch {
            //Develop.DebugPrint("Fehler beim Zeichen des Randes " + design, ex);
        }
    }

    /// <summary>
    /// Bild wird in dieser Routine nicht mehr gändert, aber in der nachfolgenden
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="txt"></param>
    /// <param name="qi"></param>
    /// <param name="align"></param>
    /// <param name="fitInRect"></param>
    /// <param name="design"></param>
    /// <param name="state"></param>
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>
    /// <param name="translate"></param>
    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi,
        Alignment align,
        Rectangle fitInRect, Design design, States state,
        Control? child, bool deleteBack, bool translate) => Draw_FormatedText(gr, txt, qi, align, fitInRect, DesignOf(design, state), child, deleteBack, translate);

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, BlueFont? bFont, bool translate) => Draw_FormatedText(gr, txt, qi, align, fitInRect, null, false, bFont, translate);

    //private static void Draw_Border_DuoColor(Graphics GR, RowItem Row, Rectangle r, bool NurOben) {
    //    var c1 = Color.FromArgb(Value(Row, col_Color_Border_2, 0));
    //    var c2 = Color.FromArgb(Value(Row, col_Color_Border_3, 0));
    //    var lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Height), c1, c2) {
    //        GammaCorrection = true
    //    };
    //    var x = GR.SmoothingMode;
    //    GR.SmoothingMode = SmoothingMode.Default; //Returns the smoothing mode to default for a crisp structure
    //    GR.FillRectangle(lgb, r.Left, r.Top, r.Width + 1, 2); // Oben
    //    if (!NurOben) {
    //        GR.FillRectangle(lgb, r.Left, r.Bottom - 1, r.Width + 1, 2); // unten
    //        GR.FillRectangle(lgb, r.Left, r.Top, 2, r.Height + 1); // links
    //        GR.FillRectangle(lgb, r.Right - 1, r.Top, 2, r.Height + 1); // rechts
    //    }
    //    GR.SmoothingMode = x;
    //}
    /// <summary>
    /// Status des Bildes (Disabled) wird geändert
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="txt"></param>
    /// <param name="qi"></param>
    /// <param name="align"></param>
    /// <param name="fitInRect"></param>
    /// <param name="design"></param>
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>
    /// <param name="translate"></param>
    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align,
        Rectangle fitInRect, SkinDesign design,
        Control? child, bool deleteBack, bool translate) {
        if (string.IsNullOrEmpty(txt) && qi == null) { return; }
        QuickImage? tmpImage = null;
        if (qi != null) { tmpImage = QuickImage.Get(qi, AdditionalState(design.Status)); }
        Draw_FormatedText(gr, txt, tmpImage, align, fitInRect, child, deleteBack, design.Font, translate);
    }

    /// <summary>
    /// Zeichnet den Text und das Bild ohne weitere Modifikation
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="txt"></param>
    /// <param name="qi"></param>
    /// <param name="align"></param>
    /// <param name="fitInRect"></param>
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>
    /// <param name="bFont"></param>
    /// <param name="translate"></param>
    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, Control? child, bool deleteBack, BlueFont? bFont, bool translate) {
        var pSize = SizeF.Empty;
        var tSize = SizeF.Empty;
        float xp = 0;
        float yp1 = 0;
        float yp2 = 0;
        if (qi != null) {
            lock (qi) {
                pSize = ((Bitmap)qi).Size;
            }
        }
        if (LanguageTool.Translation != null) { txt = LanguageTool.DoTranslate(txt, translate); }
        if (bFont != null) {
            if (fitInRect.Width > 0) { txt = BlueFont.TrimByWidth(bFont, txt, fitInRect.Width - pSize.Width); }
            tSize = bFont.MeasureString(txt);
        }
        if (align.HasFlag(Alignment.Right)) { xp = fitInRect.Width - pSize.Width - tSize.Width; }
        if (align.HasFlag(Alignment.HorizontalCenter)) { xp = (float)((fitInRect.Width - pSize.Width - tSize.Width) / 2.0); }
        if (align.HasFlag(Alignment.VerticalCenter)) {
            yp1 = (float)((fitInRect.Height - pSize.Height) / 2.0);
            yp2 = (float)((fitInRect.Height - tSize.Height) / 2.0);
        }
        if (align.HasFlag(Alignment.Bottom)) {
            yp1 = fitInRect.Height - pSize.Height;
            yp2 = fitInRect.Height - tSize.Height;
        }
        if (deleteBack) {
            if (child != null) {
                if (!string.IsNullOrEmpty(txt)) { Draw_Back_Transparent(gr, new Rectangle((int)(fitInRect.X + pSize.Width + xp - 1), (int)(fitInRect.Y + yp2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2)), child); }
                if (qi != null) { Draw_Back_Transparent(gr, new Rectangle((int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1), (int)pSize.Width, (int)pSize.Height), child); }
            } else {
                var c = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
                if (!string.IsNullOrEmpty(txt)) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + pSize.Width + xp - 1), (int)(fitInRect.Y + yp2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2))); }
                if (qi != null) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1), (int)pSize.Width, (int)pSize.Height)); }
            }
        }
        try {
            if (qi != null) { gr.DrawImage(qi, (int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1)); }
            if (!string.IsNullOrEmpty(txt)) { bFont?.DrawString(gr, txt, fitInRect.X + pSize.Width + xp, fitInRect.Y + yp2); }
        } catch {
            // es kommt selten vor, dass das Graphics-Objekt an anderer Stelle verwendet wird.
            //Develop.DebugPrint(ex);
        }
    }

    public static BlueFont GetBlueFont(Design design, States state) => DesignOf(design, state).Font;

    public static BlueFont GetBlueFont(string style, PadStyles format) {
        if ((int)format > 100) { return BlueFont.DefaultFont; }
        if (format == PadStyles.Undefiniert || string.IsNullOrEmpty(style)) { return BlueFont.DefaultFont; }

        InitStyles();
        if (StyleTb is not { IsDisposed: false } tb ||
            StyleTb_Style is not { IsDisposed: false } cs ||
            StyleTb_Name is not { IsDisposed: false } cn ||
            StyleTb_Font is not { IsDisposed: false } cf) { return BlueFont.DefaultFont; }

        var f1 = new FilterItem(cn, BlueTable.Enums.FilterType.Istgleich_GroßKleinEgal, style);
        var f2 = new FilterItem(cs, BlueTable.Enums.FilterType.Istgleich, ((int)format).ToString());

        var r = tb.Row[f1, f2];

        if (r == null) {
            if (tb.IsEditable(false)) {
                var fc = new FilterItem[] { f1, f2 };
                tb.Row.GenerateAndAdd(fc, "Unbekannter Stil");
            }

            return BlueFont.DefaultFont;
        }

        var font = GetBlueFont(r);

        if (tb.IsEditable(false)) {
            r.CellSet(cf, font.ParseableItems().FinishParseable(), "Automatische Korrektur");
        }

        return font;
    }

    public static BlueFont GetBlueFont(RowItem? r) {
        if (r == null || StyleTb_Font is not { IsDisposed: false } cf) { return BlueFont.DefaultFont; }

        var s = r.CellGetString(cf);

        return BlueFont.Get(s);
    }

    public static List<AbstractListItem> GetFonts(string sheetStyle) {
        List<AbstractListItem> rahms =
        [
            ItemOf("Haupt-Überschrift", ((int)PadStyles.Überschrift).ToString(),
                GetBlueFont(sheetStyle, PadStyles.Überschrift).SymbolForReadableText()),
            ItemOf("Untertitel für Haupt-Überschrift", ((int)PadStyles.Untertitel).ToString(),
                GetBlueFont(sheetStyle, PadStyles.Untertitel).SymbolForReadableText()),
            ItemOf("Überschrift für Kapitel", ((int)PadStyles.Kapitel).ToString(),
                GetBlueFont(sheetStyle, PadStyles.Kapitel).SymbolForReadableText()),
            ItemOf("Standard", ((int)PadStyles.Standard).ToString(),
                GetBlueFont(sheetStyle, PadStyles.Standard).SymbolForReadableText()),
            ItemOf("Standard Fett", ((int)PadStyles.Hervorgehoben).ToString(),
                GetBlueFont(sheetStyle, PadStyles.Hervorgehoben).SymbolForReadableText()),
            ItemOf("Standard Alternativ-Design", ((int)PadStyles.Alternativ).ToString(),
                GetBlueFont(sheetStyle, PadStyles.Alternativ).SymbolForReadableText()),
            ItemOf("Kleiner Zusatz", ((int)PadStyles.Kleiner_Zusatz).ToString(),
                GetBlueFont(sheetStyle, PadStyles.Kleiner_Zusatz).SymbolForReadableText())
        ];
        //rahms.Sort();
        return rahms;
    }

    public static List<AbstractListItem> GetRahmenArt(string sheetStyle, bool mitOhne) {
        var rahms = new List<AbstractListItem>();
        if (mitOhne) {
            rahms.Add(ItemOf("Ohne Rahmen", ((int)PadStyles.Undefiniert).ToString(), ImageCode.Kreuz));
        }
        rahms.Add(ItemOf("Haupt-Überschrift", ((int)PadStyles.Überschrift).ToString(), GetBlueFont(sheetStyle, PadStyles.Überschrift).SymbolOfLine()));
        rahms.Add(ItemOf("Untertitel für Haupt-Überschrift", ((int)PadStyles.Untertitel).ToString(), GetBlueFont(sheetStyle, PadStyles.Untertitel).SymbolOfLine()));
        rahms.Add(ItemOf("Überschrift für Kapitel", ((int)PadStyles.Kapitel).ToString(), GetBlueFont(sheetStyle, PadStyles.Kapitel).SymbolOfLine()));
        rahms.Add(ItemOf("Standard", ((int)PadStyles.Standard).ToString(), GetBlueFont(sheetStyle, PadStyles.Standard).SymbolOfLine()));
        rahms.Add(ItemOf("Standard Fett", ((int)PadStyles.Hervorgehoben).ToString(), GetBlueFont(sheetStyle, PadStyles.Hervorgehoben).SymbolOfLine()));
        rahms.Add(ItemOf("Standard Alternativ-Design", ((int)PadStyles.Alternativ).ToString(), GetBlueFont(sheetStyle, PadStyles.Alternativ).SymbolOfLine()));
        rahms.Add(ItemOf("Kleiner Zusatz", ((int)PadStyles.Kleiner_Zusatz).ToString(), GetBlueFont(sheetStyle, PadStyles.Kleiner_Zusatz).SymbolOfLine()));
        //rahms.Sort();
        return rahms;
    }

    public static Color IdColor(List<int>? id) => id is not { Count: not 0 } ? IdColor(-1) : IdColor(id[0]);

    public static Color IdColor(int id) {
        if (id < 0) { return Color.White; }

        switch (id % 10) {
            case 0:
                return Color.Red;

            case 1:
                return Color.Blue;

            case 2:
                return Color.Green;

            case 3:
                return Color.Yellow;

            case 4:
                return Color.Purple;

            case 5:
                return Color.Cyan;

            case 6:
                return Color.Orange;

            case 7:
                return Color.LightBlue;

            case 8:
                return Color.PaleVioletRed;

            case 9:
                return Color.LightGreen;

            default:
                return Color.Gray;
        }
    }

    public static void InitStyles() {
        StyleTb ??= Table.LoadResource(Assembly.GetAssembly(typeof(Skin)), "Styles.BDB", "Styles", true, false);

        StyleTb_Name = StyleTb?.Column["Name"];
        StyleTb_Style = StyleTb?.Column["Style"];
        StyleTb_Font = StyleTb?.Column["Font"];
    }

    // Der Abstand von z.B. in Textboxen: Text Linke Koordinate
    public static void LoadSkin() {
        //_SkinString = "Windows10";
        Design.Add(Enums.Design.Button, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_Disabled, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "DFDFDF", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_3px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "DFDFDF", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_3px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_AutoFilter, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_AutoFilter, States.Checked, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "FF0000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_AutoFilter, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard, string.Empty, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_HasFocus_MousePressed, string.Empty, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_MouseOver_HasFocus_MousePressed, string.Empty, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EAEAEA", string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_HasFocus, string.Empty, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_MouseOver_HasFocus, string.Empty, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);

        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CDCDCD", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard_MouseOver_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CECECE", string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard_Disabled, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CECECE", string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CDCDCD", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard_MouseOver_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CECECE", string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard_Disabled, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CECECE", string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty);

        Design.Add(Enums.Design.Button_SliderDesign, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_MouseOver_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F9F9F9", string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Ohne, "B6B6B6", string.Empty, string.Empty);

        Design.Add(Enums.Design.Button_EckpunktSchieber, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_EckpunktSchieber, States.Checked_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_EckpunktSchieber_Phantom, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_EckpunktSchieber_Joint, States.Standard, "Windows 11 Blue|5|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFD800", string.Empty, RahmenArt.Solide_1px, "FF6A00", string.Empty, string.Empty);

        Design.Add(Enums.Design.TabStrip_Head, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, -2, 0, 5, HintergrundArt.Verlauf_Vertical_2, "F0F0F0", "E4E4E4", RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Checked, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "DFDFDF", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Checked_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, -2, 0, 5, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, -2, 0, 5, HintergrundArt.Verlauf_Vertical_2, "F0F0F0", "E4E4E4", RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Checked, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "F4F5F6", string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "F4F5F6", string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Checked_MouseOver, "Windows 11 Blue|0|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "F4F5F6", string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Standard_MouseOver, "Windows 11 Blue|0|0", Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Caption, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Caption, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "CheckBox");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "CheckBox_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "CheckBox_Disabled_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_MouseOver, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "CheckBox_Checked_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "CheckBox_Disabled");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "CheckBox_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "CheckBox_Checked_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "CheckBox_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "CheckBox_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "CheckBox_Checked_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "CheckBox");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "CheckBox_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "OptionBox");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "OptionBox_Checked");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "OptionBox_Disabled_Checked");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_MouseOver, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "OptionBox_Checked_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "OptionBox_Disabled");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "OptionBox_Checked");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "OptionBox_Checked_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "OptionBox");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, "OptionBox_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "OptionBox_Checked_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "OptionBox");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, "000000", "OptionBox_MouseOver");
        Design.Add(Enums.Design.TextBox, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.TextBox, States.Checked, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.TextBox, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.TextBox, States.Checked_HasFocus, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.TextBox, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.ListBox, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.ListBox, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_MouseOver, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_HasFocus, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_MouseOver_HasFocus, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_HasFocus_MousePressed, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_And_Pad, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_And_Pad, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_And_Pad, States.Standard_MouseOver_HasFocus, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_And_Pad, States.Standard_MouseOver_HasFocus_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_And_Pad, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_And_Pad, States.Standard_HasFocus, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.EasyPic, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.EasyPic, States.Standard_HasFocus_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.EasyPic, States.Standard_MouseOver_HasFocus_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.EasyPic, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.EasyPic, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.EasyPic, States.Standard_HasFocus, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_MouseOver_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F9F9F9", string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MouseOver_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MouseOver_HasFocus_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F9F9F9", string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MouseOver, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MousePressed, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "EFEFEF", string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "DFDFDF", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "DFDFDF", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_MouseOver_HasFocus_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_MouseOver, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_HasFocus, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_MouseOver_HasFocus, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_HasFocus_MousePressed, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_MouseOver, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_MouseOver_HasFocus_MousePressed, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_MouseOver_HasFocus, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Caption, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Caption, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_HasFocus_MousePressed, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_MouseOver_HasFocus_MousePressed, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_Disabled, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_MouseOver, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_HasFocus, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_MouseOver_HasFocus, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Frame, States.Standard, "Windows 11 Italic|0|0", Enums.Kontur.Rechteck, 1, 1, 0, 1, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Frame, States.Standard_Disabled, "Windows 11 Italic Disabled|0|1", Enums.Kontur.Rechteck, 1, 1, 0, 1, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_Standard, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_MsgBox, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_QuickInfo, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_DesktopBenachrichtigung, States.Standard, "Windows 11|6", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Solide_3px, "5D5D5D", string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_BitteWarten, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_AutoFilter, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_AutoFilter, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_KontextMenu, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F0F0F0", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_SelectBox_Dropdown, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Checked, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Checked_Disabled, "Windows 11 Checked Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "5D5D5D", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Checked_MouseOver, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Standard_MouseOver, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu, States.Standard_MouseOver, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu_Caption, States.Standard, "Windows 11|7", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu_Caption, States.Standard_Disabled, "Windows 11 Disabled|7", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu_Caption, States.Standard_MouseOver, "Windows 11|7", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_MouseOver_HasFocus_MousePressed, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Checked, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Checked_Disabled, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "5D5D5D", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Checked_MouseOver, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_MouseOver, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_HasFocus, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_MouseOver_HasFocus, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard, "Windows 11|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_MouseOver_MousePressed, "Windows 11 MouseOver|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CCE8FF", string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CCE8FF", string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "DFDFDF", string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked_MouseOver, "Windows 11 MouseOver|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CCE8FF", string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CCE8FF", string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_MouseOver, "Windows 11 MouseOver|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "E5F3FF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_MousePressed, "Windows 11|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "CCE8FF", string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox_Caption, States.Standard, "Windows 11|7", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox_Caption, States.Standard_Disabled, "Windows 11 Checked|0", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "DFDFDF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Item_Listbox_Caption, States.Standard_MouseOver, "Windows 11|7", Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "BFDFFF", string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty);
        Design.Add(Enums.Design.GroupBox, States.Standard, "Windows 11|0", Enums.Kontur.Rechteck, 0, -7, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.GroupBox, States.Standard_Disabled, "Windows 11 Disabled|0", Enums.Kontur.Rechteck, 0, -7, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.GroupBoxBold, States.Standard, "Windows 11 Checked|7", Enums.Kontur.Rechteck, 9, -11, 9, 9, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_21px, "40568D", string.Empty, string.Empty);
        Design.Add(Enums.Design.GroupBoxBold, States.Standard_Disabled, "Windows 11 Disabled|7", Enums.Kontur.Rechteck, 9, -11, 9, 9, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_21px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Body, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Body, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Body, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F4F5F6", string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Body, States.Standard_Disabled, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "F4F5F6", string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Back, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Back, States.Standard, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Progressbar, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "FFFFFF", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty);
        Design.Add(Enums.Design.Progressbar_Füller, States.Standard, string.Empty, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, "0072BC", string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_Lines_thick, States.Standard, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_Lines_thin, States.Standard, string.Empty, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_Cursor, States.Standard, string.Empty, Enums.Kontur.Rechteck, -1, -1, -1, -1, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_3px, "ACACAC", string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_Cursor, States.Standard_HasFocus, string.Empty, Enums.Kontur.Rechteck, -1, -1, -1, -1, HintergrundArt.Ohne, string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty);
        Inited = true;

        St[0] = ImageCodeEffect.WindowsXPDisabled;

        PenLinieDünn = new Pen(Color_Border(Enums.Design.Table_Lines_thin, States.Standard));
        PenLinieKräftig = new Pen(Color_Border(Enums.Design.Table_Lines_thick, States.Standard));
        PenLinieDick = new Pen(Color_Border(Enums.Design.Table_Lines_thick, States.Standard), 3);
    }

    public static PadStyles RepairStyle(PadStyles style) {
        switch ((int)style) {
            case < 100:
                return style;

            case 10001:
                return PadStyles.Überschrift;

            case 10002:
                return PadStyles.Untertitel;

            case 10003:
                return PadStyles.Kapitel;

            case 10004:
                return PadStyles.Standard;

            case 10005:
                return PadStyles.Kleiner_Zusatz;

            case 10006:
                return PadStyles.Alternativ;

            case 10007:
                return PadStyles.Hervorgehoben;

            default:
                return PadStyles.Standard;
        }
    }

    internal static Color Color_Border(Design design, States state) => DesignOf(design, state).BorderColor1;

    private static double GetDpiScale() {
        using var g = Graphics.FromHwnd(IntPtr.Zero);
        return g.DpiX / 96.0; // 96 DPI = 100% Skalierung
    }

    //internal static BlueFont? GetBlueFont(PadStyles padStyle, string sheetStyle, int stufe) {
    //    switch (stufe) {
    //        case 4:
    //            return GetBlueFont(sheetStyle, padStyle);

    //        case 3:
    //            switch (padStyle) {
    //                case PadStyles.Standard:
    //                    return GetBlueFont(sheetStyle, PadStyles.Kapitel);

    //                case PadStyles.Hervorgehoben:
    //                    return GetBlueFont(sheetStyle, PadStyles.Kapitel);
    //            }
    //            break;

    //        case 2:
    //            switch (padStyle) {
    //                case PadStyles.Standard:
    //                    return GetBlueFont(sheetStyle, PadStyles.Untertitel);

    //                case PadStyles.Hervorgehoben:
    //                    return GetBlueFont(sheetStyle, PadStyles.Untertitel);
    //            }
    //            break;

    //        case 1:
    //            switch (padStyle) {
    //                case PadStyles.Standard:
    //                    return GetBlueFont(sheetStyle, PadStyles.Überschrift);

    //                case PadStyles.Hervorgehoben:
    //                    return GetBlueFont(sheetStyle, PadStyles.Überschrift);
    //            }
    //            break;

    //        case 7:
    //            switch (padStyle) {
    //                case PadStyles.Standard:
    //                    return GetBlueFont(sheetStyle, PadStyles.Hervorgehoben);

    //                case PadStyles.Hervorgehoben:
    //                    return GetBlueFont(sheetStyle, PadStyles.Standard);
    //            }
    //            break;
    //    }
    //    Develop.DebugPrint(ErrorType.Error, "Stufe " + stufe + " nicht definiert.");
    //    return null;
    //}

    //internal static BlueFont GetBlueFont(Design design, States state, int stufe) {
    //    if (stufe != 4 && design != Enums.Design.TextBox) {
    //        if (design == Enums.Design.Form_QuickInfo) { return GetBlueFont(design, state); } // QuickInfo kann jeden Text enthatlten
    //        Develop.DebugPrint(ErrorType.Warning, "Design unbekannt: " + (int)design);
    //        return GetBlueFont(design, state);
    //    }
    //    switch (stufe) {
    //        case 4:
    //            return GetBlueFont(design, state);

    //        case 3:
    //            return GetBlueFont(Enums.Design.TextBox_Stufe3, state);

    //        case 2:
    //            return GetBlueFont(Enums.Design.TextBox_Stufe3, state);

    //        case 1:
    //            return GetBlueFont(Enums.Design.TextBox_Stufe3, state);

    //        case 7:
    //            return GetBlueFont(Enums.Design.TextBox_Bold, state);
    //    }

    //    Develop.DebugPrint(ErrorType.Error, "Stufe " + stufe + " nicht definiert.");
    //    return GetBlueFont(design, state);
    //}
    private static GraphicsPath? Kontur(Kontur kon, Rectangle r) {
        switch (kon) {
            case Enums.Kontur.Rechteck:
                return Poly_Rechteck(r);

            case Enums.Kontur.Rechteck_R4:
                return Poly_RoundRec(r, 4);

            case Enums.Kontur.Ohne:
                return null;

            default:
                return Poly_Rechteck(r);
        }
    }

    #endregion
}