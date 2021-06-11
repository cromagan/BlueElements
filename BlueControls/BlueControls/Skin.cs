#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
//  = A3 & ".Design.Add(enStates."&B3&", enKontur."& C3 & ", " &D3&", "&E3&", "&F3&","&G3&", enHintergrundArt."&H3&","&I3&",'"&J3&"','"&K3&"','"&L3&"',enRahmenArt."&M3&",'"&N3&"','"&O3&"','"&P3&"','"&Q3&"','"&R3&"');"
#region Win 10
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
#endregion
#region Win XP
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
#endregion
#region GlossyCyan
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
#endregion
namespace BlueControls {
    public static class Skin {
        public static Database StyleDB;
        public static bool inited = false;
        private static readonly enImageCodeEffect[] ST = new enImageCodeEffect[1];
        internal static Pen Pen_LinieDünn;
        internal static Pen Pen_LinieKräftig;
        internal static Pen Pen_LinieDick;
        public static readonly float Scale = (float)Math.Round(System.Windows.Forms.SystemInformation.VirtualScreen.Width / System.Windows.SystemParameters.VirtualScreenWidth, 2);
        public static string ErrorFont = "<Name=Arial, Size=8, Color=FF0000>";
        public static string DummyStandardFont = "<Name=Arial, Size=10>";
        public static readonly int PaddingSmal = 3; // Der Abstand von z.B. in Textboxen: Text Linke Koordinate
        public static readonly int Padding = 9;
        public static readonly Dictionary<enDesign, Dictionary<enStates, clsDesign>> Design = new();
        public static void LoadSkin() {
            //_SkinString = "Windows10";
            //SkinDB = Database.LoadResource(Assembly.GetAssembly(typeof(Skin)), _SkinString + ".skn", "Skin", true, Convert.ToBoolean(Develop.AppName() == "SkinDesigner"));
            //="Design.Add(enDesign."& A3 & ",enStates."&B3&", enKontur."& C3 & ", " &D3&", "&E3&", "&F3&","&G3&", enHintergrundArt."&H3&","&I3&",'"&J3&"','"&K3&"','"&L3&"',enRahmenArt."&M3&",'"&N3&"','"&O3&"','"&P3&"','"&Q3&"','"&R3&"');"
            Design.Add(enDesign.Button, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button, enStates.Standard_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Button, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_3px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "DFDFDF", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_3px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_3px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_CheckBox, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "DFDFDF", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_3px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_3px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_OptionButton, enStates.Standard_MouseOver_HasFocus, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Button_AutoFilter, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.Button_AutoFilter, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "FF0000", "", "", "", "");
            Design.Add(enDesign.Button_AutoFilter, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.Button_ComboBox, enStates.Standard, enKontur.Rechteck, -3, -3, -3, -3, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_ComboBox, enStates.Standard_HasFocus_MousePressed, enKontur.Rechteck, -3, -3, -3, -3, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_ComboBox, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, -3, -3, -3, -3, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_ComboBox, enStates.Standard_Disabled, enKontur.Rechteck, -3, -3, -3, -3, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_ComboBox, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_3px, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.Button_ComboBox, enStates.Standard_HasFocus, enKontur.Rechteck, -3, -3, -3, -3, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_ComboBox, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, -3, -3, -3, -3, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Waagerecht, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CDCDCD", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Waagerecht, enStates.Standard_MouseOver_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CECECE", "", "", enRahmenArt.Solide_3px, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Waagerecht, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Waagerecht, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CECECE", "", "", enRahmenArt.Solide_3px, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Senkrecht, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CDCDCD", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Senkrecht, enStates.Standard_MouseOver_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CECECE", "", "", enRahmenArt.Solide_3px, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Senkrecht, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_Slider_Senkrecht, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CECECE", "", "", enRahmenArt.Solide_3px, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Button_SliderDesign, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Button_SliderDesign, enStates.Standard_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "", "");
            Design.Add(enDesign.Button_SliderDesign, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "", "");
            Design.Add(enDesign.Button_SliderDesign, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F9F9F9", "", "", enRahmenArt.Ohne, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.Button_SliderDesign, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Ohne, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.Button_SliderDesign, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "", "");
            Design.Add(enDesign.Button_SliderDesign, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "", "");
            Design.Add(enDesign.Button_EckpunktSchieber, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "000000", "", "", "", "");
            Design.Add(enDesign.Button_EckpunktSchieber, enStates.Checked_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Solide_1px, "000000", "", "", "", "");
            Design.Add(enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.TabStrip_Head, enStates.Standard, enKontur.Rechteck, 0, 0, -2, 5, enHintergrundArt.Verlauf_Vertical_2, 0.5f, "F0F0F0", "E4E4E4", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.TabStrip_Head, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.TabStrip_Head, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "DFDFDF", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.TabStrip_Head, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.TabStrip_Head, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, -2, 5, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.TabStrip_Head, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, -2, 5, enHintergrundArt.Verlauf_Vertical_2, 0.5f, "F0F0F0", "E4E4E4", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.RibbonBar_Head, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.RibbonBar_Head, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "F4F5F6", "", "", enRahmenArt.Solide_1px, "E5E4E5", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.RibbonBar_Head, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "F4F5F6", "", "", enRahmenArt.Solide_1px, "E5E4E5", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.RibbonBar_Head, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "F4F5F6", "", "", enRahmenArt.Solide_1px, "E5E4E5", "", "", "{Name=Calibri, Size=10[K]15, Color=0000ff}", "");
            Design.Add(enDesign.RibbonBar_Head, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.RibbonBar_Head, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=0000ff}", "");
            Design.Add(enDesign.Caption, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Caption, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "CheckBox");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Checked, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Checked_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "CheckBox_Disabled_Checked");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Checked_MouseOver, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked_MouseOver");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "CheckBox_Disabled");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked_MouseOver");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Standard_MouseOver, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "CheckBox_MouseOver");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_Checked_MouseOver");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox");
            Design.Add(enDesign.CheckBox_TextStyle, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "CheckBox_MouseOver");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "OptionBox");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Checked, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Checked_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "OptionBox_Disabled_Checked");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Checked_MouseOver, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked_MouseOver");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "OptionBox_Disabled");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked_MouseOver");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Standard_MouseOver, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "OptionBox_MouseOver");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_Checked_MouseOver");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox");
            Design.Add(enDesign.OptionButton_TextStyle, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.FocusDotLine, "", "", "000000", "{Name=Calibri, Size=10[K]15}", "OptionBox_MouseOver");
            Design.Add(enDesign.TextBox, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.TextBox, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.TextBox, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.TextBox, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.TextBox, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.ListBox, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.ListBox, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Standard_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.ComboBox_Textbox, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Table_And_Pad, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.Table_And_Pad, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.Table_And_Pad, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "", "");
            Design.Add(enDesign.EasyPic, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.EasyPic, enStates.Standard_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "", "");
            Design.Add(enDesign.EasyPic, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "", "");
            Design.Add(enDesign.EasyPic, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.EasyPic, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.EasyPic, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "", "");
            Design.Add(enDesign.TextBox_Stufe3, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=12,Bold=True,Underline=True}", "");
            Design.Add(enDesign.TextBox_Stufe3, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=12,Bold=True,Underline=True,Color=ffffff}", "");
            Design.Add(enDesign.TextBox_Stufe3, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=12,Bold=True,Underline=True,Color=c0c0c0}", "");
            Design.Add(enDesign.TextBox_Stufe3, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=12,Bold=True,Underline=True,Color=ffffff}", "");
            Design.Add(enDesign.TextBox_Stufe3, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=12,Bold=True,Underline=True}", "");
            Design.Add(enDesign.TextBox_Bold, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.TextBox_Bold, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Bold=True,Color=ffffff}", "");
            Design.Add(enDesign.TextBox_Bold, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15,Bold=True,Color=9d9d9d}", "");
            Design.Add(enDesign.TextBox_Bold, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Bold=True,Color=ffffff}", "");
            Design.Add(enDesign.TextBox_Bold, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.Slider_Hintergrund_Waagerecht, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Waagerecht, enStates.Standard_MouseOver_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Ohne, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Waagerecht, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F9F9F9", "", "", enRahmenArt.Ohne, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Waagerecht, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Ohne, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Waagerecht, enStates.Standard_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Ohne, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Senkrecht, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Senkrecht, enStates.Standard_MouseOver_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Ohne, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Senkrecht, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Ohne, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Senkrecht, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F9F9F9", "", "", enRahmenArt.Ohne, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Senkrecht, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Ohne, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Slider_Hintergrund_Senkrecht, enStates.Standard_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EFEFEF", "", "", enRahmenArt.Ohne, "B7B7B7", "", "", "", "");
            Design.Add(enDesign.Ribbonbar_Button, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button, enStates.Standard_HasFocus_MousePressed, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Ribbonbar_Button, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "DFDFDF", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_CheckBox, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "DFDFDF", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "81B8EF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Button_OptionButton, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Standard_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Solide_1px, "000000", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Checked_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Checked_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Solide_1px, "000000", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Checked_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "D8D8D8", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Checked_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Solide_1px, "000000", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Standard_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbon_ComboBox_Textbox, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "3399FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Caption, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Ribbonbar_Caption, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Ribbonbar_Button_Combobox, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Ribbonbar_Button_Combobox, enStates.Standard_HasFocus_MousePressed, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Ribbonbar_Button_Combobox, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Ribbonbar_Button_Combobox, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Ribbonbar_Button_Combobox, enStates.Standard_MouseOver, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Ribbonbar_Button_Combobox, enStates.Standard_HasFocus, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Ribbonbar_Button_Combobox, enStates.Standard_MouseOver_HasFocus, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.RibbonBar_Frame, enStates.Standard, enKontur.Rechteck, 1, 0, 1, 1, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "ACACAC", "", "", "{Name=Calibri, Size=10[K]15,Italic=True}", "");
            Design.Add(enDesign.RibbonBar_Frame, enStates.Standard_Disabled, enKontur.Rechteck, 1, 0, 1, 1, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "ACACAC", "", "", "{Name=Calibri, Size=10[K]15,Italic=True,Color=9d9d9d}", "");
            Design.Add(enDesign.Form_Standard, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Form_MsgBox, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Form_QuickInfo, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "4DA1B5", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Form_DesktopBenachrichtigung, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "1F1F1F", "", "", enRahmenArt.Solide_3px, "484848", "", "", "{Name=Calibri, Size=12[K]5,Color=ffffff}", "");
            Design.Add(enDesign.Form_BitteWarten, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "4DA1B5", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Form_AutoFilter, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "000000", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Form_AutoFilter, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Form_KontextMenu, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F0F0F0", "", "", enRahmenArt.Solide_1px, "000000", "", "", "", "");
            Design.Add(enDesign.Form_SelectBox_Dropdown, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "000000", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_DropdownMenu, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_DropdownMenu, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_DropdownMenu, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "5D5D5D", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_DropdownMenu, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Solide_1px, "000000", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_DropdownMenu, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Item_DropdownMenu, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_KontextMenu, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_KontextMenu, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Item_KontextMenu, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_KontextMenu_Caption, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.Item_KontextMenu_Caption, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.Item_KontextMenu_Caption, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "4DA1B5", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Standard_MouseOver_HasFocus_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "5D5D5D", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Solide_1px, "000000", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Standard_HasFocus, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_Autofilter, enStates.Standard_MouseOver_HasFocus, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Standard_MouseOver_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CCE8FF", "", "", enRahmenArt.Solide_1px, "99D1FF", "", "", "{Name=Calibri, Size=10[K]15,Underline=True}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Checked, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CCE8FF", "", "", enRahmenArt.Solide_1px, "99D1FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Checked_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "DFDFDF", "", "", enRahmenArt.Solide_1px, "B7B7B7", "", "", "{Name=Calibri, Size=10[K]15, Color=ffffff}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Checked_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CCE8FF", "", "", enRahmenArt.Solide_1px, "99D1FF", "", "", "{Name=Calibri, Size=10[K]15,Underline=True}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Standard_Disabled, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Checked_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CCE8FF", "", "", enRahmenArt.Solide_1px, "99D1FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "E5F3FF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Underline=True}", "");
            Design.Add(enDesign.Item_Listbox, enStates.Standard_MousePressed, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "CCE8FF", "", "", enRahmenArt.Solide_1px, "99D1FF", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Item_Listbox_Caption, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.Item_Listbox_Caption, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "DFDFDF", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Bold=True,Color=ffffff}", "");
            Design.Add(enDesign.Item_Listbox_Caption, enStates.Standard_MouseOver, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "BFDFFF", "", "", enRahmenArt.Solide_1px, "4DA1B5", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.Frame, enStates.Standard, enKontur.Rechteck, 0, 0, -7, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "ACACAC", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Frame, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, -7, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_1px, "ACACAC", "", "", "{Name=Calibri, Size=10[K]15, Color=9d9d9d}", "");
            Design.Add(enDesign.TabStrip_Body, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "ACACAC", "", "", "", "");
            Design.Add(enDesign.TabStrip_Body, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "ACACAC", "", "", "", "");
            Design.Add(enDesign.RibbonBar_Body, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F4F5F6", "", "", enRahmenArt.Solide_1px, "E5E4E5", "", "", "", "");
            Design.Add(enDesign.RibbonBar_Body, enStates.Standard_Disabled, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "F4F5F6", "", "", enRahmenArt.Solide_1px, "E5E4E5", "", "", "", "");
            Design.Add(enDesign.RibbonBar_Back, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 5, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.TabStrip_Back, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Progressbar, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "FFFFFF", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "", "");
            Design.Add(enDesign.Progressbar_Füller, enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "0072BC", "", "", enRahmenArt.Ohne, "", "", "", "", "");
            Design.Add(enDesign.Table_Lines_thick, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "ACACAC", "", "", "", "");
            Design.Add(enDesign.Table_Lines_thin, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "D8D8D8", "", "", "", "");
            Design.Add(enDesign.Table_Cursor, enStates.Standard, enKontur.Rechteck, -1, -1, -1, -1, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_3px, "ACACAC", "", "", "", "");
            Design.Add(enDesign.Table_Cursor, enStates.Standard_HasFocus, enKontur.Rechteck, -1, -1, -1, -1, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Solide_3px, "3399FF", "", "", "", "");
            Design.Add(enDesign.Table_Cell, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15}", "");
            Design.Add(enDesign.Table_Cell_New, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Italic=True}", "");
            Design.Add(enDesign.Table_Column, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=10[K]15,Bold=True}", "");
            Design.Add(enDesign.Table_Cell_Chapter, enStates.Standard, enKontur.Ohne, 0, 0, 0, 0, enHintergrundArt.Ohne, 0, "", "", "", enRahmenArt.Ohne, "", "", "", "{Name=Calibri, Size=15,Bold=True,Underline=True}", "");
            inited = true;
            //ColX1 = SkinDB.Column["X1"];
            //ColX2 = SkinDB.Column["X2"];
            //ColY1 = SkinDB.Column["Y1"];
            //ColY2 = SkinDB.Column["Y2"];
            //col_Color_Back_1 = SkinDB.Column["Color_Back_1"];
            //col_Color_Back_2 = SkinDB.Column["Color_Back_2"];
            ////col_Color_Back_3 = SkinDB.Column["Color_Back_3"];
            //col_Color_Border_1 = SkinDB.Column["Color_Border_1"];
            //col_Color_Border_2 = SkinDB.Column["Color_Border_2"];
            //col_Color_Border_3 = SkinDB.Column["Color_Border_3"];
            //col_Kontur = SkinDB.Column["Kontur"];
            //col_Draw_Back = SkinDB.Column["Draw_Back"];
            //col_Verlauf_Mitte = SkinDB.Column["Verlauf_Mitte"];
            //col_Border_Style = SkinDB.Column["Border_Style"];
            ////col_Status = SkinDB.Column["Status"];
            //col_Font = SkinDB.Column["Font"];
            //col_StandardPic = SkinDB.Column["StandardPic"];
            ST[0] = enImageCodeEffect.WindowsXPDisabled;
            //ST[0] = (enImageCodeEffect)int.Parse(SkinDB?.Tags[0]);
            Pen_LinieDünn = new Pen(Color_Border(enDesign.Table_Lines_thin, enStates.Standard));
            Pen_LinieKräftig = new Pen(Color_Border(enDesign.Table_Lines_thick, enStates.Standard));
            Pen_LinieDick = new Pen(Color_Border(enDesign.Table_Lines_thick, enStates.Standard), 3);
        }
        public static enImageCodeEffect AdditionalState(enStates vState) => vState.HasFlag(enStates.Standard_Disabled) ? ST[0] : enImageCodeEffect.Ohne;
        public static Color Color_Back(enDesign vDesign, enStates vState) => DesignOf(vDesign, vState).BackColor1;
        internal static Color Color_Border(enDesign vDesign, enStates vState) => DesignOf(vDesign, vState).BorderColor1;
        #region  Back 
        public static void Draw_Back(Graphics gr, enDesign design, enStates state, Rectangle r, System.Windows.Forms.Control control, bool needTransparenz) => Draw_Back(gr, DesignOf(design, state), r, control, needTransparenz);
        public static clsDesign DesignOf(enDesign design, enStates state) {
            try {
                return Design[design][state];
            } catch {
                clsDesign d = new() {
                    BackColor1 = Color.White,
                    BorderColor1 = Color.Red,
                    bFont = BlueFont.Get("Arial", 10f, false, false, false, false, false, Color.Red, Color.Black, false, false, false),
                    HintergrundArt = enHintergrundArt.Solide,
                    RahmenArt = enRahmenArt.Solide_1px,
                    Kontur = enKontur.Rechteck
                };
                return d;
            }
        }
        public static void Draw_Back(Graphics gr, clsDesign design, Rectangle r, System.Windows.Forms.Control control, bool needTransparenz) {
            try {
                if (design.Need) {
                    if (!needTransparenz) { design.Need = false; }
                    if (design.Kontur != enKontur.Ohne) {
                        if (design.HintergrundArt != enHintergrundArt.Ohne) {
                            if (design.Kontur == enKontur.Rechteck && design.X1 >= 0 && design.X2 >= 0 && design.Y1 >= 0 && design.Y2 >= 0) { design.Need = false; }
                            if (design.Kontur == enKontur.Rechteck_R4 && design.X1 >= 1 && design.X2 >= 1 && design.Y1 >= 1 && design.Y2 >= 1) { design.Need = false; }
                        }
                    }
                }
                if (design.Need) { Draw_Back_Transparent(gr, r, control); }
                if (design.HintergrundArt == enHintergrundArt.Ohne || design.Kontur == enKontur.Ohne) { return; }
                r.X -= design.X1;
                r.Y -= design.Y1;
                r.Width += design.X1 + design.X2;
                r.Height += design.Y1 + design.Y2;
                if (r.Width < 1 || r.Height < 1) { return; }// Durchaus möglich, Creative-Pad, usereingabe
                switch (design.HintergrundArt) {
                    case enHintergrundArt.Ohne:
                        break;
                    case enHintergrundArt.Solide:
                        gr.FillPath(new SolidBrush(design.BackColor1), Kontur(design.Kontur, r));
                        break;
                    case enHintergrundArt.Verlauf_Vertical_2:
                        //var c1 = Color.FromArgb(Value(row, col_Color_Back_1, 0));
                        //var c2 = Color.FromArgb(Value(row, col_Color_Back_2, 0));
                        LinearGradientBrush lgb = new(r, design.BackColor1, design.BackColor2, LinearGradientMode.Vertical);
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
                    case enHintergrundArt.Unbekannt:
                        break;
                    default:
                        Develop.DebugPrint(design.HintergrundArt);
                        break;
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }
        public static void Draw_Back_Transparent(Graphics gr, Rectangle r, System.Windows.Forms.Control control) {
            if (control?.Parent == null) { return; }
            switch (control.Parent) {
                case IUseMyBackColor _:
                    gr.FillRectangle(new SolidBrush(control.Parent.BackColor), r);
                    return;
                case IBackgroundNone _:
                    Draw_Back_Transparent(gr, r, control.Parent);
                    break;
                case GenericControl TRB:
                    if (TRB.BitmapOfControl() == null) { return; }
                    gr.DrawImage(TRB.BitmapOfControl(), r, new Rectangle(control.Left + r.Left, control.Top + r.Top, r.Width, r.Height), GraphicsUnit.Pixel);
                    break;
                case System.Windows.Forms.Form frm:
                    gr.FillRectangle(new SolidBrush(frm.BackColor), r);
                    break;
                case System.Windows.Forms.SplitContainer _:
                    Draw_Back_Transparent(gr, r, control.Parent);
                    break;
                case System.Windows.Forms.SplitterPanel _:
                    Draw_Back_Transparent(gr, r, control.Parent);
                    break;
                case System.Windows.Forms.TableLayoutPanel _:
                    Draw_Back_Transparent(gr, r, control.Parent);
                    break;
                case System.Windows.Forms.Panel _:
                    Draw_Back_Transparent(gr, r, control.Parent);
                    break;
                default:
                    System.Windows.Forms.ButtonRenderer.DrawParentBackground(gr, r, control); // Ein Versuch ist es allemal wert..
                    Develop.DebugPrint("Unbekannter Typ: " + control.Parent.Name);
                    break;
            }
        }
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
        #endregion
        private static GraphicsPath Kontur(enKontur Kon, Rectangle r) => Kon switch {
            enKontur.Rechteck => modAllgemein.Poly_Rechteck(r),// GR.SmoothingModex = Drawing2D.SmoothingMode.None
            enKontur.Rechteck_R4 => modAllgemein.Poly_RoundRec(r, 4),// GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
            enKontur.Rechteck_R11 => modAllgemein.Poly_RoundRec(r, 11),//  GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
            enKontur.Rechteck_R20 => modAllgemein.Poly_RoundRec(r, 20),//    GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
                                                                       //break; case Is = enKontur.Rechteck_R4_NurOben
                                                                       //    r.Y2 += 4
                                                                       //    GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
                                                                       //    Return Poly_RoundRec(r, 4)
            enKontur.Ohne => null,
            _ => modAllgemein.Poly_Rechteck(r),//  GR.SmoothingModex = Drawing2D.SmoothingMode.None
        };
        #region  Border 
        public static void Draw_Border(Graphics GR, enDesign vDesign, enStates vState, Rectangle r) => Draw_Border(GR, DesignOf(vDesign, vState), r);
        //[Obsolete]
        //public static void Draw_Border(Graphics GR, RowItem row, Rectangle r) {
        //    if (row == null) { return; }
        //    var d = new clsDesign();
        //    d.Kontur = (enKontur)Value(row, col_Kontur, -1);
        //    if (d.Kontur == enKontur.Ohne) { return; }
        //    d.RahmenArt = (enRahmenArt)Value(row, col_Border_Style, -1);
        //    if (d.RahmenArt == enRahmenArt.Ohne) { return; }
        //    d.X1 = Value(row, ColX1, 0);
        //    d.Y1 = Value(row, ColY1, 0);
        //    d.X2 = Value(row, ColX2, 0);
        //    d.Y2 = Value(row, ColY2, 0);
        //    d.BorderColor1 = Color.FromArgb(Value(row, col_Color_Border_1, 0));
        //    Draw_Border(GR, d, r);
        //}
        public static void Draw_Border(Graphics GR, clsDesign design, Rectangle r) {
            if (design.Kontur == enKontur.Ohne) { return; }
            if (design.RahmenArt == enRahmenArt.Ohne) { return; }
            if (design.Kontur == enKontur.Unbekannt) {
                design.Kontur = enKontur.Rechteck;
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
            GraphicsPath PathX;
            Pen PenX;
            try {
                switch (design.RahmenArt) {
                    case enRahmenArt.Solide_1px:
                        PathX = Kontur(design.Kontur, r);
                        PenX = new Pen(design.BorderColor1);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;
                    case enRahmenArt.Solide_1px_FocusDotLine:
                        PathX = Kontur(design.Kontur, r);
                        PenX = new Pen(design.BorderColor1);
                        GR.DrawPath(PenX, PathX);
                        r.Inflate(-3, -3);
                        PathX = Kontur(design.Kontur, r);
                        PenX = new Pen(design.BorderColor3) {
                            DashStyle = DashStyle.Dot
                        };
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;
                    case enRahmenArt.FocusDotLine:
                        PenX = new Pen(design.BorderColor3) {
                            DashStyle = DashStyle.Dot
                        };
                        r.Inflate(-3, -3);
                        PathX = Kontur(design.Kontur, r);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;
                    case enRahmenArt.Solide_3px:
                        PathX = Kontur(design.Kontur, r);
                        PenX = new Pen(design.BorderColor1, 3);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;
                    default:
                        PathX = Kontur(design.Kontur, r);
                        PenX = new Pen(Color.Red);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        Develop.DebugPrint(design.RahmenArt);
                        break;
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }
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
        #endregion
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
        public static void Draw_FormatedText(Graphics gr, string txt, enDesign design, enStates state, QuickImage imageCode, enAlignment align, Rectangle fitInRect, System.Windows.Forms.Control child, bool deleteBack, bool translate) => Draw_FormatedText(gr, txt, imageCode, DesignOf(design, state), align, fitInRect, child, deleteBack, translate);
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
        public static void Draw_FormatedText(Graphics gr, string txt, QuickImage qi, clsDesign design, enAlignment align, Rectangle fitInRect, System.Windows.Forms.Control child, bool deleteBack, bool translate) {
            if (string.IsNullOrEmpty(txt) && qi == null) { return; }
            QuickImage tmpImage = null;
            if (qi != null) { tmpImage = QuickImage.Get(qi, AdditionalState(design.Status)); }
            Draw_FormatedText(gr, txt, tmpImage, align, fitInRect, child, deleteBack, design.bFont, translate);
        }
        public static ItemCollectionList GetRahmenArt(RowItem SheetStyle, bool MitOhne) {
            ItemCollectionList Rahms = new();
            if (MitOhne) {
                Rahms.Add("Ohne Rahmen", ((int)PadStyles.Undefiniert).ToString(), enImageCode.Kreuz);
            }
            Rahms.Add("Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Haupt).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Haupt, SheetStyle).SymbolOfLine());
            Rahms.Add("Untertitel für Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Untertitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Untertitel, SheetStyle).SymbolOfLine());
            Rahms.Add("Überschrift für Kapitel", ((int)PadStyles.Style_Überschrift_Kapitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Kapitel, SheetStyle).SymbolOfLine());
            Rahms.Add("Standard", ((int)PadStyles.Style_Standard).ToString(), GetBlueFont(PadStyles.Style_Standard, SheetStyle).SymbolOfLine());
            Rahms.Add("Standard Fett", ((int)PadStyles.Style_StandardFett).ToString(), GetBlueFont(PadStyles.Style_StandardFett, SheetStyle).SymbolOfLine());
            Rahms.Add("Standard Alternativ-Design", ((int)PadStyles.Style_StandardAlternativ).ToString(), GetBlueFont(PadStyles.Style_StandardAlternativ, SheetStyle).SymbolOfLine());
            Rahms.Add("Kleiner Zusatz", ((int)PadStyles.Style_KleinerZusatz).ToString(), GetBlueFont(PadStyles.Style_KleinerZusatz, SheetStyle).SymbolOfLine());
            Rahms.Sort();
            return Rahms;
        }
        public static ItemCollectionList GetFonts(RowItem SheetStyle) {
            ItemCollectionList Rahms = new()
            {
                //   Rahms.Add(New ItemCollection.TextListItem(CInt(PadStyles.Undefiniert).ToString, "Ohne Rahmen", enImageCode.Kreuz))
                { "Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Haupt).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Haupt, SheetStyle).SymbolForReadableText() },
                { "Untertitel für Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Untertitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Untertitel, SheetStyle).SymbolForReadableText() },
                { "Überschrift für Kapitel", ((int)PadStyles.Style_Überschrift_Kapitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Kapitel, SheetStyle).SymbolForReadableText() },
                { "Standard", ((int)PadStyles.Style_Standard).ToString(), GetBlueFont(PadStyles.Style_Standard, SheetStyle).SymbolForReadableText() },
                { "Standard Fett", ((int)PadStyles.Style_StandardFett).ToString(), GetBlueFont(PadStyles.Style_StandardFett, SheetStyle).SymbolForReadableText() },
                { "Standard Alternativ-Design", ((int)PadStyles.Style_StandardAlternativ).ToString(), GetBlueFont(PadStyles.Style_StandardAlternativ, SheetStyle).SymbolForReadableText() },
                { "Kleiner Zusatz", ((int)PadStyles.Style_KleinerZusatz).ToString(), GetBlueFont(PadStyles.Style_KleinerZusatz, SheetStyle).SymbolForReadableText() }
            };
            Rahms.Sort();
            return Rahms;
        }
        /// <summary>
        /// Zeichnet den Text und das Bild ohne weitere Modifikation
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="txt"></param>
        /// <param name="qi"></param>
        /// <param name="vAlign"></param>
        /// <param name="FitInRect"></param>
        /// <param name="Child"></param>
        /// <param name="DeleteBack"></param>
        /// <param name="F"></param>
        public static void Draw_FormatedText(Graphics gr, string txt, QuickImage qi, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack, BlueFont F, bool Translate) {
            if (gr.TextRenderingHint != TextRenderingHint.ClearTypeGridFit) {
                gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            }
            //  GR.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit
            var pSize = SizeF.Empty;
            var tSize = SizeF.Empty;
            float XP = 0;
            float YP1 = 0;
            float YP2 = 0;
            if (qi != null) { pSize = qi.BMP.Size; }
            if (LanguageTool.Translation != null) { txt = LanguageTool.DoTranslate(txt, Translate); }
            if (F != null) {
                if (FitInRect.Width > 0) { txt = txt.TrimByWidth(FitInRect.Width - pSize.Width, F); }
                tSize = gr.MeasureString(txt, F.Font());
            }
            if (vAlign.HasFlag(enAlignment.Right)) { XP = FitInRect.Width - pSize.Width - tSize.Width; }
            if (vAlign.HasFlag(enAlignment.HorizontalCenter)) { XP = (float)((FitInRect.Width - pSize.Width - tSize.Width) / 2.0); }
            if (vAlign.HasFlag(enAlignment.VerticalCenter)) {
                YP1 = (float)((FitInRect.Height - pSize.Height) / 2.0);
                YP2 = (float)((FitInRect.Height - tSize.Height) / 2.0);
            }
            if (vAlign.HasFlag(enAlignment.Bottom)) {
                YP1 = FitInRect.Height - pSize.Height;
                YP2 = FitInRect.Height - tSize.Height;
            }
            if (DeleteBack) {
                if (!string.IsNullOrEmpty(txt)) { Draw_Back_Transparent(gr, new Rectangle((int)(FitInRect.X + pSize.Width + XP - 1), (int)(FitInRect.Y + YP2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2)), Child); }
                if (qi != null) { Draw_Back_Transparent(gr, new Rectangle((int)(FitInRect.X + XP), (int)(FitInRect.Y + YP1), (int)pSize.Width, (int)pSize.Height), Child); }
            }
            try {
                if (qi != null) { gr.DrawImage(qi.BMP, (int)(FitInRect.X + XP), (int)(FitInRect.Y + YP1)); }
                if (!string.IsNullOrEmpty(txt)) { gr.DrawString(txt, F.Font(), F.Brush_Color_Main, FitInRect.X + pSize.Width + XP, FitInRect.Y + YP2); }
            } catch (Exception) {
                // es kommt selten vor, dass das Graphics-Objekt an anderer Stelle verwendet wird. Was immer das auch heißen mag...
                //Develop.DebugPrint(ex);
            }
        }
        public static Size FormatedText_NeededSize(string text, QuickImage image, BlueFont font, int minSie) {
            try {
                var pSize = SizeF.Empty;
                var tSize = SizeF.Empty;
                if (font == null) { return new Size(3, 3); }
                if (image != null) { pSize = image.BMP.Size; }
                if (!string.IsNullOrEmpty(text)) { tSize = BlueFont.MeasureString(text, font.Font()); }
                return !string.IsNullOrEmpty(text)
                    ? image == null
                        ? new Size((int)(tSize.Width + 1), Math.Max((int)tSize.Height, minSie))
                        : new Size((int)(tSize.Width + 2 + pSize.Width + 1), Math.Max((int)tSize.Height, (int)pSize.Height))
                    : image != null ? new Size((int)pSize.Width, (int)pSize.Height) : new Size(minSie, minSie);
            } catch {
                // tmpImageCode wird an anderer Stelle verwendet
                return FormatedText_NeededSize(text, image, font, minSie);
            }
        }
        internal static BlueFont GetBlueFont(int design, enStates state, RowItem rowOfStyle, int stufe) => design > 10000 ? GetBlueFont((PadStyles)design, rowOfStyle, stufe) : GetBlueFont((enDesign)design, state, stufe);
        internal static BlueFont GetBlueFont(PadStyles padStyle, RowItem rowOfStyle, int stufe) {
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
            Develop.DebugPrint(enFehlerArt.Fehler, "Stufe " + stufe + " nicht definiert.");
            return null;
        }
        internal static BlueFont GetBlueFont(enDesign design, enStates state, int stufe) {
            if (stufe != 4 && design != enDesign.TextBox) {
                if (design == enDesign.Form_QuickInfo) { return GetBlueFont(design, state); } // QuickInfo kann jeden Text enthatlten
                Develop.DebugPrint(enFehlerArt.Warnung, "Design unbekannt: " + (int)design);
                return GetBlueFont(design, state);
            }
            switch (stufe) {
                case 4:
                    return GetBlueFont(design, state);
                case 3:
                    return GetBlueFont(enDesign.TextBox_Stufe3, state);
                case 2:
                    return GetBlueFont(enDesign.TextBox_Stufe3, state);
                case 1:
                    return GetBlueFont(enDesign.TextBox_Stufe3, state);
                case 7:
                    return GetBlueFont(enDesign.TextBox_Bold, state);
            }
            Develop.DebugPrint(enFehlerArt.Fehler, "Stufe " + stufe + " nicht definiert.");
            return GetBlueFont(design, state);
        }
        public static BlueFont GetBlueFont(PadStyles format, RowItem rowOfStyle) {
            if (StyleDB == null) { InitStyles(); }
            return StyleDB == null || rowOfStyle == null ? BlueFont.Get(ErrorFont) : GetBlueFont(StyleDB, ((int)format).ToString(), rowOfStyle);
        }
        public static BlueFont GetBlueFont(Database styleDB, string column, RowItem row) => GetBlueFont(styleDB, styleDB.Column[column], row);
        public static BlueFont GetBlueFont(Database styleDB, ColumnItem column, RowItem row) {
            var _String = styleDB.Cell.GetString(column, row);
            if (string.IsNullOrEmpty(_String)) {
                Develop.DebugPrint("Schrift nicht definiert: " + styleDB.Filename + " - " + column.Name + " - " + row.CellFirstString());
                return null;
            }
            return BlueFont.Get(_String);
        }
        public static BlueFont GetBlueFont(enDesign design, enStates state) => DesignOf(design, state).bFont;
        #region  Styles 
        public static List<string> AllStyles() {
            if (StyleDB == null) { InitStyles(); }
            return StyleDB?.Column[0].Contents(null, null);
        }
        public static void InitStyles() => StyleDB = Database.LoadResource(Assembly.GetAssembly(typeof(Skin)), "Styles.MDB", "Styles", true, false);
        #endregion
    }
}
