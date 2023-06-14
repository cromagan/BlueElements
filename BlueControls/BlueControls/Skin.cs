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

#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using static BlueBasics.Polygons;
using Size = System.Drawing.Size;

//  = A3 & ".Design.GenerateAndAdd(enStates."&B3&", enKontur."& C3 & ", " &D3&", "&E3&", "&F3&","&G3&", enHintergrundArt."&H3&","&I3&",'"&J3&"','"&K3&"','"&L3&"',enRahmenArt."&M3&",'"&N3&"','"&O3&"','"&P3&"','"&Q3&"','"&R3&"');"

//	Control	Status	Kontur	X1	X2	Y1	Y2	Draw Back	Verlauf Mitte	Color Back 1	Color Back 2	Color Back 3	Border Style	Color Border 1	Color Border 2	Color Border 3	Schrift	StandardPic
//	Button	Standard	Rechteck					Solide		EAEAEA			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	Button	Standard_HasFocus_MousePressed	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		EAEAEA			Solide_3px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button	Standard_Disabled	Rechteck					Solide		EFEFEF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Button	Standard_MouseOver	Rechteck					Solide		EAEAEA			Solide_3px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	Button	Standard_HasFocus	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button	Standard_MouseOver_HasFocus	Rechteck					Solide		EAEAEA			Solide_3px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Standard	Rechteck					Solide		EAEAEA			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Button_CheckBox	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_3px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Standard_Disabled	Rechteck					Solide		EFEFEF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Button_CheckBox	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Standard_MouseOver	Rechteck					Solide		EAEAEA			Solide_3px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Standard_HasFocus	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck					Solide		EAEAEA			Solide_3px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Standard	Rechteck					Solide		EAEAEA			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Button_OptionButton	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_3px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Standard_Disabled	Rechteck					Solide		EFEFEF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Button_OptionButton	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Standard_MouseOver	Rechteck					Solide		EAEAEA			Solide_3px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_3px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Standard_HasFocus	Rechteck					Solide		EAEAEA			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Button_OptionButton	Standard_MouseOver_HasFocus															{Name=Calibri, Size=10[K]15}
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
//	TabStrip_Head	Standard	Rechteck			-2	5	Verlauf_Vertical_Solide	0,5	F0F0F0	E4E4E4		Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	TabStrip_Head	Checked	Rechteck				5	Solide		FFFFFF			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	TabStrip_Head	Checked_Disabled	Rechteck				5	Solide		DFDFDF			Solide_1px	B7B7B7			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	TabStrip_Head	Checked_MouseOver	Rechteck				5	Solide		FFFFFF			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	TabStrip_Head	Standard_Disabled	Rechteck			-2	5	Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	TabStrip_Head	Standard_MouseOver	Rechteck			-2	5	Verlauf_Vertical_Solide	0,5	F0F0F0	E4E4E4		Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	RibbonBar_Head	Standard	Rechteck				5	Solide		FFFFFF			Ohne				{Name=Calibri, Size=10[K]15}
//	RibbonBar_Head	Checked	Rechteck				5	Solide		F4F5F6			Solide_1px	E5E4E5			{Name=Calibri, Size=10[K]15}
//	RibbonBar_Head	Checked_Disabled	Rechteck				5	Solide		F4F5F6			Solide_1px	E5E4E5			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	RibbonBar_Head	Checked_MouseOver	Rechteck				5	Solide		F4F5F6			Solide_1px	E5E4E5			{Name=Calibri, Size=10[K]15, Color=0000ff}
//	RibbonBar_Head	Standard_Disabled	Rechteck				5	Solide		FFFFFF			Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	RibbonBar_Head	Standard_MouseOver	Rechteck				5	Solide		FFFFFF			Ohne				{Name=Calibri, Size=10[K]15, Color=0000ff}
//	Caption	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	CheckBox_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	CheckBox
//	CheckBox_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}	CheckBox_Disabled_Checked
//	CheckBox_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}	CheckBox_Disabled
//	CheckBox_TextStyle	Checked_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Checked_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	CheckBox_Checked
//	CheckBox_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	CheckBox_MouseOver
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	CheckBox
//	CheckBox_TextStyle	Standard_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	CheckBox_MouseOver
//	OptionButton_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	OptionBox
//	OptionButton_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}	OptionBox_Disabled_Checked
//	OptionButton_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}	OptionBox_Disabled
//	OptionButton_TextStyle	Checked_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Checked_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}	OptionBox_MouseOver
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver_HasFocus	Rechteck					Ohne					FocusDotLine			0	{Name=Calibri, Size=10[K]15}	OptionBox_MouseOver
//	TextBox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	TextBox	Checked	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	TextBox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	TextBox	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	TextBox	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	ListBox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	ListBox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8
//	ComboBox_Textbox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	ComboBox_Textbox	Checked	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	ComboBox_Textbox	Checked_MouseOver	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	ComboBox_Textbox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	ComboBox_Textbox	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	ComboBox_Textbox	Standard_MouseOver	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	ComboBox_Textbox	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Table_And_Pad	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	Table_And_Pad	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8
//	Table_And_Pad	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	EasyPic	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	EasyPic	Standard_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	EasyPic	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	EasyPic	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8
//	EasyPic	Standard_MouseOver	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6
//	EasyPic	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF
//	TextBox_Stufe3	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			{Name=Calibri, Size=12, Bold=True, Underline=True}
//	TextBox_Stufe3	Checked	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=12, Bold=True, Underline=True, Color=ffffff}
//	TextBox_Stufe3	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=12, Bold=True, Underline=True, Color=c0c0c0}
//	TextBox_Stufe3	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=12, Bold=True, Underline=True, Color=ffffff}
//	TextBox_Stufe3	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=12, Bold=True, Underline=True}
//	TextBox_Bold	Standard	Rechteck					Solide		FFFFFF			Solide_1px	B6B6B6			{Name=Calibri, Size=10[K]15, Bold=True}
//	TextBox_Bold	Checked	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Bold=True, Color=ffffff}
//	TextBox_Bold	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Bold=True, Color=9d9d9d}
//	TextBox_Bold	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Bold=True, Color=ffffff}
//	TextBox_Bold	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15, Bold=True}
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
//	Ribbonbar_Button	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button	Standard_HasFocus_MousePressed	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Ohne					Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Ribbonbar_Button	Standard_MouseOver	Rechteck					Ohne					Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button	Standard_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver	Rechteck					Ohne					Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Standard_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Checked	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	81B8EF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver	Rechteck					Ohne					Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		BFDFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Standard_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck					Ohne					Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbon_ComboBox_Textbox	Standard	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbon_ComboBox_Textbox	Checked	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver	Rechteck					Solide		0072BC			Solide_1px	0			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_Disabled	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck					Solide		0072BC			Solide_1px	0			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver	Rechteck					Solide		FFFFFF			Solide_1px	D8D8D8			{Name=Calibri, Size=10[K]15}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck					Solide		0072BC			Solide_1px	0			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck					Solide		FFFFFF			Solide_1px	3399FF			{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Caption	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Ribbonbar_Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Ribbonbar_Button_Combobox	Standard	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_Disabled	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus	Ohne					Ohne					Ohne
//	RibbonBar_Frame	Standard	Rechteck	1	0	1	1	Ohne					Solide_1px	ACACAC			{Name=Calibri, Size=10[K]15, Italic=True}
//	RibbonBar_Frame	Standard_Disabled	Rechteck	1	0	1	1	Ohne					Solide_1px	ACACAC			{Name=Calibri, Size=10[K]15, Italic=True, Color=9d9d9d}
//	Form_Standard	Standard	Rechteck					Solide		F0F0F0			Ohne
//	Form_MsgBox	Standard	Rechteck					Solide		F0F0F0			Ohne				{Name=Calibri, Size=10[K]15}
//	Form_QuickInfo	Standard	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			{Name=Calibri, Size=10[K]15}
//	Form_DesktopBenachrichtigung	Standard	Rechteck					Solide		1F1F1F			Solide_3px	484848			{Name=Calibri, Size=12[K]5, Color=ffffff}
//	Form_BitteWarten	Standard	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			{Name=Calibri, Size=10[K]15}
//	Form_AutoFilter	Standard	Rechteck					Solide		FFFFFF			Solide_1px	0			{Name=Calibri, Size=10[K]15}
//	Form_AutoFilter	Standard_Disabled															{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Form_KontextMenu	Standard	Rechteck					Solide		F0F0F0			Solide_1px	0
//	Form_SelectBox_Dropdown	Standard	Rechteck					Solide		FFFFFF			Solide_1px	0			{Name=Calibri, Size=10[K]15}
//	Item_DropdownMenu	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Item_DropdownMenu	Checked	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_DropdownMenu	Checked_Disabled	Rechteck					Solide		5D5D5D			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_DropdownMenu	Checked_MouseOver	Rechteck					Solide		0072BC			Solide_1px	0			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_DropdownMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Item_DropdownMenu	Standard_MouseOver	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_KontextMenu	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Item_KontextMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Item_KontextMenu	Standard_MouseOver	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_KontextMenu_Caption	Standard	Rechteck					Solide		BFDFFF			Ohne				{Name=Calibri, Size=10[K]15, Bold=True}
//	Item_KontextMenu_Caption	Standard_Disabled	Rechteck					Solide		BFDFFF			Ohne				{Name=Calibri, Size=10[K]15, Bold=True}
//	Item_KontextMenu_Caption	Standard_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			{Name=Calibri, Size=10[K]15, Bold=True}
//	Item_Autofilter	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Item_Autofilter	Standard_MouseOver_HasFocus_MousePressed	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_Autofilter	Checked	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_Autofilter	Checked_Disabled	Rechteck					Solide		5D5D5D			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_Autofilter	Checked_MouseOver	Rechteck					Solide		0072BC			Solide_1px	0			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_Autofilter	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Item_Autofilter	Standard_MouseOver	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_Autofilter	Standard_HasFocus	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Item_Autofilter	Standard_MouseOver_HasFocus	Rechteck					Solide		0072BC			Ohne				{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_Listbox	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Item_Listbox	Standard_MouseOver_MousePressed	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			{Name=Calibri, Size=10[K]15, Underline=True}
//	Item_Listbox	Checked	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			{Name=Calibri, Size=10[K]15}
//	Item_Listbox	Checked_Disabled	Rechteck					Solide		DFDFDF			Solide_1px	B7B7B7			{Name=Calibri, Size=10[K]15, Color=ffffff}
//	Item_Listbox	Checked_MouseOver	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			{Name=Calibri, Size=10[K]15, Underline=True}
//	Item_Listbox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Color=9d9d9d}
//	Item_Listbox	Checked_MousePressed	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			{Name=Calibri, Size=10[K]15}
//	Item_Listbox	Standard_MouseOver	Rechteck					Solide		E5F3FF			Ohne				{Name=Calibri, Size=10[K]15, Underline=True}
//	Item_Listbox	Standard_MousePressed	Rechteck					Solide		CCE8FF			Solide_1px	99D1FF			{Name=Calibri, Size=10[K]15}
//	Item_Listbox_Caption	Standard	Rechteck					Solide		BFDFFF			Ohne				{Name=Calibri, Size=10[K]15, Bold=True}
//	Item_Listbox_Caption	Standard_Disabled	Rechteck					Solide		DFDFDF			Ohne				{Name=Calibri, Size=10[K]15, Bold=True, Color=ffffff}
//	Item_Listbox_Caption	Standard_MouseOver	Rechteck					Solide		BFDFFF			Solide_1px	4DA1B5			{Name=Calibri, Size=10[K]15, Bold=True}
//	Frame	Standard	Rechteck			-7		Ohne					Solide_1px	ACACAC			{Name=Calibri, Size=10[K]15}
//	Frame	Standard_Disabled	Rechteck			-7		Ohne					Solide_1px	ACACAC			{Name=Calibri, Size=10[K]15, Color=9d9d9d}
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
//	Table_Cell	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15}
//	Table_Cell_New	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Italic=True}
//	Table_Column	Standard	Ohne					Ohne					Ohne				{Name=Calibri, Size=10[K]15, Bold=True}
//	Table_Cell_Chapter	Standard															{Name=Calibri, Size=15, Bold=True, Underline=True}

