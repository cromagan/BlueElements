Imports BlueControls.Controls
Imports BlueElements
Imports BlueControls
Imports BlueControls.Enums
Imports BlueBasics.Enums

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmTest
    Inherits BlueControls.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.BlueButton24 = New Button()
        Me.BlueButton25 = New Button()
        Me.BlueButton22 = New Button()
        Me.BlueButton23 = New Button()
        Me.BlueFrame5 = New GroupBox()
        Me.BlueCaption7 = New Caption()
        Me.BlueButton18 = New Button()
        Me.BlueButton20 = New Button()
        Me.BlueProgressbar1 = New ProgressBar()
        Me.BlueFrame4 = New GroupBox()
        Me.BlueCaption1 = New Caption()
        Me.BlueCaption6 = New Caption()
        Me.BlueCaption5 = New Caption()
        Me.BlueFrame3 = New GroupBox()
        Me.BlueCaption10 = New Caption()
        Me.BlueCaption4 = New Caption()
        Me.BlueButton17 = New Button()
        Me.BlueButton19 = New Button()
        Me.BlueButton21 = New Button()
        Me.BlueSelectBox2 = New ComboBox()
        Me.BlueTextBox4 = New TextBox()
        Me.BlueSelectBox4 = New ComboBox()
        Me.BlueSelectBox3 = New ComboBox()
        Me.BlueListBox2 = New ListBox()
        Me.BlueListBox1 = New ListBox()
        Me.BlueButton9 = New Button()
        Me.BlueButton10 = New Button()
        Me.BlueButton11 = New Button()
        Me.BlueButton12 = New Button()
        Me.BlueButton7 = New Button()
        Me.BlueButton8 = New Button()
        Me.BlueButton6 = New Button()
        Me.BlueButton5 = New Button()
        Me.BlueTable1 = New Table()
        Me.BlueTextBox2 = New TextBox()
        Me.BlueButton4 = New Button()
        Me.BlueButton3 = New Button()
        Me.BlueCategory1 = New ListBox()
        Me.BlueTextBox1 = New TextBox()
        Me.BlueFrame2 = New GroupBox()
        Me.BlueCaption2 = New Caption()
        Me.BlueFrame1 = New GroupBox()
        Me.BlueCaption11 = New Caption()
        Me.BlueCaption3 = New Caption()
        Me.BlueButton16 = New Button()
        Me.BlueButton14 = New Button()
        Me.BlueButton1 = New Button()
        Me.BlueSelectBox1 = New ComboBox()
        Me.BlueTextBox3 = New TextBox()
        Me.But1_Body = New GroupBox()
        Me.BlueCaption12 = New Caption()
        Me.BlueButton26 = New Button()
        Me.BlueButton29 = New Button()
        Me.BlueButton30 = New Button()
        Me.BlueButton31 = New Button()
        Me.BlueCaption8 = New Caption()
        Me.BlueTabControl1 = New TabControl()
        Me.BlueTabPage1 = New TabPage()
        Me.BlueCreativePad2 = New CreativePad()
        Me.BlueCreativePad1 = New CreativePad()
        Me.BlueEasyPic1 = New EasyPic()
        Me.BlueEasyPic2 = New EasyPic()
        Me.BlueTabPage2 = New TabPage()
        Me.BlueTabPage3 = New TabPage()
        Me.BlueTabPage4 = New TabPage()
        Me.BlueButton2 = New Button()
        Me.BlueTextBox5 = New TextBox()
        Me.BlueTextBox6 = New TextBox()
        Me.BlueButton13 = New Button()
        Me.BlueSlider1 = New Slider()
        Me.BlueSlider2 = New Slider()
        Me.BlueSlider3 = New Slider()
        Me.BlueSlider4 = New Slider()
        Me.BlueButton15 = New Button()
        Me.BlueButton27 = New Button()
        Me.BlueFrame5.SuspendLayout()
        Me.BlueFrame4.SuspendLayout()
        Me.BlueFrame3.SuspendLayout()
        Me.BlueFrame2.SuspendLayout()
        Me.BlueFrame1.SuspendLayout()
        Me.But1_Body.SuspendLayout()
        Me.BlueTabControl1.SuspendLayout()
        Me.BlueTabPage1.SuspendLayout()
        Me.SuspendLayout()
        '
        'BlueButton24
        '
        Me.BlueButton24.ButtonStyle = enButtonStyle.Optionbox_Text
        Me.BlueButton24.Checked = True
        Me.BlueButton24.Enabled = False
        Me.BlueButton24.Location = New System.Drawing.Point(208, 304)
        Me.BlueButton24.Name = "BlueButton24"
        Me.BlueButton24.Size = New System.Drawing.Size(64, 16)
        Me.BlueButton24.TabIndex = 36
        Me.BlueButton24.Text = "Option"
        '
        'BlueButton25
        '
        Me.BlueButton25.ButtonStyle = enButtonStyle.Checkbox_Text
        Me.BlueButton25.Checked = True
        Me.BlueButton25.Enabled = False
        Me.BlueButton25.Location = New System.Drawing.Point(136, 304)
        Me.BlueButton25.Name = "BlueButton25"
        Me.BlueButton25.Size = New System.Drawing.Size(64, 16)
        Me.BlueButton25.TabIndex = 35
        Me.BlueButton25.Text = "Check"
        '
        'BlueButton22
        '
        Me.BlueButton22.ButtonStyle = enButtonStyle.Optionbox
        Me.BlueButton22.Checked = True
        Me.BlueButton22.Enabled = False
        Me.BlueButton22.Location = New System.Drawing.Point(208, 192)
        Me.BlueButton22.Name = "BlueButton22"
        Me.BlueButton22.Size = New System.Drawing.Size(64, 24)
        Me.BlueButton22.TabIndex = 34
        Me.BlueButton22.Text = "Option"
        '
        'BlueButton23
        '
        Me.BlueButton23.ButtonStyle = enButtonStyle.Checkbox
        Me.BlueButton23.Checked = True
        Me.BlueButton23.Enabled = False
        Me.BlueButton23.Location = New System.Drawing.Point(136, 192)
        Me.BlueButton23.Name = "BlueButton23"
        Me.BlueButton23.Size = New System.Drawing.Size(64, 24)
        Me.BlueButton23.TabIndex = 33
        Me.BlueButton23.Text = "Check"
        '
        'BlueFrame5
        '
        Me.BlueFrame5.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BlueFrame5.BackColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(233, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.BlueFrame5.CausesValidation = False
        Me.BlueFrame5.Controls.Add(Me.BlueCaption7)
        Me.BlueFrame5.Controls.Add(Me.BlueButton18)
        Me.BlueFrame5.Controls.Add(Me.BlueButton20)
        Me.BlueFrame5.Location = New System.Drawing.Point(0, 80)
        Me.BlueFrame5.Name = "BlueFrame5"
        Me.BlueFrame5.Size = New System.Drawing.Size(1498, 32)
        '
        'BlueCaption7
        '
        Me.BlueCaption7.CausesValidation = False
        Me.BlueCaption7.Enabled = False
        Me.BlueCaption7.Location = New System.Drawing.Point(7, 0)
        Me.BlueCaption7.Name = "BlueCaption7"
        Me.BlueCaption7.Size = New System.Drawing.Size(51, 31)
        Me.BlueCaption7.Text = "Disabled<br>Checked"
        '
        'BlueButton18
        '
        Me.BlueButton18.ButtonStyle = enButtonStyle.Optionbox
        Me.BlueButton18.Checked = True
        Me.BlueButton18.Enabled = False
        Me.BlueButton18.Location = New System.Drawing.Point(120, 3)
        Me.BlueButton18.Name = "BlueButton18"
        Me.BlueButton18.Size = New System.Drawing.Size(64, 26)
        Me.BlueButton18.TabIndex = 5
        Me.BlueButton18.Text = "Option"
        '
        'BlueButton20
        '
        Me.BlueButton20.ButtonStyle = enButtonStyle.Checkbox
        Me.BlueButton20.Checked = True
        Me.BlueButton20.Enabled = False
        Me.BlueButton20.Location = New System.Drawing.Point(61, 3)
        Me.BlueButton20.Name = "BlueButton20"
        Me.BlueButton20.Size = New System.Drawing.Size(56, 26)
        Me.BlueButton20.TabIndex = 3
        Me.BlueButton20.Text = "Check"
        '
        'BlueProgressbar1
        '
        Me.BlueProgressbar1.Location = New System.Drawing.Point(64, 536)
        Me.BlueProgressbar1.Name = "BlueProgressbar1"
        Me.BlueProgressbar1.Prozent = 50
        Me.BlueProgressbar1.Size = New System.Drawing.Size(144, 24)
        Me.BlueProgressbar1.TabIndex = 31
        Me.BlueProgressbar1.TabStop = False
        Me.BlueProgressbar1.Text = "BlueProgressbar1"
        '
        'BlueFrame4
        '
        Me.BlueFrame4.BackColor = System.Drawing.SystemColors.Control
        Me.BlueFrame4.CausesValidation = False
        Me.BlueFrame4.Controls.Add(Me.BlueCaption1)
        Me.BlueFrame4.Enabled = False
        Me.BlueFrame4.Location = New System.Drawing.Point(144, 328)
        Me.BlueFrame4.Name = "BlueFrame4"
        Me.BlueFrame4.Size = New System.Drawing.Size(128, 104)
        Me.BlueFrame4.Text = "BlueFrame4"
        '
        'BlueCaption1
        '
        Me.BlueCaption1.CausesValidation = False
        Me.BlueCaption1.Location = New System.Drawing.Point(16, 32)
        Me.BlueCaption1.Name = "BlueCaption1"
        Me.BlueCaption1.Size = New System.Drawing.Size(96, 24)
        Me.BlueCaption1.Text = "Caption"
        Me.BlueCaption1.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueCaption6
        '
        Me.BlueCaption6.CausesValidation = False
        Me.BlueCaption6.Location = New System.Drawing.Point(8, 160)
        Me.BlueCaption6.Name = "BlueCaption6"
        Me.BlueCaption6.Size = New System.Drawing.Size(56, 24)
        Me.BlueCaption6.Text = "Disabled"
        Me.BlueCaption6.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueCaption5
        '
        Me.BlueCaption5.CausesValidation = False
        Me.BlueCaption5.Location = New System.Drawing.Point(8, 128)
        Me.BlueCaption5.Name = "BlueCaption5"
        Me.BlueCaption5.Size = New System.Drawing.Size(56, 24)
        Me.BlueCaption5.Text = "Standard"
        Me.BlueCaption5.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueFrame3
        '
        Me.BlueFrame3.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BlueFrame3.BackColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(233, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.BlueFrame3.CausesValidation = False
        Me.BlueFrame3.Controls.Add(Me.BlueCaption10)
        Me.BlueFrame3.Controls.Add(Me.BlueCaption4)
        Me.BlueFrame3.Controls.Add(Me.BlueButton17)
        Me.BlueFrame3.Controls.Add(Me.BlueButton19)
        Me.BlueFrame3.Controls.Add(Me.BlueButton21)
        Me.BlueFrame3.Controls.Add(Me.BlueSelectBox2)
        Me.BlueFrame3.Controls.Add(Me.BlueTextBox4)
        Me.BlueFrame3.Location = New System.Drawing.Point(0, 40)
        Me.BlueFrame3.Name = "BlueFrame3"
        Me.BlueFrame3.Size = New System.Drawing.Size(1498, 40)
        '
        'BlueCaption10
        '
        Me.BlueCaption10.CausesValidation = False
        Me.BlueCaption10.Location = New System.Drawing.Point(440, 12)
        Me.BlueCaption10.Name = "BlueCaption10"
        Me.BlueCaption10.Size = New System.Drawing.Size(96, 16)
        Me.BlueCaption10.Text = "Caption"
        Me.BlueCaption10.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueCaption4
        '
        Me.BlueCaption4.CausesValidation = False
        Me.BlueCaption4.Enabled = False
        Me.BlueCaption4.Location = New System.Drawing.Point(7, 12)
        Me.BlueCaption4.Name = "BlueCaption4"
        Me.BlueCaption4.Size = New System.Drawing.Size(50, 16)
        Me.BlueCaption4.Text = "Disabled"
        '
        'BlueButton17
        '
        Me.BlueButton17.ButtonStyle = enButtonStyle.Optionbox
        Me.BlueButton17.Enabled = False
        Me.BlueButton17.Location = New System.Drawing.Point(178, 3)
        Me.BlueButton17.Name = "BlueButton17"
        Me.BlueButton17.Size = New System.Drawing.Size(64, 34)
        Me.BlueButton17.TabIndex = 5
        Me.BlueButton17.Text = "Option"
        '
        'BlueButton19
        '
        Me.BlueButton19.ButtonStyle = enButtonStyle.Checkbox
        Me.BlueButton19.Enabled = False
        Me.BlueButton19.Location = New System.Drawing.Point(119, 3)
        Me.BlueButton19.Name = "BlueButton19"
        Me.BlueButton19.Size = New System.Drawing.Size(56, 34)
        Me.BlueButton19.TabIndex = 3
        Me.BlueButton19.Text = "Check"
        '
        'BlueButton21
        '
        Me.BlueButton21.Enabled = False
        Me.BlueButton21.Location = New System.Drawing.Point(60, 3)
        Me.BlueButton21.Name = "BlueButton21"
        Me.BlueButton21.Size = New System.Drawing.Size(56, 34)
        Me.BlueButton21.TabIndex = 1
        Me.BlueButton21.Text = "Button"
        '
        'BlueSelectBox2
        '
        Me.BlueSelectBox2.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueSelectBox2.Enabled = False
        Me.BlueSelectBox2.Format = enDataFormat.Text
        Me.BlueSelectBox2.Location = New System.Drawing.Point(245, 3)
        Me.BlueSelectBox2.MultiLine = True
        Me.BlueSelectBox2.Name = "BlueSelectBox2"
        Me.BlueSelectBox2.Size = New System.Drawing.Size(99, 34)
        Me.BlueSelectBox2.TabIndex = 14
        Me.BlueSelectBox2.Text = "BlueSelectBox2"
        '
        'BlueTextBox4
        '
        Me.BlueTextBox4.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueTextBox4.Enabled = False
        Me.BlueTextBox4.Location = New System.Drawing.Point(347, 8)
        Me.BlueTextBox4.Name = "BlueTextBox4"
        Me.BlueTextBox4.Size = New System.Drawing.Size(88, 24)
        Me.BlueTextBox4.TabIndex = 10
        Me.BlueTextBox4.Text = "BlueTextBox2"
        '
        'BlueSelectBox4
        '
        Me.BlueSelectBox4.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueSelectBox4.Enabled = False
        Me.BlueSelectBox4.Format = enDataFormat.Text
        Me.BlueSelectBox4.Location = New System.Drawing.Point(280, 160)
        Me.BlueSelectBox4.MultiLine = True
        Me.BlueSelectBox4.Name = "BlueSelectBox4"
        Me.BlueSelectBox4.Size = New System.Drawing.Size(88, 24)
        Me.BlueSelectBox4.TabIndex = 14
        Me.BlueSelectBox4.Text = "BlueSelectBox2"
        '
        'BlueSelectBox3
        '
        Me.BlueSelectBox3.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueSelectBox3.Format = enDataFormat.Text
        Me.BlueSelectBox3.Location = New System.Drawing.Point(280, 128)
        Me.BlueSelectBox3.MultiLine = True
        Me.BlueSelectBox3.Name = "BlueSelectBox3"
        Me.BlueSelectBox3.Size = New System.Drawing.Size(88, 24)
        Me.BlueSelectBox3.TabIndex = 4
        Me.BlueSelectBox3.Text = "BlueSelectBox1"
        '
        'BlueListBox2
        '
        Me.BlueListBox2.AddAllowed = enAddType.Text
        Me.BlueListBox2.Enabled = False
        Me.BlueListBox2.Location = New System.Drawing.Point(424, 336)
        Me.BlueListBox2.Name = "BlueListBox2"
        Me.BlueListBox2.QuickInfo = ""
        Me.BlueListBox2.Size = New System.Drawing.Size(136, 112)
        Me.BlueListBox2.TabIndex = 24
        Me.BlueListBox2.Text = "BlueListBox2"
        '
        'BlueListBox1
        '
        Me.BlueListBox1.AddAllowed = enAddType.Text
        Me.BlueListBox1.Location = New System.Drawing.Point(424, 208)
        Me.BlueListBox1.Name = "BlueListBox1"
        Me.BlueListBox1.QuickInfo = ""
        Me.BlueListBox1.Size = New System.Drawing.Size(136, 112)
        Me.BlueListBox1.TabIndex = 23
        Me.BlueListBox1.Text = "BlueListBox1"
        '
        'BlueButton9
        '
        Me.BlueButton9.ButtonStyle = enButtonStyle.Optionbox_Text
        Me.BlueButton9.Enabled = False
        Me.BlueButton9.Location = New System.Drawing.Point(208, 280)
        Me.BlueButton9.Name = "BlueButton9"
        Me.BlueButton9.Size = New System.Drawing.Size(64, 16)
        Me.BlueButton9.TabIndex = 22
        Me.BlueButton9.Text = "Option"
        '
        'BlueButton10
        '
        Me.BlueButton10.ButtonStyle = enButtonStyle.Checkbox_Text
        Me.BlueButton10.Enabled = False
        Me.BlueButton10.Location = New System.Drawing.Point(136, 280)
        Me.BlueButton10.Name = "BlueButton10"
        Me.BlueButton10.Size = New System.Drawing.Size(64, 16)
        Me.BlueButton10.TabIndex = 21
        Me.BlueButton10.Text = "Check"
        '
        'BlueButton11
        '
        Me.BlueButton11.ButtonStyle = enButtonStyle.Optionbox_Text
        Me.BlueButton11.Location = New System.Drawing.Point(208, 256)
        Me.BlueButton11.Name = "BlueButton11"
        Me.BlueButton11.Size = New System.Drawing.Size(64, 16)
        Me.BlueButton11.TabIndex = 20
        Me.BlueButton11.Text = "Option"
        '
        'BlueButton12
        '
        Me.BlueButton12.ButtonStyle = enButtonStyle.Checkbox_Text
        Me.BlueButton12.Location = New System.Drawing.Point(136, 256)
        Me.BlueButton12.Name = "BlueButton12"
        Me.BlueButton12.Size = New System.Drawing.Size(64, 16)
        Me.BlueButton12.TabIndex = 19
        Me.BlueButton12.Text = "Check"
        '
        'BlueButton7
        '
        Me.BlueButton7.ButtonStyle = enButtonStyle.Optionbox
        Me.BlueButton7.Enabled = False
        Me.BlueButton7.Location = New System.Drawing.Point(208, 160)
        Me.BlueButton7.Name = "BlueButton7"
        Me.BlueButton7.Size = New System.Drawing.Size(64, 24)
        Me.BlueButton7.TabIndex = 18
        Me.BlueButton7.Text = "Option"
        '
        'BlueButton8
        '
        Me.BlueButton8.ButtonStyle = enButtonStyle.Checkbox
        Me.BlueButton8.Enabled = False
        Me.BlueButton8.Location = New System.Drawing.Point(136, 160)
        Me.BlueButton8.Name = "BlueButton8"
        Me.BlueButton8.Size = New System.Drawing.Size(64, 24)
        Me.BlueButton8.TabIndex = 17
        Me.BlueButton8.Text = "Check"
        '
        'BlueButton6
        '
        Me.BlueButton6.ButtonStyle = enButtonStyle.Optionbox
        Me.BlueButton6.Location = New System.Drawing.Point(208, 128)
        Me.BlueButton6.Name = "BlueButton6"
        Me.BlueButton6.Size = New System.Drawing.Size(32, 24)
        Me.BlueButton6.TabIndex = 16
        Me.BlueButton6.Text = "Option"
        '
        'BlueButton5
        '
        Me.BlueButton5.ButtonStyle = enButtonStyle.Checkbox
        Me.BlueButton5.Location = New System.Drawing.Point(136, 128)
        Me.BlueButton5.Name = "BlueButton5"
        Me.BlueButton5.Size = New System.Drawing.Size(64, 24)
        Me.BlueButton5.TabIndex = 15
        Me.BlueButton5.Text = "Check"
        '
        'BlueTable1
        '
        Me.BlueTable1.Location = New System.Drawing.Point(280, 328)
        Me.BlueTable1.Name = "BlueTable1"
        Me.BlueTable1.Size = New System.Drawing.Size(80, 80)
        Me.BlueTable1.TabIndex = 13
        Me.BlueTable1.Text = "BlueTable1"
        '
        'BlueTextBox2
        '
        Me.BlueTextBox2.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueTextBox2.Enabled = False
        Me.BlueTextBox2.Location = New System.Drawing.Point(376, 160)
        Me.BlueTextBox2.Name = "BlueTextBox2"
        Me.BlueTextBox2.Size = New System.Drawing.Size(88, 24)
        Me.BlueTextBox2.TabIndex = 10
        Me.BlueTextBox2.Text = "BlueTextBox2"
        '
        'BlueButton4
        '
        Me.BlueButton4.Enabled = False
        Me.BlueButton4.Location = New System.Drawing.Point(72, 160)
        Me.BlueButton4.Name = "BlueButton4"
        Me.BlueButton4.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton4.TabIndex = 9
        Me.BlueButton4.Text = "Button"
        '
        'BlueButton3
        '
        Me.BlueButton3.Location = New System.Drawing.Point(72, 128)
        Me.BlueButton3.Name = "BlueButton3"
        Me.BlueButton3.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton3.TabIndex = 8
        Me.BlueButton3.Text = "Button"
        '
        'BlueCategory1
        '
        Me.BlueCategory1.AddAllowed = enAddType.Text
        Me.BlueCategory1.CheckBehavior = enCheckBehavior.MultiSelection
        Me.BlueCategory1.Location = New System.Drawing.Point(8, 440)
        Me.BlueCategory1.Name = "BlueCategory1"
        Me.BlueCategory1.QuickInfo = ""
        Me.BlueCategory1.Size = New System.Drawing.Size(136, 80)
        Me.BlueCategory1.TabIndex = 7
        Me.BlueCategory1.Text = "BlueCategory1"
        '
        'BlueTextBox1
        '
        Me.BlueTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueTextBox1.Location = New System.Drawing.Point(376, 128)
        Me.BlueTextBox1.Name = "BlueTextBox1"
        Me.BlueTextBox1.Size = New System.Drawing.Size(88, 24)
        Me.BlueTextBox1.TabIndex = 6
        Me.BlueTextBox1.Text = "BlueTextBox1"
        '
        'BlueFrame2
        '
        Me.BlueFrame2.BackColor = System.Drawing.SystemColors.Control
        Me.BlueFrame2.CausesValidation = False
        Me.BlueFrame2.Controls.Add(Me.BlueCaption2)
        Me.BlueFrame2.Location = New System.Drawing.Point(8, 328)
        Me.BlueFrame2.Name = "BlueFrame2"
        Me.BlueFrame2.Size = New System.Drawing.Size(128, 104)
        Me.BlueFrame2.Text = "BlueFrame2"
        '
        'BlueCaption2
        '
        Me.BlueCaption2.CausesValidation = False
        Me.BlueCaption2.Location = New System.Drawing.Point(8, 32)
        Me.BlueCaption2.Name = "BlueCaption2"
        Me.BlueCaption2.Size = New System.Drawing.Size(96, 24)
        Me.BlueCaption2.Text = "Caption"
        Me.BlueCaption2.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueFrame1
        '
        Me.BlueFrame1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BlueFrame1.BackColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(233, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.BlueFrame1.CausesValidation = False
        Me.BlueFrame1.Controls.Add(Me.BlueCaption11)
        Me.BlueFrame1.Controls.Add(Me.BlueCaption3)
        Me.BlueFrame1.Controls.Add(Me.BlueButton16)
        Me.BlueFrame1.Controls.Add(Me.BlueButton14)
        Me.BlueFrame1.Controls.Add(Me.BlueButton1)
        Me.BlueFrame1.Controls.Add(Me.BlueSelectBox1)
        Me.BlueFrame1.Controls.Add(Me.BlueTextBox3)
        Me.BlueFrame1.Location = New System.Drawing.Point(0, 0)
        Me.BlueFrame1.Name = "BlueFrame1"
        Me.BlueFrame1.Size = New System.Drawing.Size(1498, 40)
        '
        'BlueCaption11
        '
        Me.BlueCaption11.CausesValidation = False
        Me.BlueCaption11.Location = New System.Drawing.Point(436, 12)
        Me.BlueCaption11.Name = "BlueCaption11"
        Me.BlueCaption11.Size = New System.Drawing.Size(96, 16)
        Me.BlueCaption11.Text = "Caption"
        Me.BlueCaption11.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueCaption3
        '
        Me.BlueCaption3.CausesValidation = False
        Me.BlueCaption3.Location = New System.Drawing.Point(7, 12)
        Me.BlueCaption3.Name = "BlueCaption3"
        Me.BlueCaption3.Size = New System.Drawing.Size(51, 16)
        Me.BlueCaption3.Text = "Standard"
        '
        'BlueButton16
        '
        Me.BlueButton16.ButtonStyle = enButtonStyle.Optionbox
        Me.BlueButton16.Location = New System.Drawing.Point(179, 3)
        Me.BlueButton16.Name = "BlueButton16"
        Me.BlueButton16.Size = New System.Drawing.Size(64, 34)
        Me.BlueButton16.TabIndex = 4
        Me.BlueButton16.Text = "Option"
        '
        'BlueButton14
        '
        Me.BlueButton14.ButtonStyle = enButtonStyle.Checkbox
        Me.BlueButton14.Location = New System.Drawing.Point(120, 3)
        Me.BlueButton14.Name = "BlueButton14"
        Me.BlueButton14.Size = New System.Drawing.Size(56, 34)
        Me.BlueButton14.TabIndex = 2
        Me.BlueButton14.Text = "Check"
        '
        'BlueButton1
        '
        Me.BlueButton1.Location = New System.Drawing.Point(61, 3)
        Me.BlueButton1.Name = "BlueButton1"
        Me.BlueButton1.Size = New System.Drawing.Size(56, 34)
        Me.BlueButton1.TabIndex = 0
        Me.BlueButton1.Text = "Button"
        '
        'BlueSelectBox1
        '
        Me.BlueSelectBox1.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueSelectBox1.Format = enDataFormat.Text
        Me.BlueSelectBox1.Location = New System.Drawing.Point(246, 3)
        Me.BlueSelectBox1.MultiLine = True
        Me.BlueSelectBox1.Name = "BlueSelectBox1"
        Me.BlueSelectBox1.Size = New System.Drawing.Size(96, 34)
        Me.BlueSelectBox1.TabIndex = 4
        Me.BlueSelectBox1.Text = "BlueSelectBox1"
        '
        'BlueTextBox3
        '
        Me.BlueTextBox3.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueTextBox3.Location = New System.Drawing.Point(345, 8)
        Me.BlueTextBox3.Name = "BlueTextBox3"
        Me.BlueTextBox3.Size = New System.Drawing.Size(88, 24)
        Me.BlueTextBox3.TabIndex = 6
        Me.BlueTextBox3.Text = "BlueTextBox1"
        '
        'But1_Body
        '
        Me.But1_Body.BackColor = System.Drawing.SystemColors.Control
        Me.But1_Body.CausesValidation = False
        Me.But1_Body.Controls.Add(Me.BlueCaption12)
        Me.But1_Body.Location = New System.Drawing.Point(20, 32)
        Me.But1_Body.Name = "But1_Body"
        Me.But1_Body.Size = New System.Drawing.Size(208, 120)
        Me.But1_Body.Text = "BlueFrame2"
        '
        'BlueCaption12
        '
        Me.BlueCaption12.CausesValidation = False
        Me.BlueCaption12.Location = New System.Drawing.Point(8, 24)
        Me.BlueCaption12.Name = "BlueCaption12"
        Me.BlueCaption12.Size = New System.Drawing.Size(96, 16)
        Me.BlueCaption12.Text = "Caption"
        Me.BlueCaption12.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueButton26
        '
        Me.BlueButton26.Location = New System.Drawing.Point(600, 528)
        Me.BlueButton26.Name = "BlueButton26"
        Me.BlueButton26.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton26.TabIndex = 37
        Me.BlueButton26.Text = "Klick!"
        '
        'BlueButton29
        '
        Me.BlueButton29.Location = New System.Drawing.Point(664, 528)
        Me.BlueButton29.Name = "BlueButton29"
        Me.BlueButton29.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton29.TabIndex = 38
        Me.BlueButton29.Text = "Klick!"
        '
        'BlueButton30
        '
        Me.BlueButton30.Location = New System.Drawing.Point(728, 528)
        Me.BlueButton30.Name = "BlueButton30"
        Me.BlueButton30.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton30.TabIndex = 39
        Me.BlueButton30.Text = "Klick!"
        '
        'BlueButton31
        '
        Me.BlueButton31.ButtonStyle = enButtonStyle.Optionbox_Text
        Me.BlueButton31.Location = New System.Drawing.Point(208, 232)
        Me.BlueButton31.Name = "BlueButton31"
        Me.BlueButton31.Size = New System.Drawing.Size(64, 16)
        Me.BlueButton31.TabIndex = 20
        Me.BlueButton31.Text = "Option"
        '
        'BlueCaption8
        '
        Me.BlueCaption8.CausesValidation = False
        Me.BlueCaption8.Location = New System.Drawing.Point(8, 192)
        Me.BlueCaption8.Name = "BlueCaption8"
        Me.BlueCaption8.Size = New System.Drawing.Size(56, 32)
        Me.BlueCaption8.Text = "Disabled<br>Checked"
        Me.BlueCaption8.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'BlueTabControl1
        '
        Me.BlueTabControl1.Controls.Add(Me.BlueTabPage1)
        Me.BlueTabControl1.Controls.Add(Me.BlueTabPage2)
        Me.BlueTabControl1.Controls.Add(Me.BlueTabPage3)
        Me.BlueTabControl1.Controls.Add(Me.BlueTabPage4)
        Me.BlueTabControl1.HotTrack = True
        Me.BlueTabControl1.Location = New System.Drawing.Point(576, 144)
        Me.BlueTabControl1.Name = "BlueTabControl1"
        Me.BlueTabControl1.Size = New System.Drawing.Size(912, 336)
        Me.BlueTabControl1.TabIndex = 41
        '
        'BlueTabPage1
        '
        Me.BlueTabPage1.Controls.Add(Me.BlueCreativePad2)
        Me.BlueTabPage1.Controls.Add(Me.BlueCreativePad1)
        Me.BlueTabPage1.Controls.Add(Me.BlueEasyPic1)
        Me.BlueTabPage1.Controls.Add(Me.BlueEasyPic2)
        Me.BlueTabPage1.Controls.Add(Me.But1_Body)
        Me.BlueTabPage1.Location = New System.Drawing.Point(4, 25)
        Me.BlueTabPage1.Name = "BlueTabPage1"
        Me.BlueTabPage1.Size = New System.Drawing.Size(904, 307)
        Me.BlueTabPage1.TabIndex = 0
        Me.BlueTabPage1.Text = "BlueTabPage1"
        '
        'BlueCreativePad2
        '
        Me.BlueCreativePad2.Enabled = False
        Me.BlueCreativePad2.GridShow = 10.0!
        Me.BlueCreativePad2.GridSnap = 1.0!
        Me.BlueCreativePad2.Location = New System.Drawing.Point(240, 40)
        Me.BlueCreativePad2.Name = "BlueCreativePad2"
        Me.BlueCreativePad2.RandinMM = New System.Windows.Forms.Padding(0)
        Me.BlueCreativePad2.SheetSizeInMM = New System.Drawing.SizeF(0!, 0!)
        Me.BlueCreativePad2.SheetStyle = "Lysithea"
        Me.BlueCreativePad2.Size = New System.Drawing.Size(176, 104)
        Me.BlueCreativePad2.TabIndex = 50
        Me.BlueCreativePad2.Text = "BlueCreativePad2"
        '
        'BlueCreativePad1
        '
        Me.BlueCreativePad1.GridShow = 10.0!
        Me.BlueCreativePad1.GridSnap = 1.0!
        Me.BlueCreativePad1.Location = New System.Drawing.Point(432, 40)
        Me.BlueCreativePad1.Name = "BlueCreativePad1"
        Me.BlueCreativePad1.RandinMM = New System.Windows.Forms.Padding(0)
        Me.BlueCreativePad1.SheetSizeInMM = New System.Drawing.SizeF(0!, 0!)
        Me.BlueCreativePad1.SheetStyle = "Lysithea"
        Me.BlueCreativePad1.Size = New System.Drawing.Size(464, 256)
        Me.BlueCreativePad1.TabIndex = 49
        Me.BlueCreativePad1.Text = "BlueCreativePad1"
        '
        'BlueEasyPic1
        '
        Me.BlueEasyPic1.CausesValidation = False
        Me.BlueEasyPic1.Location = New System.Drawing.Point(24, 200)
        Me.BlueEasyPic1.Name = "BlueEasyPic1"
        Me.BlueEasyPic1.Size = New System.Drawing.Size(100, 80)
        Me.BlueEasyPic1.Text = "BlueEasyPic1"
        '
        'BlueEasyPic2
        '
        Me.BlueEasyPic2.CausesValidation = False
        Me.BlueEasyPic2.Enabled = False
        Me.BlueEasyPic2.Location = New System.Drawing.Point(136, 200)
        Me.BlueEasyPic2.Name = "BlueEasyPic2"
        Me.BlueEasyPic2.Size = New System.Drawing.Size(96, 80)
        Me.BlueEasyPic2.Text = "BlueEasyPic2"
        '
        'BlueTabPage2
        '
        Me.BlueTabPage2.Location = New System.Drawing.Point(4, 25)
        Me.BlueTabPage2.Name = "BlueTabPage2"
        Me.BlueTabPage2.Size = New System.Drawing.Size(904, 307)
        Me.BlueTabPage2.TabIndex = 1
        Me.BlueTabPage2.Text = "BlueTabPage2"
        '
        'BlueTabPage3
        '
        Me.BlueTabPage3.Location = New System.Drawing.Point(4, 25)
        Me.BlueTabPage3.Name = "BlueTabPage3"
        Me.BlueTabPage3.Size = New System.Drawing.Size(904, 307)
        Me.BlueTabPage3.TabIndex = 2
        Me.BlueTabPage3.Text = "BlueTabPage3"
        '
        'BlueTabPage4
        '
        Me.BlueTabPage4.Location = New System.Drawing.Point(4, 25)
        Me.BlueTabPage4.Name = "BlueTabPage4"
        Me.BlueTabPage4.Size = New System.Drawing.Size(904, 307)
        Me.BlueTabPage4.TabIndex = 3
        Me.BlueTabPage4.Text = "BlueTabPage4"
        '
        'BlueButton2
        '
        Me.BlueButton2.Location = New System.Drawing.Point(792, 528)
        Me.BlueButton2.Name = "BlueButton2"
        Me.BlueButton2.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton2.TabIndex = 42
        Me.BlueButton2.Text = "Klick!"
        '
        'BlueTextBox5
        '
        Me.BlueTextBox5.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueTextBox5.Format = enDataFormat.Text
        Me.BlueTextBox5.Location = New System.Drawing.Point(472, 128)
        Me.BlueTextBox5.Name = "BlueTextBox5"
        Me.BlueTextBox5.Size = New System.Drawing.Size(88, 24)
        Me.BlueTextBox5.TabIndex = 6
        Me.BlueTextBox5.Text = "BlueTextBox1"
        '
        'BlueTextBox6
        '
        Me.BlueTextBox6.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.BlueTextBox6.Enabled = False
        Me.BlueTextBox6.Format = enDataFormat.Text
        Me.BlueTextBox6.Location = New System.Drawing.Point(472, 160)
        Me.BlueTextBox6.Name = "BlueTextBox6"
        Me.BlueTextBox6.Size = New System.Drawing.Size(88, 24)
        Me.BlueTextBox6.TabIndex = 10
        Me.BlueTextBox6.Text = "BlueTextBox2"
        '
        'BlueButton13
        '
        Me.BlueButton13.ButtonStyle = enButtonStyle.Optionbox
        Me.BlueButton13.Location = New System.Drawing.Point(240, 128)
        Me.BlueButton13.Name = "BlueButton13"
        Me.BlueButton13.Size = New System.Drawing.Size(32, 24)
        Me.BlueButton13.TabIndex = 43
        Me.BlueButton13.Text = "Option"
        '
        'BlueSlider1
        '
        Me.BlueSlider1.CausesValidation = False
        Me.BlueSlider1.Location = New System.Drawing.Point(176, 448)
        Me.BlueSlider1.Name = "BlueSlider1"
        Me.BlueSlider1.Size = New System.Drawing.Size(200, 24)
        Me.BlueSlider1.Text = "BlueSlider1"
        '
        'BlueSlider2
        '
        Me.BlueSlider2.CausesValidation = False
        Me.BlueSlider2.Enabled = False
        Me.BlueSlider2.Location = New System.Drawing.Point(176, 472)
        Me.BlueSlider2.Name = "BlueSlider2"
        Me.BlueSlider2.Size = New System.Drawing.Size(200, 24)
        Me.BlueSlider2.Text = "BlueSlider2"
        '
        'BlueSlider3
        '
        Me.BlueSlider3.CausesValidation = False
        Me.BlueSlider3.Enabled = False
        Me.BlueSlider3.Location = New System.Drawing.Point(400, 336)
        Me.BlueSlider3.Name = "BlueSlider3"
        Me.BlueSlider3.Orientation = enOrientation.Senkrecht
        Me.BlueSlider3.Size = New System.Drawing.Size(24, 160)
        Me.BlueSlider3.Text = "BlueSlider3"
        '
        'BlueSlider4
        '
        Me.BlueSlider4.CausesValidation = False
        Me.BlueSlider4.Location = New System.Drawing.Point(376, 336)
        Me.BlueSlider4.Name = "BlueSlider4"
        Me.BlueSlider4.Orientation = enOrientation.Senkrecht
        Me.BlueSlider4.Size = New System.Drawing.Size(24, 160)
        Me.BlueSlider4.Text = "BlueSlider4"
        '
        'BlueButton15
        '
        Me.BlueButton15.Location = New System.Drawing.Point(536, 528)
        Me.BlueButton15.Name = "BlueButton15"
        Me.BlueButton15.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton15.TabIndex = 47
        Me.BlueButton15.Text = "Klick!"
        '
        'BlueButton27
        '
        Me.BlueButton27.Location = New System.Drawing.Point(856, 528)
        Me.BlueButton27.Name = "BlueButton27"
        Me.BlueButton27.Size = New System.Drawing.Size(56, 24)
        Me.BlueButton27.TabIndex = 48
        Me.BlueButton27.Text = "Klick!"
        '
        'frmTest
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1498, 562)
        Me.Controls.Add(Me.BlueButton27)
        Me.Controls.Add(Me.BlueButton15)
        Me.Controls.Add(Me.BlueSlider3)
        Me.Controls.Add(Me.BlueSlider4)
        Me.Controls.Add(Me.BlueSlider2)
        Me.Controls.Add(Me.BlueSlider1)
        Me.Controls.Add(Me.BlueButton13)
        Me.Controls.Add(Me.BlueButton2)
        Me.Controls.Add(Me.BlueTabControl1)
        Me.Controls.Add(Me.BlueCaption8)
        Me.Controls.Add(Me.BlueButton30)
        Me.Controls.Add(Me.BlueButton29)
        Me.Controls.Add(Me.BlueButton26)
        Me.Controls.Add(Me.BlueButton24)
        Me.Controls.Add(Me.BlueButton25)
        Me.Controls.Add(Me.BlueButton22)
        Me.Controls.Add(Me.BlueButton23)
        Me.Controls.Add(Me.BlueFrame5)
        Me.Controls.Add(Me.BlueProgressbar1)
        Me.Controls.Add(Me.BlueFrame4)
        Me.Controls.Add(Me.BlueCaption6)
        Me.Controls.Add(Me.BlueCaption5)
        Me.Controls.Add(Me.BlueFrame3)
        Me.Controls.Add(Me.BlueSelectBox4)
        Me.Controls.Add(Me.BlueSelectBox3)
        Me.Controls.Add(Me.BlueListBox2)
        Me.Controls.Add(Me.BlueListBox1)
        Me.Controls.Add(Me.BlueButton9)
        Me.Controls.Add(Me.BlueButton10)
        Me.Controls.Add(Me.BlueButton31)
        Me.Controls.Add(Me.BlueButton11)
        Me.Controls.Add(Me.BlueButton12)
        Me.Controls.Add(Me.BlueButton7)
        Me.Controls.Add(Me.BlueButton8)
        Me.Controls.Add(Me.BlueButton6)
        Me.Controls.Add(Me.BlueButton5)
        Me.Controls.Add(Me.BlueTable1)
        Me.Controls.Add(Me.BlueTextBox6)
        Me.Controls.Add(Me.BlueTextBox2)
        Me.Controls.Add(Me.BlueButton4)
        Me.Controls.Add(Me.BlueButton3)
        Me.Controls.Add(Me.BlueTextBox5)
        Me.Controls.Add(Me.BlueCategory1)
        Me.Controls.Add(Me.BlueTextBox1)
        Me.Controls.Add(Me.BlueFrame2)
        Me.Controls.Add(Me.BlueFrame1)
        Me.Name = "frmTest"
        Me.Text = "frmTest"
        Me.BlueFrame5.ResumeLayout(False)
        Me.BlueFrame4.ResumeLayout(False)
        Me.BlueFrame3.ResumeLayout(False)
        Me.BlueFrame2.ResumeLayout(False)
        Me.BlueFrame1.ResumeLayout(False)
        Me.But1_Body.ResumeLayout(False)
        Me.BlueTabControl1.ResumeLayout(False)
        Me.BlueTabPage1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents BlueButton1 As Button
    Friend WithEvents BlueFrame1 As GroupBox
    Friend WithEvents BlueFrame2 As GroupBox
    Friend WithEvents BlueSelectBox1 As ComboBox
    Friend WithEvents BlueTextBox1 As TextBox
    Friend WithEvents BlueCategory1 As ListBox
    Friend WithEvents BlueButton3 As Button
    Friend WithEvents BlueButton4 As Button
    Friend WithEvents BlueTextBox2 As TextBox
    Friend WithEvents BlueCaption2 As Caption
    Friend WithEvents BlueTable1 As Table
    Friend WithEvents BlueSelectBox2 As ComboBox
    Friend WithEvents BlueButton5 As Button
    Friend WithEvents BlueButton6 As Button
    Friend WithEvents BlueButton7 As Button
    Friend WithEvents BlueButton8 As Button
    Friend WithEvents BlueButton9 As Button
    Friend WithEvents BlueButton10 As Button
    Friend WithEvents BlueButton11 As Button
    Friend WithEvents BlueButton12 As Button
    Friend WithEvents BlueButton16 As Button
    Friend WithEvents BlueButton14 As Button
    Friend WithEvents BlueListBox1 As ListBox
    Friend WithEvents BlueListBox2 As ListBox
    Friend WithEvents BlueCaption3 As Caption
    Friend WithEvents BlueFrame3 As GroupBox
    Friend WithEvents BlueCaption4 As Caption
    Friend WithEvents BlueButton17 As Button
    Friend WithEvents BlueButton19 As Button
    Friend WithEvents BlueButton21 As Button
    Friend WithEvents BlueCaption5 As Caption
    Friend WithEvents BlueCaption6 As Caption
    Friend WithEvents BlueSelectBox3 As ComboBox
    Friend WithEvents BlueSelectBox4 As ComboBox
    Friend WithEvents BlueTextBox3 As TextBox
    Friend WithEvents BlueTextBox4 As TextBox
    Friend WithEvents BlueFrame4 As GroupBox
    Friend WithEvents BlueProgressbar1 As ProgressBar
    Friend WithEvents BlueFrame5 As GroupBox
    Friend WithEvents BlueCaption7 As Caption
    Friend WithEvents BlueButton18 As Button
    Friend WithEvents BlueButton20 As Button
    Friend WithEvents BlueButton22 As Button
    Friend WithEvents BlueButton23 As Button
    Friend WithEvents BlueButton24 As Button
    Friend WithEvents BlueButton25 As Button
    Friend WithEvents But1_Body As GroupBox
    Friend WithEvents BlueCaption1 As Caption
    Friend WithEvents BlueCaption10 As Caption
    Friend WithEvents BlueCaption11 As Caption
    Friend WithEvents BlueCaption12 As Caption
    Friend WithEvents BlueButton26 As Button
    Friend WithEvents BlueButton29 As Button
    Friend WithEvents BlueButton30 As Button
    Friend WithEvents BlueButton31 As Button
    Friend WithEvents BlueCaption8 As Caption
    Friend WithEvents BlueTabControl1 As TabControl
    Friend WithEvents BlueTabPage1 As TabPage
    Friend WithEvents BlueTabPage2 As TabPage
    Friend WithEvents BlueTabPage3 As TabPage
    Friend WithEvents BlueTabPage4 As TabPage
    Friend WithEvents BlueButton2 As Button
    Friend WithEvents BlueTextBox5 As TextBox
    Friend WithEvents BlueTextBox6 As TextBox
    Friend WithEvents BlueButton13 As Button
    Friend WithEvents BlueSlider1 As Slider
    Friend WithEvents BlueSlider2 As Slider
    Friend WithEvents BlueSlider3 As Slider
    Friend WithEvents BlueSlider4 As Slider
    Friend WithEvents BlueEasyPic1 As EasyPic
    Friend WithEvents BlueEasyPic2 As EasyPic
    Friend WithEvents BlueCreativePad2 As CreativePad
    Friend WithEvents BlueCreativePad1 As CreativePad
    Friend WithEvents BlueButton15 As Button
    Friend WithEvents BlueButton27 As Button
End Class
