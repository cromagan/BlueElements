Imports BlueControls.Controls
Imports BlueControls.Enums

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
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
        Me.VS1 = New System.Windows.Forms.PictureBox()
        Me.VS3 = New System.Windows.Forms.PictureBox()
        Me.VS2 = New System.Windows.Forms.PictureBox()
        Me.Dis0 = New ComboBox()
        Me.BlueCaption2 = New Caption()
        Me.LöAn = New Button()
        Me.FilterAus = New Button()
        Me.BlueButton3 = New Button()
        Me.TabView = New Table()
        Me.F0 = New Formula()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.Button1 = New Button()
        Me.Kopf2 = New Button()
        Me.SymHinzu = New Button()
        Me.Zeig1 = New Button()
        Me.BlueCaption1 = New Caption()
        Me.Dis1 = New ComboBox()
        Me.VS32 = New System.Windows.Forms.PictureBox()
        Me.VS22 = New System.Windows.Forms.PictureBox()
        Me.VS12 = New System.Windows.Forms.PictureBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Kopf1 = New Button()
        Me.Zeig0 = New Button()
        Me.Bindung0 = New Caption()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.BlueTabControl1 = New TabControl()
        Me.BlueTabPage1 = New TabPage()
        Me.BlueFrame3 = New GroupBox()
        Me.BlueFrame2 = New GroupBox()
        Me.BlueFrame1 = New GroupBox()
        CType(Me.VS1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.VS3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.VS2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel3.SuspendLayout()
        CType(Me.VS32, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.VS22, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.VS12, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.BlueTabControl1.SuspendLayout()
        Me.BlueTabPage1.SuspendLayout()
        Me.BlueFrame3.SuspendLayout()
        Me.BlueFrame2.SuspendLayout()
        Me.BlueFrame1.SuspendLayout()
        Me.SuspendLayout()
        '
        'VS1
        '
        Me.VS1.Location = New System.Drawing.Point(368, 8)
        Me.VS1.Name = "VS1"
        Me.VS1.Size = New System.Drawing.Size(72, 72)
        Me.VS1.TabIndex = 4
        Me.VS1.TabStop = False
        '
        'VS3
        '
        Me.VS3.Location = New System.Drawing.Point(248, 8)
        Me.VS3.Name = "VS3"
        Me.VS3.Size = New System.Drawing.Size(112, 40)
        Me.VS3.TabIndex = 5
        Me.VS3.TabStop = False
        '
        'VS2
        '
        Me.VS2.Location = New System.Drawing.Point(448, 8)
        Me.VS2.Name = "VS2"
        Me.VS2.Size = New System.Drawing.Size(32, 72)
        Me.VS2.TabIndex = 6
        Me.VS2.TabStop = False
        '
        'Dis0
        '
        Me.Dis0.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.Dis0.Format = BlueBasics.Enums.enDataFormat.Text
        Me.Dis0.Location = New System.Drawing.Point(112, 8)
        Me.Dis0.Name = "Dis0"
        Me.Dis0.Size = New System.Drawing.Size(120, 24)
        Me.Dis0.TabIndex = 13
        '
        'BlueCaption2
        '
        Me.BlueCaption2.CausesValidation = False
        Me.BlueCaption2.Location = New System.Drawing.Point(8, 8)
        Me.BlueCaption2.Name = "BlueCaption2"
        Me.BlueCaption2.Size = New System.Drawing.Size(96, 16)
        Me.BlueCaption2.Text = "Disabled Effekt:"
        Me.BlueCaption2.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'LöAn
        '
        Me.LöAn.Location = New System.Drawing.Point(16, 24)
        Me.LöAn.Name = "LöAn"
        Me.LöAn.Size = New System.Drawing.Size(125, 22)
        Me.LöAn.TabIndex = 17
        Me.LöAn.Text = "Angezeigte löschen"
        '
        'FilterAus
        '
        Me.FilterAus.ImageCode = "Trichter|16||1"
        Me.FilterAus.Location = New System.Drawing.Point(8, 24)
        Me.FilterAus.Name = "FilterAus"
        Me.FilterAus.Size = New System.Drawing.Size(128, 22)
        Me.FilterAus.TabIndex = 12
        Me.FilterAus.Text = "alle Filter aus"
        '
        'BlueButton3
        '
        Me.BlueButton3.Location = New System.Drawing.Point(16, 2)
        Me.BlueButton3.Name = "BlueButton3"
        Me.BlueButton3.Size = New System.Drawing.Size(72, 22)
        Me.BlueButton3.TabIndex = 6
        Me.BlueButton3.Text = "Beispiel"
        '
        'TabView
        '
        Me.TabView.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.TabView.Location = New System.Drawing.Point(0, 288)
        Me.TabView.Name = "TabView"
        Me.TabView.Size = New System.Drawing.Size(983, 519)
        Me.TabView.TabIndex = 0
        '
        'F0
        '
        Me.F0.Dock = System.Windows.Forms.DockStyle.Fill
        Me.F0.Location = New System.Drawing.Point(3, 3)
        Me.F0.MinimumSize = New System.Drawing.Size(320, 50)
        Me.F0.Name = "F0"
        Me.F0.Size = New System.Drawing.Size(485, 73)
        Me.F0.TabIndex = 14
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.Panel3, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.F0, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 1)
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 8)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 99.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(983, 178)
        Me.TableLayoutPanel1.TabIndex = 18
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.Button1)
        Me.Panel3.Controls.Add(Me.Kopf2)
        Me.Panel3.Controls.Add(Me.SymHinzu)
        Me.Panel3.Controls.Add(Me.Zeig1)
        Me.Panel3.Controls.Add(Me.BlueCaption1)
        Me.Panel3.Controls.Add(Me.Dis1)
        Me.Panel3.Controls.Add(Me.VS32)
        Me.Panel3.Controls.Add(Me.VS22)
        Me.Panel3.Controls.Add(Me.VS12)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel3.Location = New System.Drawing.Point(494, 82)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(486, 93)
        Me.Panel3.TabIndex = 17
        '
        'Button1
        '
        Me.Button1.ImageCode = "Datenbank|16"
        Me.Button1.Location = New System.Drawing.Point(328, 56)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(64, 32)
        Me.Button1.TabIndex = 18
        Me.Button1.Text = "Überspiele"
        '
        'Kopf2
        '
        Me.Kopf2.ImageCode = "Datenbank|16"
        Me.Kopf2.Location = New System.Drawing.Point(256, 56)
        Me.Kopf2.Name = "Kopf2"
        Me.Kopf2.Size = New System.Drawing.Size(64, 32)
        Me.Kopf2.TabIndex = 17
        Me.Kopf2.Text = "Kopf"
        '
        'SymHinzu
        '
        Me.SymHinzu.Location = New System.Drawing.Point(160, 56)
        Me.SymHinzu.Name = "SymHinzu"
        Me.SymHinzu.Size = New System.Drawing.Size(88, 32)
        Me.SymHinzu.TabIndex = 16
        Me.SymHinzu.Text = "Symbol hinzu"
        '
        'Zeig1
        '
        Me.Zeig1.ImageCode = "Pfeil_Oben|24||4"
        Me.Zeig1.Location = New System.Drawing.Point(8, 56)
        Me.Zeig1.Name = "Zeig1"
        Me.Zeig1.Size = New System.Drawing.Size(144, 32)
        Me.Zeig1.TabIndex = 15
        Me.Zeig1.Text = "das Skin anzeigen"
        '
        'BlueCaption1
        '
        Me.BlueCaption1.CausesValidation = False
        Me.BlueCaption1.Location = New System.Drawing.Point(8, 8)
        Me.BlueCaption1.Name = "BlueCaption1"
        Me.BlueCaption1.Size = New System.Drawing.Size(96, 16)
        Me.BlueCaption1.Text = "Disabled Effekt:"
        Me.BlueCaption1.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch
        '
        'Dis1
        '
        Me.Dis1.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.Dis1.Format = BlueBasics.Enums.enDataFormat.Text
        Me.Dis1.Location = New System.Drawing.Point(120, 8)
        Me.Dis1.Name = "Dis1"
        Me.Dis1.Size = New System.Drawing.Size(120, 24)
        Me.Dis1.TabIndex = 13
        '
        'VS32
        '
        Me.VS32.Location = New System.Drawing.Point(248, 8)
        Me.VS32.Name = "VS32"
        Me.VS32.Size = New System.Drawing.Size(112, 40)
        Me.VS32.TabIndex = 5
        Me.VS32.TabStop = False
        '
        'VS22
        '
        Me.VS22.Location = New System.Drawing.Point(448, 8)
        Me.VS22.Name = "VS22"
        Me.VS22.Size = New System.Drawing.Size(32, 72)
        Me.VS22.TabIndex = 6
        Me.VS22.TabStop = False
        '
        'VS12
        '
        Me.VS12.Location = New System.Drawing.Point(368, 8)
        Me.VS12.Name = "VS12"
        Me.VS12.Size = New System.Drawing.Size(72, 72)
        Me.VS12.TabIndex = 4
        Me.VS12.TabStop = False
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.Kopf1)
        Me.Panel1.Controls.Add(Me.Zeig0)
        Me.Panel1.Controls.Add(Me.Bindung0)
        Me.Panel1.Controls.Add(Me.BlueCaption2)
        Me.Panel1.Controls.Add(Me.Dis0)
        Me.Panel1.Controls.Add(Me.VS3)
        Me.Panel1.Controls.Add(Me.VS2)
        Me.Panel1.Controls.Add(Me.VS1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(3, 82)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(485, 93)
        Me.Panel1.TabIndex = 16
        '
        'Kopf1
        '
        Me.Kopf1.ImageCode = "Datenbank|16"
        Me.Kopf1.Location = New System.Drawing.Point(296, 56)
        Me.Kopf1.Name = "Kopf1"
        Me.Kopf1.Size = New System.Drawing.Size(64, 32)
        Me.Kopf1.TabIndex = 15
        Me.Kopf1.Text = "Kopf"
        '
        'Zeig0
        '
        Me.Zeig0.ImageCode = "Pfeil_Oben|24||4"
        Me.Zeig0.Location = New System.Drawing.Point(0, 56)
        Me.Zeig0.Name = "Zeig0"
        Me.Zeig0.Size = New System.Drawing.Size(152, 32)
        Me.Zeig0.TabIndex = 14
        Me.Zeig0.Text = "das Skin anzeigen"
        '
        'Bindung0
        '
        Me.Bindung0.CausesValidation = False
        Me.Bindung0.Location = New System.Drawing.Point(8, 32)
        Me.Bindung0.Name = "Bindung0"
        Me.Bindung0.Size = New System.Drawing.Size(54, 18)
        Me.Bindung0.Text = "Bindung1"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.TableLayoutPanel1)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Location = New System.Drawing.Point(0, 110)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(983, 178)
        Me.Panel2.TabIndex = 19
        '
        'BlueTabControl1
        '
        Me.BlueTabControl1.Controls.Add(Me.BlueTabPage1)
        Me.BlueTabControl1.Dock = System.Windows.Forms.DockStyle.Top
        Me.BlueTabControl1.HotTrack = True
        Me.BlueTabControl1.IsRibbonBar = True
        Me.BlueTabControl1.Location = New System.Drawing.Point(0, 0)
        Me.BlueTabControl1.Name = "BlueTabControl1"
        Me.BlueTabControl1.Size = New System.Drawing.Size(983, 110)
        Me.BlueTabControl1.TabIndex = 20
        '
        'BlueTabPage1
        '
        Me.BlueTabPage1.Controls.Add(Me.BlueFrame3)
        Me.BlueTabPage1.Controls.Add(Me.BlueFrame2)
        Me.BlueTabPage1.Controls.Add(Me.BlueFrame1)
        Me.BlueTabPage1.Location = New System.Drawing.Point(4, 25)
        Me.BlueTabPage1.Name = "BlueTabPage1"
        Me.BlueTabPage1.Size = New System.Drawing.Size(975, 81)
        Me.BlueTabPage1.TabIndex = 0
        Me.BlueTabPage1.Text = "Start"
        '
        'BlueFrame3
        '
        Me.BlueFrame3.CausesValidation = False
        Me.BlueFrame3.Controls.Add(Me.BlueButton3)
        Me.BlueFrame3.Dock = System.Windows.Forms.DockStyle.Left
        Me.BlueFrame3.Location = New System.Drawing.Point(312, 0)
        Me.BlueFrame3.Name = "BlueFrame3"
        Me.BlueFrame3.Size = New System.Drawing.Size(128, 81)
        Me.BlueFrame3.Text = "Sonstiges"
        '
        'BlueFrame2
        '
        Me.BlueFrame2.CausesValidation = False
        Me.BlueFrame2.Controls.Add(Me.LöAn)
        Me.BlueFrame2.Dock = System.Windows.Forms.DockStyle.Left
        Me.BlueFrame2.Location = New System.Drawing.Point(144, 0)
        Me.BlueFrame2.Name = "BlueFrame2"
        Me.BlueFrame2.Size = New System.Drawing.Size(168, 81)
        Me.BlueFrame2.Text = "Administration"
        '
        'BlueFrame1
        '
        Me.BlueFrame1.CausesValidation = False
        Me.BlueFrame1.Controls.Add(Me.FilterAus)
        Me.BlueFrame1.Dock = System.Windows.Forms.DockStyle.Left
        Me.BlueFrame1.Location = New System.Drawing.Point(0, 0)
        Me.BlueFrame1.Name = "BlueFrame1"
        Me.BlueFrame1.Size = New System.Drawing.Size(144, 81)
        Me.BlueFrame1.Text = "Filter"
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(983, 807)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.TabView)
        Me.Controls.Add(Me.BlueTabControl1)
        Me.Name = "frmMain"
        Me.Text = "Skin Designer"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.VS1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.VS3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.VS2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        CType(Me.VS32, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.VS22, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.VS12, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.BlueTabControl1.ResumeLayout(False)
        Me.BlueTabPage1.ResumeLayout(False)
        Me.BlueFrame3.ResumeLayout(False)
        Me.BlueFrame2.ResumeLayout(False)
        Me.BlueFrame1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TabView As Table
    Friend WithEvents BlueButton3 As Button
    Friend WithEvents VS1 As System.Windows.Forms.PictureBox
    Friend WithEvents VS3 As System.Windows.Forms.PictureBox
    Friend WithEvents VS2 As System.Windows.Forms.PictureBox
    Friend WithEvents BlueCaption2 As Caption
    Friend WithEvents Dis0 As ComboBox
    Friend WithEvents FilterAus As Button
    Friend WithEvents F0 As Formula
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Dis1 As ComboBox
    Friend WithEvents VS32 As System.Windows.Forms.PictureBox
    Friend WithEvents VS22 As System.Windows.Forms.PictureBox
    Friend WithEvents VS12 As System.Windows.Forms.PictureBox
    Friend WithEvents BlueCaption1 As Caption
    Friend WithEvents Bindung0 As Caption
    Friend WithEvents LöAn As Button
    Friend WithEvents Zeig1 As Button
    Friend WithEvents Zeig0 As Button
    Friend WithEvents BlueTabControl1 As TabControl
    Friend WithEvents BlueTabPage1 As TabPage
    Friend WithEvents BlueFrame1 As GroupBox
    Friend WithEvents BlueFrame3 As GroupBox
    Friend WithEvents BlueFrame2 As GroupBox
    Friend WithEvents SymHinzu As Button
    Friend WithEvents Kopf2 As Button
    Friend WithEvents Kopf1 As Button
    Friend WithEvents Button1 As Button
End Class