//	Control	Status	Kontur	X1	X2	Y1	Y2	Draw Back	Verlauf Mitte	Color Back 1	Color Back 2	Color Back 3	Border Style	Color Border 1	Color Border 2	Color Border 3	Schrift	StandardPic
//	Button	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	003C74			{Name=Arial, Size=9, Color=000000}
//	Button	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Button	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button	Standard_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, Size=9, Color=000000}
//	Button	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	003C74			{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Checked	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	003C74			{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Checked_Disabled	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Button_CheckBox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, Size=9, Color=000000}
//	Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Standard	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	003C74			{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Checked	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	003C74			{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Checked_Disabled	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Button_OptionButton	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Button_OptionButton	Checked_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Standard_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	C2DBFF	8CB4F2	{Name=Arial, Size=9, Color=000000}
//	Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
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
//	TabStrip_Head	Standard	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	003C74			{Name=Arial, Size=9, Color=000000}
//	TabStrip_Head	Checked	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	919B9C			{Name=Arial, Size=9, Color=000000}
//	TabStrip_Head	Checked_Disabled	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	TabStrip_Head	Checked_MouseOver	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px_DuoColor_NurOben	919B9C	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	TabStrip_Head	Standard_Disabled	Rechteck_R4	0	0	0	5	Solide		F5F4EA			Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	TabStrip_Head	Standard_MouseOver	Rechteck_R4	0	0	0	5	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px_DuoColor_NurOben	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	RibbonBar_Head	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	RibbonBar_Head	Checked	Rechteck	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	919B9C			{Name=Arial, Size=9, Color=000000}
//	RibbonBar_Head	Checked_Disabled	Rechteck	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	RibbonBar_Head	Checked_MouseOver	Rechteck	0	0	0	5	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	F4F3EE	Solide_1px	919B9C			{Name=Arial, Size=9, Color=0000ff}
//	RibbonBar_Head	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	RibbonBar_Head	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=0000ff}
//	Caption	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	CheckBox_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	CheckBox
//	CheckBox_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}	CheckBox_Disabled_Checked
//	CheckBox_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}	CheckBox_Disabled
//	CheckBox_TextStyle	Checked_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	CheckBox_Checked
//	CheckBox_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	CheckBox_MouseOver
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	CheckBox
//	CheckBox_TextStyle	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	CheckBox_MouseOver
//	OptionButton_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	OptionBox
//	OptionButton_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}	OptionBox_Disabled_Checked
//	OptionButton_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}	OptionBox_Disabled
//	OptionButton_TextStyle	Checked_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}	OptionBox_MouseOver
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}	OptionBox_MouseOver
//	TextBox	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	TextBox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	TextBox	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	TextBox	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	TextBox	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	ListBox	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	ListBox	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	ComboBox_Textbox	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, Size=9, Color=000000}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, Size=9, Color=000000}
//	ComboBox_Textbox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	ComboBox_Textbox	Checked_MouseOver	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	ComboBox_Textbox	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	ComboBox_Textbox	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	ComboBox_Textbox	Standard_MouseOver	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	ComboBox_Textbox	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, Size=9, Color=000000}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px_FocusDotLine	7F9DB9		0	{Name=Arial, Size=9, Color=000000}
//	Table_And_Pad	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	Table_And_Pad	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	Table_And_Pad	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	003C74
//	EasyPic	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	EasyPic	Standard_MouseOver	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	TextBox_Stufe3	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=12, Bold=True, Underline=True, Color=000000}
//	TextBox_Stufe3	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=12, Bold=True, Underline=True, Color=ffffff}
//	TextBox_Stufe3	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, Size=12, Bold=True, Underline=True, Color=a6a6a6}
//	TextBox_Stufe3	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=12, Bold=True, Underline=True, Color=ffffff}
//	TextBox_Stufe3	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=12, Bold=True, Underline=True, Color=000000}
//	TextBox_Bold	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=9, Bold=True, Color=000000}
//	TextBox_Bold	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Bold=True, Color=ffffff}
//	TextBox_Bold	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Arial, Size=9, Bold=True, Color=a6a6a6}
//	TextBox_Bold	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Bold=True, Color=ffffff}
//	TextBox_Bold	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Arial, Size=9, Bold=True, Color=000000}
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
//	Ribbonbar_Button	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Ribbonbar_Button	Standard_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button	Standard_HasFocus	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_Disabled	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	BDBAA2	ECE9D8	FFFFFF	Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_HasFocus	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_Disabled	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	BDBAA2	ECE9D8	FFFFFF	Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	DCD7CB	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	D1CCC1	E3E2DA	EFEEEA	Solide_1px_DuoColor	003C74	FFDA8C	FFB834	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_HasFocus	Rechteck_R4	0	0	-4	-4	Ohne					FocusDotLine			404040	{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	-4	-4	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px_FocusDotLine	0		404040	{Name=Arial, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	-3	-3	-3	Verlauf_Vertical_3	0,5	FFB834	FFDA8C	FFE696	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_Disabled	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver	Rechteck_R4	0	-3	-3	-3	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus	Rechteck_R4	0	-3	-3	-3	Ohne					Solide_1px	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck_R4	0	-3	-3	-3	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Caption	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Ribbonbar_Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_Combobox	Standard	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_Disabled	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Ohne	7F9DB9
//	RibbonBar_Frame	Standard	Rechteck	1	0	1	1	Ohne					Solide_1px	0			{Name=Arial, Size=9, Color=0000ff}
//	RibbonBar_Frame	Standard_Disabled	Rechteck	1	0	1	1	Ohne					Solide_1px	C9C7BA			{Name=Arial, Size=9, Color=a6a6a6}
//	Form_Standard	Standard	Rechteck	0	0	0	0	Solide		ECE9D8			Ohne
//	Form_MsgBox	Standard	Rechteck	0	0	0	0	Solide		ECE9D8			Ohne	7F9DB9			{Name=Arial, Size=9, Color=000000}
//	Form_QuickInfo	Standard	Rechteck	0	0	0	0	Solide		FFFFEE			Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Form_DesktopBenachrichtigung	Standard	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	FFFFFF	F4F2E8	DCD7CB	Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Form_BitteWarten	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFEE			Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Form_AutoFilter	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Form_AutoFilter	Standard_Disabled															{Name=Arial, Size=9, Color=a6a6a6}
//	Form_KontextMenu	Standard	Rechteck	0	0	0	0	Solide		F4F3EE			Solide_1px	0
//	Form_SelectBox_Dropdown	Standard	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	0			{Name=Arial, Size=9, Color=000000}
//	Item_DropdownMenu	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Item_DropdownMenu	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Item_DropdownMenu	Checked_Disabled	Rechteck	0	0	0	0	Solide		BDBAA2			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Item_DropdownMenu	Checked_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	316AC5	C1D3FB	316AC5	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=ffffff}
//	Item_DropdownMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Item_DropdownMenu	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=000000}
//	Item_KontextMenu	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Item_KontextMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Item_KontextMenu	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=000000}
//	Item_KontextMenu_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Ohne	97AEE2			{Name=Arial, Size=9, Bold=True, Color=000000}
//	Item_KontextMenu_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	BDBAA2	F4F3EE	F4F3EE	Ohne				{Name=Arial, Size=9, Bold=True, Color=a6a6a6}
//	Item_KontextMenu_Caption	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Solide_1px	97AEE2			{Name=Arial, Size=9, Bold=True, Color=000000}
//	Item_Autofilter	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Item_Autofilter	Standard_MouseOver_HasFocus_MousePressed	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	AEC8F7	C1D3FB	E6EEFC	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=000000}
//	Item_Autofilter	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Item_Autofilter	Checked_Disabled	Rechteck	0	0	0	0	Solide		BDBAA2			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Item_Autofilter	Checked_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	316AC5	C1D3FB	316AC5	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=ffffff}
//	Item_Autofilter	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Item_Autofilter	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=000000}
//	Item_Autofilter	Standard_HasFocus	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Item_Autofilter	Standard_MouseOver_HasFocus	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=000000}
//	Item_Listbox	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Item_Listbox	Standard_MouseOver_MousePressed	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=ffffff}
//	Item_Listbox	Checked	Rechteck	0	0	0	0	Solide		316AC5			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Item_Listbox	Checked_Disabled	Rechteck	0	0	0	0	Solide		BDBAA2			Ohne				{Name=Arial, Size=9, Color=ffffff}
//	Item_Listbox	Checked_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	316AC5	C1D3FB	316AC5	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=ffffff}
//	Item_Listbox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=a6a6a6}
//	Item_Listbox	Checked_MousePressed	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=ffffff}
//	Item_Listbox	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Vertical_3	0,5	E6EEFC	C1D3FB	AEC8F7	Solide_1px	97AEE2			{Name=Arial, Size=9, Color=ffffff}
//	Item_Listbox	Standard_MousePressed	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Item_Listbox_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Solide_1px	97AEE2			{Name=Arial, Size=9, Bold=True, Color=000000}
//	Item_Listbox_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	BDBAA2	F4F3EE	F4F3EE	Ohne				{Name=Arial, Size=9, Bold=True, Color=a6a6a6}
//	Item_Listbox_Caption	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Horizontal_3	0,5	AEC8F7	F4F3EE	F4F3EE	Solide_1px	97AEE2			{Name=Arial, Size=9, Bold=True, Color=000000}
//	Frame	Standard	Rechteck_RRechteckRechteck	0	0	-6	0	Ohne					Solide_1px	919B9C			{Name=Arial, Size=9, Color=0000ff}
//	Frame	Standard_Disabled	Rechteck_RRechteckRechteck	0	0	-6	0	Ohne					Solide_1px	919B9C			{Name=Arial, Size=9, Color=a6a6a6}
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
//	Table_Cell	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Color=000000}
//	Table_Cell_New	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Italic=True, Color=a6a6a6}
//	Table_Column	Standard	Ohne					Ohne					Ohne				{Name=Arial, Size=9, Bold=True, Color=ffffff}
//	Table_Cell_Chapter	Standard															{Name=Arial, Size=14, Bold=True, Underline=True, Color=000000}

//	Control	Status	Kontur	X1	X2	Y1	Y2	Draw Back	Verlauf Mitte	Color Back 1	Color Back 2	Color Back 3	Border Style	Color Border 1	Color Border 2	Color Border 3	Schrift	StandardPic
//	Button	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button	Standard_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	Glossy		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button	Standard_Disabled	Rechteck_R2Ohne	0	0	0	0	Glossy		87B7CD			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Button	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_CheckBox	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_CheckBox	Checked	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_CheckBox	Checked_Disabled	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Button_CheckBox	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_CheckBox	Standard_Disabled	Rechteck_R2Ohne	0	0	0	0	Glossy		87B7CD			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Button_CheckBox	Checked_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_CheckBox	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_CheckBox	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_OptionButton	Standard	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_OptionButton	Checked	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_OptionButton	Checked_Disabled	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Button_OptionButton	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_OptionButton	Standard_Disabled	Rechteck_R2Ohne	0	0	0	0	Glossy		87B7CD			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Button_OptionButton	Checked_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_OptionButton	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Glossy		2FBBFF			Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-3	-3	-3	-3	GlossyPressed		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Button_OptionButton	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Glossy		0095DD			Solide_1px_FocusDotLine	FFFFFF		FFFFFF	{Name=Comic Sans MS, Size=9, Color=ffffff}
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
//	TabStrip_Head	Standard	Rechteck_R2Ohne	0	0	0	15	Glossy		0095DD			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	TabStrip_Head	Checked	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFB834			Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=000000}
//	TabStrip_Head	Checked_Disabled	Rechteck_R2Ohne	0	0	0	8	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	TabStrip_Head	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFDA8C			Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=000000}
//	TabStrip_Head	Standard_Disabled	Rechteck_R2Ohne	0	0	0	15	Glossy		87B7CD			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	TabStrip_Head	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	15	Glossy		2FBBFF			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	RibbonBar_Head	Standard	Rechteck_R2Ohne	0	0	0	15	Glossy		BCDFED			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	RibbonBar_Head	Checked	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFE696			Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=000000}
//	RibbonBar_Head	Checked_Disabled	Rechteck_R2Ohne	0	0	0	8	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	F8DFB1			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	RibbonBar_Head	Checked_MouseOver	Rechteck_R2Ohne	0	0	0	15	GlossyPressed		FFE696			Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	RibbonBar_Head	Standard_Disabled	Rechteck_R2Ohne	0	0	0	15	Glossy		ECE9D8			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	RibbonBar_Head	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	15	Glossy		BCDFED			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Color=ffffff}
//	Caption	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	CheckBox_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox
//	CheckBox_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}	CheckBox_Disabled_Checked
//	CheckBox_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}	CheckBox_Disabled
//	CheckBox_TextStyle	Checked_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_Checked
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_MouseOver
//	CheckBox_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_Checked_MouseOver
//	CheckBox_TextStyle	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox
//	CheckBox_TextStyle	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	CheckBox_MouseOver
//	OptionButton_TextStyle	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox
//	OptionButton_TextStyle	Checked	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}	OptionBox_Disabled_Checked
//	OptionButton_TextStyle	Checked_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}	OptionBox_Disabled
//	OptionButton_TextStyle	Checked_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox_Checked
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox_MouseOver
//	OptionButton_TextStyle	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox_Checked_MouseOver
//	OptionButton_TextStyle	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox
//	OptionButton_TextStyle	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					FocusDotLine			404040	{Name=Comic Sans MS, Size=9, Color=0046d5}	OptionBox_MouseOver
//	TextBox	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	TextBox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	TextBox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	TextBox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	TextBox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	ListBox	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	ListBox	Standard_Disabled	Rechteck_RRechteckRechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	ComboBox_Textbox	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	ComboBox_Textbox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	ComboBox_Textbox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	ComboBox_Textbox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	ComboBox_Textbox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	ComboBox_Textbox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	ComboBox_Textbox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Table_And_Pad	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	Table_And_Pad	Standard_Disabled	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	Table_And_Pad	Standard_HasFocus	Rechteck	0	0	0	0	Solide		FFFFFF			Solide_1px	7F9DB9
//	EasyPic	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA
//	EasyPic	Standard_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	EasyPic	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2
//	TextBox_Stufe3	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=12, Underline=True, Color=0046d5}
//	TextBox_Stufe3	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=12, Underline=True, Color=000000}
//	TextBox_Stufe3	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=12, Underline=True, Color=a6a6a6}
//	TextBox_Stufe3	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=12, Underline=True, Color=000000}
//	TextBox_Stufe3	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=12, Underline=True, Color=0046d5}
//	TextBox_Bold	Standard	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Bold=True, Color=0046d5}
//	TextBox_Bold	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Bold=True, Color=000000}
//	TextBox_Bold	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Bold=True, Color=a6a6a6}
//	TextBox_Bold	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Bold=True, Color=000000}
//	TextBox_Bold	Standard_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	97AEE2			{Name=Comic Sans MS, Size=9, Bold=True, Color=0046d5}
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
//	Ribbonbar_Button	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button	Standard_HasFocus_MousePressed	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	F7FAFB	E4EFF3	F7FAFB	Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Ribbonbar_Button	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Ohne	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Checked	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_Disabled	Rechteck_R2Ohne	-2	-2	-2	-2	Verlauf_Vertical_Solide	0,5	ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFE696			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Ohne	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_CheckBox	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_CheckBox	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Checked	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_Disabled	Rechteck_R2Ohne	-2	-2	-2	-2	Verlauf_Vertical_Solide	0,5	ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFE696			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Checked_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFB834			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Ohne	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R2Ohne	-2	-2	-2	-2	Solide		FFDA8C			Solide_1px_FocusDotLine	0		FFFFFF	{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbonbar_Button_OptionButton	Standard_HasFocus	Rechteck_R2Ohne	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Button_OptionButton	Standard_MouseOver_HasFocus	Rechteck_R2Ohne	0	0	0	0	Verlauf_Vertical_3	0,5	E4EFF3	F7FAFB	E4EFF3	Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_Disabled	Rechteck_R4	0	0	0	0	Solide		FFFFFF			Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Checked_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Checked_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Ribbon_ComboBox_Textbox	Standard_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbon_ComboBox_Textbox	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	FFFFFF			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Caption	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Ribbonbar_Caption	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Ribbonbar_Button_Combobox	Standard	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus_MousePressed	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_Disabled	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_HasFocus	Ohne					Ohne					Ohne
//	Ribbonbar_Button_Combobox	Standard_MouseOver_HasFocus	Ohne					Ohne					Ohne
//	RibbonBar_Frame	Standard	Rechteck	1	0	1	1	Ohne					Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=000000}
//	RibbonBar_Frame	Standard_Disabled	Rechteck	1	0	1	1	Ohne					Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Form_Standard	Standard	Rechteck	0	0	0	0	Solide		D7E7EE			Ohne
//	Form_MsgBox	Standard	Rechteck	0	0	0	0	Solide		D7E7EE			Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Form_QuickInfo	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Form_DesktopBenachrichtigung	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Form_BitteWarten	Standard	Rechteck_R2Ohne	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Form_AutoFilter	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Form_AutoFilter	Standard_Disabled															{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Form_KontextMenu	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB
//	Form_SelectBox_Dropdown	Standard	Rechteck_R4	0	0	0	0	Solide		F7FAFB			Solide_1px	498DAB			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_DropdownMenu	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_DropdownMenu	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_DropdownMenu	Checked_Disabled	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Item_DropdownMenu	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_DropdownMenu	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Item_DropdownMenu	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_KontextMenu	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_KontextMenu	Standard_Disabled	Rechteck	0	0	0	0	Solide		F7FAFB			Ohne				{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Item_KontextMenu	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_KontextMenu_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_KontextMenu_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	ECE9D8	F7FAFB		Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Item_KontextMenu_Caption	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Solide_1px	0			{Name=Comic Sans MS, Size=9, Color=000000}
//	Item_Autofilter	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_Autofilter	Standard_MouseOver_HasFocus_MousePressed	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	0046D5			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_Autofilter	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Autofilter	Checked_Disabled	Rechteck_R4	0	0	0	0	Ohne					Solide_1px	C9C7BA			{Name=Comic Sans MS, Size=9, Color=dbdbdb}
//	Item_Autofilter	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Autofilter	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Italic=True, Color=dbdbdb}
//	Item_Autofilter	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Autofilter	Standard_HasFocus	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_Autofilter	Standard_MouseOver_HasFocus	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Listbox	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_Listbox	Standard_MouseOver_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Listbox	Checked	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Listbox	Checked_Disabled	Rechteck_R4	0	0	0	0	Verlauf_Vertical_Solide		ECE9D8	F8DFB1		Solide_1px	FFB834			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Item_Listbox	Checked_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Listbox	Standard_Disabled	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Item_Listbox	Checked_MousePressed	Rechteck_R4	0	0	0	0	Solide		FFB834			Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Listbox	Standard_MouseOver	Rechteck_R4	0	0	0	0	Verlauf_Vertical_3	0,5	FFE696	FFDA8C	FFB834	Solide_1px	0			{Name=Comic Sans MS, Size=9, Italic=True, Color=000000}
//	Item_Listbox	Standard_MousePressed	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_Listbox_Caption	Standard	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Item_Listbox_Caption	Standard_Disabled	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	ECE9D8	F7FAFB		Ohne				{Name=Comic Sans MS, Size=9, Color=a6a6a6}
//	Item_Listbox_Caption	Standard_MouseOver	Rechteck	0	0	0	0	Verlauf_Horizontal_Solide	0,5	BCDFED	F7FAFB		Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Frame	Standard	Rechteck_R2Ohne	-2	-2	-10	-2	Ohne					Solide_3px	FFB834			{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Frame	Standard_Disabled	Rechteck_R2Ohne	-1	-1	-10	-1	Ohne					Solide_1px	A6A6A6			{Name=Comic Sans MS, Size=9, Color=a6a6a6}
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
//	Table_Cell	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Color=0046d5}
//	Table_Cell_New	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9, Italic=True, Color=0046d5}
//	Table_Column	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=9,4, Bold=True, Color=0046d5}
//	Table_Cell_Chapter	Standard	Ohne					Ohne					Ohne				{Name=Comic Sans MS, Size=13, Bold=True, Underline=True, Color=0046d5}

namespace BlueControls;

public static class Skin {

    #region Fields

    public static readonly Dictionary<Design, Dictionary<States, SkinDesign>> Design = new();
    public static readonly string DummyStandardFont = "<Name=Arial, Size=10>";
    public static readonly string ErrorFont = "<Name=Arial, Size=8, Color=FF0000>";
    public static readonly int Padding = 9;
    public static readonly int PaddingSmal = 3;
    public static readonly float Scale = (float)Math.Round(SystemInformation.VirtualScreen.Width / SystemParameters.VirtualScreenWidth, 2);
    public static DatabaseAbstract? StyleDb;
    internal static Pen? PenLinieDick;
    internal static Pen? PenLinieDünn;
    internal static Pen? PenLinieKräftig;
    private static readonly ImageCodeEffect[] St = new ImageCodeEffect[1];

    #endregion

    #region Properties

    public static bool Inited { get; private set; }

    public static Color RandomColor =>
        Color.FromArgb((byte)Constants.GlobalRND.Next(0, 255),
            (byte)Constants.GlobalRND.Next(0, 255),
            (byte)Constants.GlobalRND.Next(0, 255));

    #endregion

    #region Methods

    public static ImageCodeEffect AdditionalState(States vState) => vState.HasFlag(States.Standard_Disabled) ? St[0] : ImageCodeEffect.Ohne;

    public static List<string>? AllStyles() {
        if (StyleDb == null) { InitStyles(); }
        return StyleDb?.Column?.First()?.Contents();
    }

    public static void ChangeDesign(Design ds, States status, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, float verlauf, string bc1, string bc2, string bc3, RahmenArt rahm, string boc1, string boc2, string boc3, string f, string pic) {
        Design.Remove(ds, status);
        Design.Add(ds, status, enKontur, x1, y1, x2, y2, hint, verlauf, bc1, bc2, bc3, rahm, boc1, boc2, boc3, f, pic);
    }

    public static Color Color_Back(Design vDesign, States vState) => DesignOf(vDesign, vState).BackColor1;

    public static SkinDesign DesignOf(Design design, States state) {
        try {
            return Design[design][state];
        } catch {
            SkinDesign d = new() {
                BackColor1 = Color.White,
                BorderColor1 = Color.Red,
                BFont = BlueFont.DefaultFont,
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
                        if (design.Kontur == Enums.Kontur.Rechteck && design.X1 >= 0 && design.X2 >= 0 && design.Y1 >= 0 && design.Y2 >= 0) { design.Need = false; }
                        if (design.Kontur == Enums.Kontur.Rechteck_R4 && design.X1 >= 1 && design.X2 >= 1 && design.Y1 >= 1 && design.Y2 >= 1) { design.Need = false; }
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
                    penX = new Pen(design.BorderColor3) {
                        DashStyle = DashStyle.Dot
                    };
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case RahmenArt.FocusDotLine:
                    penX = new Pen(design.BorderColor3) {
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
    /// <param name="GR"></param>
    /// <param name="txt"></param>
    /// <param name="design"></param>
    /// <param name="state"></param>
    /// <param name="imageCode"></param>
    /// <param name="align"></param>
    /// <param name="fitInRect"></param>
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>
    public static void Draw_FormatedText(Graphics gr, string txt, Design design, States state, QuickImage? imageCode, Alignment align, Rectangle fitInRect, Control? child, bool deleteBack, bool translate) => Draw_FormatedText(gr, txt, imageCode, DesignOf(design, state), align, fitInRect, child, deleteBack, translate);

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? imageCode, Alignment align, Rectangle fitInRect, BlueFont? bFont, bool translate) => Draw_FormatedText(gr, txt, imageCode, align, fitInRect, null, false, bFont, translate);

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
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, SkinDesign design, Alignment align, Rectangle fitInRect, Control? child, bool deleteBack, bool translate) {
        if (string.IsNullOrEmpty(txt) && qi == null) { return; }
        QuickImage? tmpImage = null;
        if (qi != null) { tmpImage = QuickImage.Get(qi, AdditionalState(design.Status)); }
        Draw_FormatedText(gr, txt, tmpImage, align, fitInRect, child, deleteBack, design.BFont, translate);
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

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, Control? child, bool deleteBack, BlueFont? bFont, bool translate) {
        var pSize = SizeF.Empty;
        var tSize = SizeF.Empty;
        float xp = 0;
        float yp1 = 0;
        float yp2 = 0;
        if (qi != null) { pSize = ((Bitmap)qi).Size; }
        if (LanguageTool.Translation != null) { txt = LanguageTool.DoTranslate(txt, translate); }
        if (bFont != null) {
            if (fitInRect.Width > 0) { txt = bFont.TrimByWidth(txt, fitInRect.Width - pSize.Width); }
            tSize = gr.MeasureString(txt, bFont.Font());
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
            if (!string.IsNullOrEmpty(txt)) { bFont.DrawString(gr, txt, fitInRect.X + pSize.Width + xp, fitInRect.Y + yp2); }
        } catch (Exception) {
            // es kommt selten vor, dass das Graphics-Objekt an anderer Stelle verwendet wird. Was immer das auch heißen mag...
            //Develop.DebugPrint(ex);
        }
    }

    public static Size FormatedText_NeededSize(string text, QuickImage? image, BlueFont? font, int minSie) {
        try {
            var pSize = SizeF.Empty;
            var tSize = SizeF.Empty;
            if (font == null) { return new Size(3, 3); }
            if (image != null) { pSize = ((Bitmap)image).Size; }
            if (!string.IsNullOrEmpty(text)) { tSize = BlueFont.MeasureString(text, font.Font()); }

            if (!string.IsNullOrEmpty(text)) {
                if (image == null) {
                    return new Size((int)(tSize.Width + 1), Math.Max((int)tSize.Height, minSie));
                }

                return new Size((int)(tSize.Width + 2 + pSize.Width + 1),
                    Math.Max((int)tSize.Height, (int)pSize.Height));
            }

            if (image != null) {
                return new Size((int)pSize.Width, (int)pSize.Height);
            }

            return new Size(minSie, minSie);
        } catch {
            // tmpImageCode wird an anderer Stelle verwendet
            return FormatedText_NeededSize(text, image, font, minSie);
        }
    }

    public static BlueFont GetBlueFont(PadStyles format, RowItem? rowOfStyle) {
        if (StyleDb == null) { InitStyles(); }
        return StyleDb == null || rowOfStyle == null ? BlueFont.Get(ErrorFont) : GetBlueFont(StyleDb, "X" + ((int)format), rowOfStyle);
    }

    public static BlueFont GetBlueFont(Design design, States state) => DesignOf(design, state).BFont ?? BlueFont.DefaultFont;

    /// <summary>
    /// Gibt eine Liste aller Fonts zurück, die mit dem gewählten Sheetstyle möglich sind.
    /// </summary>
    /// <param name="sheetStyle"></param>
    /// <returns></returns>

    public static ItemCollectionList GetFonts(RowItem? sheetStyle) {
        ItemCollectionList rahms = new(false)
        {
            //   Rahms.GenerateAndAdd(New ItemCollection.TextListItem(CInt(PadStyles.Undefiniert).ToString, "Ohne Rahmen", ImageCode.Kreuz))
            { "Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Haupt).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Haupt, sheetStyle).SymbolForReadableText() },
            { "Untertitel für Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Untertitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Untertitel, sheetStyle).SymbolForReadableText() },
            { "Überschrift für Kapitel", ((int)PadStyles.Style_Überschrift_Kapitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Kapitel, sheetStyle).SymbolForReadableText() },
            { "Standard", ((int)PadStyles.Style_Standard).ToString(), GetBlueFont(PadStyles.Style_Standard, sheetStyle).SymbolForReadableText() },
            { "Standard Fett", ((int)PadStyles.Style_StandardFett).ToString(), GetBlueFont(PadStyles.Style_StandardFett, sheetStyle).SymbolForReadableText() },
            { "Standard Alternativ-Design", ((int)PadStyles.Style_StandardAlternativ).ToString(), GetBlueFont(PadStyles.Style_StandardAlternativ, sheetStyle).SymbolForReadableText() },
            { "Kleiner Zusatz", ((int)PadStyles.Style_KleinerZusatz).ToString(), GetBlueFont(PadStyles.Style_KleinerZusatz, sheetStyle).SymbolForReadableText() }
        };
        //rahms.Sort();
        return rahms;
    }

    public static ItemCollectionList GetRahmenArt(RowItem? sheetStyle, bool mitOhne) {
        ItemCollectionList rahms = new(false);
        if (mitOhne) {
            _ = rahms.Add("Ohne Rahmen", ((int)PadStyles.Undefiniert).ToString(), ImageCode.Kreuz);
        }
        _ = rahms.Add("Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Haupt).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Haupt, sheetStyle).SymbolOfLine());
        _ = rahms.Add("Untertitel für Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Untertitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Untertitel, sheetStyle).SymbolOfLine());
        _ = rahms.Add("Überschrift für Kapitel", ((int)PadStyles.Style_Überschrift_Kapitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Kapitel, sheetStyle).SymbolOfLine());
        _ = rahms.Add("Standard", ((int)PadStyles.Style_Standard).ToString(), GetBlueFont(PadStyles.Style_Standard, sheetStyle).SymbolOfLine());
        _ = rahms.Add("Standard Fett", ((int)PadStyles.Style_StandardFett).ToString(), GetBlueFont(PadStyles.Style_StandardFett, sheetStyle).SymbolOfLine());
        _ = rahms.Add("Standard Alternativ-Design", ((int)PadStyles.Style_StandardAlternativ).ToString(), GetBlueFont(PadStyles.Style_StandardAlternativ, sheetStyle).SymbolOfLine());
        _ = rahms.Add("Kleiner Zusatz", ((int)PadStyles.Style_KleinerZusatz).ToString(), GetBlueFont(PadStyles.Style_KleinerZusatz, sheetStyle).SymbolOfLine());
        //rahms.Sort();
        return rahms;
    }

    public static Color IdColor(List<int>? id) {
        if (id == null || id.Count == 0) {
            return IdColor(-1);
        }
        return IdColor(id[0]);
    }

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

    public static void InitStyles() => StyleDb = DatabaseAbstract.LoadResource(Assembly.GetAssembly(typeof(Skin)), "Styles.BDB", "Styles", true, false);

    // Der Abstand von z.B. in Textboxen: Text Linke Koordinate
    public static void LoadSkin() {
        //_SkinString = "Windows10";
        //SkinDB = Database.LoadResource(Assembly.GetAssembly(typeof(Skin)), _SkinString + ".skn", "Skin", true, Develop.AppName() == "SkinDesigner");
        //="Design.GenerateAndAdd(enDesign."& A3 & ",enStates."&B3&", enKontur."& C3 & ", " &D3&", "&E3&", "&F3&","&G3&", enHintergrundArt."&H3&","&I3&",'"&J3&"','"&K3&"','"&L3&"',enRahmenArt."&M3&",'"&N3&"','"&O3&"','"&P3&"','"&Q3&"','"&R3&"');"
        Design.Add(Enums.Design.Button, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "DFDFDF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_3px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_CheckBox, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "DFDFDF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_3px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_OptionButton, States.Standard_MouseOver_HasFocus, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Button_AutoFilter, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_AutoFilter, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "FF0000", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_AutoFilter, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_ComboBox, States.Standard, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_HasFocus_MousePressed, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_Disabled, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EAEAEA", string.Empty, string.Empty, RahmenArt.Solide_3px, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_HasFocus, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_ComboBox, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, -3, -3, -3, -3, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CDCDCD", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard_MouseOver_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CECECE", string.Empty, string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Waagerecht, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CECECE", string.Empty, string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CDCDCD", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard_MouseOver_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CECECE", string.Empty, string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_Slider_Senkrecht, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CECECE", string.Empty, string.Empty, RahmenArt.Solide_3px, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F9F9F9", string.Empty, string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Ohne, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_SliderDesign, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_EckpunktSchieber, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_EckpunktSchieber, States.Checked_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Button_EckpunktSchieber_Phantom, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.TabStrip_Head, States.Standard, Enums.Kontur.Rechteck, 0, -2, 0, 5, HintergrundArt.Verlauf_Vertical_2, 0.5f, "F0F0F0", "E4E4E4", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "DFDFDF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, -2, 0, 5, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.TabStrip_Head, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, -2, 0, 5, HintergrundArt.Verlauf_Vertical_2, 0.5f, "F0F0F0", "E4E4E4", string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "F4F5F6", string.Empty, string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "F4F5F6", string.Empty, string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "F4F5F6", string.Empty, string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=0000ff}", string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.RibbonBar_Head, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=0000ff}", string.Empty);
        Design.Add(Enums.Design.Caption, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Caption, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "CheckBox");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "CheckBox_Disabled_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_MouseOver, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "CheckBox_Disabled");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_MouseOver, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "CheckBox_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked_MouseOver");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox");
        Design.Add(Enums.Design.CheckBox_TextStyle, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "OptionBox");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "OptionBox_Disabled_Checked");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_MouseOver, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "OptionBox_Disabled");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_MouseOver, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", "OptionBox_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked_MouseOver");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox");
        Design.Add(Enums.Design.OptionButton_TextStyle, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.FocusDotLine, string.Empty, string.Empty, "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_MouseOver");
        Design.Add(Enums.Design.TextBox, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.TextBox, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.TextBox, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.TextBox, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.TextBox, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.ListBox, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.ListBox, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.ComboBox_Textbox, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Table_And_Pad, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Table_And_Pad, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Table_And_Pad, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.EasyPic, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.EasyPic, States.Standard_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.EasyPic, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.EasyPic, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.EasyPic, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.EasyPic, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.TextBox_Stufe3, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=12,Bold=True,Underline=True}", string.Empty);
        Design.Add(Enums.Design.TextBox_Stufe3, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=12,Bold=True,Underline=True,Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.TextBox_Stufe3, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=12,Bold=True,Underline=True,Color=c0c0c0}", string.Empty);
        Design.Add(Enums.Design.TextBox_Stufe3, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=12,Bold=True,Underline=True,Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.TextBox_Stufe3, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=12,Bold=True,Underline=True}", string.Empty);
        Design.Add(Enums.Design.TextBox_Bold, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.TextBox_Bold, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True,Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.TextBox_Bold, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True,Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.TextBox_Bold, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True,Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.TextBox_Bold, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_MouseOver_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F9F9F9", string.Empty, string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Waagerecht, States.Standard_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MouseOver_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F9F9F9", string.Empty, string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Slider_Hintergrund_Senkrecht, States.Standard_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "EFEFEF", string.Empty, string.Empty, RahmenArt.Ohne, "B7B7B7", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_HasFocus_MousePressed, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "DFDFDF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_CheckBox, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "DFDFDF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "81B8EF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_OptionButton, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "D8D8D8", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Checked_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbon_ComboBox_Textbox, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "3399FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Caption, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Caption, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_HasFocus_MousePressed, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_MouseOver, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_HasFocus, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Ribbonbar_Button_Combobox, States.Standard_MouseOver_HasFocus, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.RibbonBar_Frame, States.Standard, Enums.Kontur.Rechteck, 1, 1, 0, 1, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Italic=True}", string.Empty);
        Design.Add(Enums.Design.RibbonBar_Frame, States.Standard_Disabled, Enums.Kontur.Rechteck, 1, 1, 0, 1, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Italic=True,Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Form_Standard, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Form_MsgBox, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Form_QuickInfo, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        // Design.GenerateAndAdd(enDesign.Form_DesktopBenachrichtigung, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "1F1F1F", string.Empty,"", enRahmenArt.Solide_3px, "484848", string.Empty,"", "{Name=Calibri, Size=12[K]5,Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Form_DesktopBenachrichtigung, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Solide_3px, "5D5D5D", string.Empty, string.Empty, "{Name=Calibri, Size=11[K]9}", string.Empty);
        Design.Add(Enums.Design.Form_BitteWarten, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Form_AutoFilter, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Form_AutoFilter, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Form_KontextMenu, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F0F0F0", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Form_SelectBox_Dropdown, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "5D5D5D", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Item_DropdownMenu, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu_Caption, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu_Caption, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.Item_KontextMenu_Caption, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_MouseOver_HasFocus_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "5D5D5D", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Solide_1px, "000000", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_HasFocus, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_Autofilter, States.Standard_MouseOver_HasFocus, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_MouseOver_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CCE8FF", string.Empty, string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Underline=True}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CCE8FF", string.Empty, string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "DFDFDF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B7B7B7", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CCE8FF", string.Empty, string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Underline=True}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_Disabled, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Checked_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CCE8FF", string.Empty, string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "E5F3FF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Underline=True}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox, States.Standard_MousePressed, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "CCE8FF", string.Empty, string.Empty, RahmenArt.Solide_1px, "99D1FF", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox_Caption, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox_Caption, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "DFDFDF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True,Color=ffffff}", string.Empty);
        Design.Add(Enums.Design.Item_Listbox_Caption, States.Standard_MouseOver, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "BFDFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "4DA1B5", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.GroupBox, States.Standard, Enums.Kontur.Rechteck, 0, -7, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.GroupBox, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, -7, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.GroupBoxBold, States.Standard, Enums.Kontur.Rechteck, 9, -11, 9, 9, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_21px, "40568D", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=FFFFFF}", string.Empty);
        Design.Add(Enums.Design.GroupBoxBold, States.Standard_Disabled, Enums.Kontur.Rechteck, 9, -11, 9, 9, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_21px, "ACACAC", string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", string.Empty);
        Design.Add(Enums.Design.TabStrip_Body, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.TabStrip_Body, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "ACACAC", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.RibbonBar_Body, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F4F5F6", string.Empty, string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.RibbonBar_Body, States.Standard_Disabled, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "F4F5F6", string.Empty, string.Empty, RahmenArt.Solide_1px, "E5E4E5", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.RibbonBar_Back, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 5, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.TabStrip_Back, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Progressbar, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "FFFFFF", string.Empty, string.Empty, RahmenArt.Solide_1px, "B6B6B6", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Progressbar_Füller, States.Standard, Enums.Kontur.Rechteck, 0, 0, 0, 0, HintergrundArt.Solide, 0, "0072BC", string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        Design.Add(Enums.Design.Table_Lines_thick, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, "ACACAC", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Table_Lines_thin, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, "D8D8D8", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Table_Cursor, States.Standard, Enums.Kontur.Rechteck, -1, -1, -1, -1, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_3px, "ACACAC", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Table_Cursor, States.Standard_HasFocus, Enums.Kontur.Rechteck, -1, -1, -1, -1, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Solide_3px, "3399FF", string.Empty, string.Empty, string.Empty, "");
        Design.Add(Enums.Design.Table_Cell, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15}", string.Empty);
        Design.Add(Enums.Design.Table_Cell_New, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Italic=True}", string.Empty);
        Design.Add(Enums.Design.Table_Column, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=10[K]15,Bold=True}", string.Empty);
        Design.Add(Enums.Design.Table_Cell_Chapter, States.Standard, Enums.Kontur.Ohne, 0, 0, 0, 0, HintergrundArt.Ohne, 0, string.Empty, string.Empty, string.Empty, RahmenArt.Ohne, string.Empty, string.Empty, string.Empty, "{Name=Calibri, Size=15,Bold=True,Underline=True}", string.Empty);
        Inited = true;

        St[0] = ImageCodeEffect.WindowsXPDisabled;

        PenLinieDünn = new Pen(Color_Border(Enums.Design.Table_Lines_thin, States.Standard));
        PenLinieKräftig = new Pen(Color_Border(Enums.Design.Table_Lines_thick, States.Standard));
        PenLinieDick = new Pen(Color_Border(Enums.Design.Table_Lines_thick, States.Standard), 3);
    }

    internal static Color Color_Border(Design vDesign, States vState) => DesignOf(vDesign, vState).BorderColor1;

    internal static BlueFont? GetBlueFont(int design, States state, RowItem? rowOfStyle, int stufe) => design > 10000 ? GetBlueFont((PadStyles)design, rowOfStyle, stufe) : GetBlueFont((Design)design, state, stufe);

    //private static void Draw_Back_Verlauf_Vertical_Glanzpunkt(Graphics GR, RowItem Row, Rectangle r) {
    //    var cb = new ColorBlend();
    //    var c1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
    //    var c2 = Color.FromArgb(Value(Row, col_Color_Back_2, 0));
    //    var PR = Value(Row, col_Verlauf_Mitte, 0.05f);
    //    if (PR < 0.06F) { PR = 0.06F; }
    //    if (PR > 0.94F) { PR = 0.94F; }
    //    var lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Bottom), c1, c1);
    //    cb.Colors = new[] { c1, c2, c1, c1, c1.Darken(0.3), c1 };
    //    cb.Positions = new[]
    //    {
    //        0.0F, (float) (PR - 0.05), (float) (PR + 0.05), (float) (1 - PR - 0.05), (float) (1 - PR + 0.05), 1.0F
    //    };
    //    lgb.InterpolationColors = cb;
    //    lgb.GammaCorrection = true;
    //    GR.FillRectangle(lgb, r);
    //}
    //private static void Draw_Back_Verlauf_Horizontal_2(Graphics GR, RowItem Row, Rectangle r) {
    //    var c1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
    //    var c2 = Color.FromArgb(Value(Row, col_Color_Back_2, 0));
    //    var lgb = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Horizontal);
    //    GR.FillRectangle(lgb, r);
    //}
    //private static void Draw_Back_Verlauf_Horizontal_3(Graphics GR, RowItem Row, Rectangle r) {
    //    var cb = new ColorBlend();
    //    var c1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
    //    var c2 = Color.FromArgb(Value(Row, col_Color_Back_2, 0));
    //    var c3 = Color.FromArgb(Value(Row, col_Color_Back_3, 0));
    //    var PR = Value(Row, col_Verlauf_Mitte, 0.5f);
    //    var lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Right, r.Top), c1, c3);
    //    cb.Colors = new[] { c1, c2, c3 };
    //    cb.Positions = new[] { 0.0F, PR, 1.0F };
    //    lgb.InterpolationColors = cb;
    //    lgb.GammaCorrection = true;
    //    GR.FillRectangle(lgb, r);
    //    GR.DrawLine(new Pen(c3), r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
    //}
    //private static void Draw_Back_Glossy(Graphics GR, RowItem Row, Rectangle r) {
    //    var col1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
    //    var cb = new ColorBlend();
    //    var c1 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.Black, 1), 0.4);
    //    var c2 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.FromArgb(64, 64, 64), 1), 0.2);
    //    var c3 = Extensions.SoftLightMix(col1, Color.FromArgb(128, 128, 128), 1);
    //    var c4 = Extensions.SoftLightMix(col1, Color.FromArgb(192, 192, 192), 1);
    //    var c5 = Extensions.OverlayMix(Extensions.SoftLightMix(col1, Color.White, 1), Color.White, 0.75);
    //    cb.Colors = new[] { c1, c2, c3, c4, c5 };
    //    cb.Positions = new[] { 0.0F, 0.25F, 0.5F, 0.75F, 1 };
    //    var lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Top + r.Height + 1), c1, c5) {
    //        InterpolationColors = cb
    //    };
    //    Draw_Back_Glossy_TMP(lgb, r, GR, 20);
    //    c2 = Color.White;
    //    cb.Colors = new[] { c2, c3, c4, c5 };
    //    cb.Positions = new[] { 0.0F, 0.5F, 0.75F, 1.0F };
    //    lgb = new LinearGradientBrush(new Point(r.Left + 1, r.Top), new Point(r.Left + 1, r.Top + r.Height - 1), c2, c5) {
    //        InterpolationColors = cb
    //    };
    //    r.Inflate(-4, -4);
    //    GR.SmoothingMode = SmoothingMode.HighQuality;
    //    Draw_Back_Glossy_TMP(lgb, r, GR, 16);
    //}
    //private static void Draw_Back_GlossyPressed(Graphics GR, RowItem Row, Rectangle r) {
    //    var col1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
    //    var cb = new ColorBlend();
    //    var c5 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.Black, 1), 0.4);
    //    var c4 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.FromArgb(64, 64, 64), 1), 0.2);
    //    var c3 = Extensions.SoftLightMix(col1, Color.FromArgb(128, 128, 128), 1);
    //    var c2 = Extensions.SoftLightMix(col1, Color.FromArgb(192, 192, 192), 1);
    //    var c1 = Extensions.OverlayMix(Extensions.SoftLightMix(col1, Color.White, 1), Color.White, 0.75);
    //    cb.Colors = new[] { c1, c2, c3, c4, c5 };
    //    cb.Positions = new[] { 0.0F, 0.25F, 0.5F, 0.75F, 1 };
    //    var lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Top + r.Height + 1), c1, c5) {
    //        InterpolationColors = cb
    //    };
    //    Draw_Back_Glossy_TMP(lgb, r, GR, 20);
    //    c2 = Color.White;
    //    cb.Colors = new[] { c2, c3, c4, c5 };
    //    cb.Positions = new[] { 0.0F, 0.5F, 0.75F, 1.0F };
    //    lgb = new LinearGradientBrush(new Point(r.Left + 1, r.Top), new Point(r.Left + 1, r.Top + r.Height - 1), c2, c5) {
    //        InterpolationColors = cb
    //    };
    //    r.Inflate(-4, -4);
    //    GR.SmoothingMode = SmoothingMode.HighQuality;
    //    Draw_Back_Glossy_TMP(lgb, r, GR, 16);
    //    //    GR.SmoothingModex = Drawing2D.SmoothingMode.None
    //}
    //private static void Draw_Back_Glossy_TMP(Brush b, Rectangle rect, Graphics GR, int RMinus) {
    //    var r = Math.Min(RMinus, Math.Min(rect.Width, rect.Height) - 1);
    //    var r2 = (int)Math.Truncate(r / 2.0);
    //    r = r2 * 2;
    //    GR.FillEllipse(b, new Rectangle(rect.Left, rect.Top - 1, r, r));
    //    GR.FillEllipse(b, new Rectangle(rect.Right - r - 1, rect.Top - 1, r, r));
    //    GR.FillEllipse(b, new Rectangle(rect.Left, rect.Bottom - r, r, r));
    //    GR.FillEllipse(b, new Rectangle(rect.Right - r - 1, rect.Bottom - r, r, r));
    //    GR.SmoothingMode = SmoothingMode.None;
    //    GR.FillRectangle(b, new Rectangle(rect.Left + r2, rect.Top, rect.Width - r, rect.Height));
    //    GR.FillRectangle(b, new Rectangle(rect.Left, rect.Top + r2, rect.Width, rect.Height - r));
    //}

    internal static BlueFont? GetBlueFont(PadStyles padStyle, RowItem? rowOfStyle, int stufe) {
        switch (stufe) {
            case 4:
                return GetBlueFont(padStyle, rowOfStyle);

            case 3:
                switch (padStyle) {
                    case PadStyles.Style_Standard:
                        return GetBlueFont(PadStyles.Style_Überschrift_Kapitel, rowOfStyle);

                    case PadStyles.Style_StandardFett:
                        return GetBlueFont(PadStyles.Style_Überschrift_Kapitel, rowOfStyle);
                        //    Case Else : Return BlueFont(vDesign, vState)
                }
                break;

            case 2:
                switch (padStyle) {
                    case PadStyles.Style_Standard:
                        return GetBlueFont(PadStyles.Style_Überschrift_Untertitel, rowOfStyle);

                    case PadStyles.Style_StandardFett:
                        return GetBlueFont(PadStyles.Style_Überschrift_Untertitel, rowOfStyle);
                        //    Case Else : Return BlueFont(vDesign, vState)
                }
                break;

            case 1:
                switch (padStyle) {
                    case PadStyles.Style_Standard:
                        return GetBlueFont(PadStyles.Style_Überschrift_Haupt, rowOfStyle);

                    case PadStyles.Style_StandardFett:
                        return GetBlueFont(PadStyles.Style_Überschrift_Haupt, rowOfStyle);
                        //  Case Else : Return BlueFont(vDesign, vState)
                }
                break;

            case 7:
                switch (padStyle) {
                    case PadStyles.Style_Standard:
                        return GetBlueFont(PadStyles.Style_StandardFett, rowOfStyle);

                    case PadStyles.Style_StandardFett:
                        return GetBlueFont(PadStyles.Style_Standard, rowOfStyle);
                        //default: : Return BlueFont(vDesign, vState)
                }
                break;
        }
        Develop.DebugPrint(FehlerArt.Fehler, "Stufe " + stufe + " nicht definiert.");
        return null;
    }

    internal static BlueFont GetBlueFont(Design design, States state, int stufe) {
        if (stufe != 4 && design != Enums.Design.TextBox) {
            if (design == Enums.Design.Form_QuickInfo) { return GetBlueFont(design, state); } // QuickInfo kann jeden Text enthatlten
            Develop.DebugPrint(FehlerArt.Warnung, "Design unbekannt: " + (int)design);
            return GetBlueFont(design, state);
        }
        switch (stufe) {
            case 4:
                return GetBlueFont(design, state);

            case 3:
                return GetBlueFont(Enums.Design.TextBox_Stufe3, state);

            case 2:
                return GetBlueFont(Enums.Design.TextBox_Stufe3, state);

            case 1:
                return GetBlueFont(Enums.Design.TextBox_Stufe3, state);

            case 7:
                return GetBlueFont(Enums.Design.TextBox_Bold, state);
        }
        Develop.DebugPrint(FehlerArt.Fehler, "Stufe " + stufe + " nicht definiert.");
        return GetBlueFont(design, state);
    }

    private static BlueFont GetBlueFont(DatabaseAbstract styleDb, string column, RowItem? row) => GetBlueFont(styleDb, styleDb.Column[column], row);

    private static BlueFont GetBlueFont(DatabaseAbstract styleDb, ColumnItem? column, RowItem? row) {
        var @string = styleDb.Cell.GetString(column, row);
        if (string.IsNullOrEmpty(@string)) {
            Develop.DebugPrint("Schrift nicht definiert: " + styleDb.TableName + " - " + column.Name + " - " + row.CellFirstString());
            return BlueFont.DefaultFont; // BlueFont.Get("Arial", 7, false, false, false, false, false, Color.Black, Color.Transparent, false, false, false);
        }
        return BlueFont.Get(@string);
    }

    private static GraphicsPath? Kontur(Kontur kon, Rectangle r) => kon switch {
        Enums.Kontur.Rechteck => Poly_Rechteck(r),// GR.SmoothingModex = Drawing2D.SmoothingMode.None
        Enums.Kontur.Rechteck_R4 => Poly_RoundRec(r, 4),// GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
        Enums.Kontur.Ohne => null,
        _ => Poly_Rechteck(r) //  GR.SmoothingModex = Drawing2D.SmoothingMode.None
    };

    #endregion
}