Option Strict On
Option Explicit On

Imports BlueBasics
Imports System.Collections.Generic
Imports System
Imports BlueDatabase
Imports BlueControls.Enums
Imports BlueControls.EventArgs
Imports BlueBasics.Enums
Imports BlueControls.Controls
Imports BlueDatabase.EventArgs
Imports BlueDatabase.Enums
Imports BlueControls.DialogBoxes

Class frmMain

    Dim Version As String = "0.0001"


    Dim DB() As Database

    Dim copyrow As RowItem = Nothing

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing


        'For z As Integer = 0 To DB.GetUpperBound(0)
        '    With DB(z)
        '        ToEnum(.Column("Control"), GetType(enDesign))
        '        ToEnum(.Column("Status"), GetType(enStates))
        '        ToEnum(.Column("Border_Style"), GetType(enRahmenArt))
        '        ToEnum(.Column("Draw_Back"), GetType(enHintergrundArt))
        '        '   ToEnum(.Column("Draw_Text"), GetType(enFont))
        '        ToEnum(.Column("Kontur"), GetType(enKontur))
        '        .Row.DoAutomatic(Nothing, True)

        '        .Release()
        '    End With
        'Next
        BlueDatabase.Database.ReleaseAll(True)
    End Sub

    'Public Sub ToEnum(ByVal Col As ColumnItem, ByRef t As System.Type)
    '    Col.ShowUndo = False

    '    For Each ThisRowItem As RowItem In Col.Database.Row
    '        If ThisRowItem IsNot Nothing Then
    '            If Not ThisRowItem.CellIsNullOrEmpty(Col) Then
    '                ThisRowItem.CellSet(Col, CInt(Val(System.Enum.Parse(t, ThisRowItem.CellGetString(Col)))))
    '            End If
    '        End If
    '    Next
    '    Col.DropDownItems.Clear()
    'End Sub


    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Dim pf As String = System.Reflection.Assembly.GetEntryAssembly.Location.FilePath
        pf = pf.PathParent(3) & "BlueControls\BlueControls\Ressourcen\Skin\"

        ReDim DB(2)
        DB(0) = New Database(False, AddressOf Table.Database_NeedPassword, AddressOf CreativePad.GenerateLayoutFromRow, AddressOf CreativePad.RenameColumnInLayout) ' MUSS IMMER  Windows10 BLEIBEN! VORLAGE!
        DB(0).LoadFromDisk(pf & "Windows10.skn")

        DB(2) = New Database(False, AddressOf Table.Database_NeedPassword, AddressOf CreativePad.GenerateLayoutFromRow, AddressOf CreativePad.RenameColumnInLayout)
        DB(2).LoadFromDisk(pf & "XP.skn")

        DB(1) = New Database(False, AddressOf Table.Database_NeedPassword, AddressOf CreativePad.GenerateLayoutFromRow, AddressOf CreativePad.RenameColumnInLayout)
        DB(1).LoadFromDisk(pf & "GlossyCyan.skn")


        For z As Integer = 0 To DB.GetUpperBound(0)
            With DB(z)
                .UnlockHard()
                ToReadable(.Column("Control"), GetType(enDesign))
                ToReadable(.Column("Status"), GetType(enStates))
                ToReadable(.Column("Border_Style"), GetType(enRahmenArt))
                ToReadable(.Column("Draw_Back"), GetType(enHintergrundArt))
                ToReadable(.Column("Kontur"), GetType(enKontur))
            End With
        Next






        TabView.Database = DB(0)

        DB(0).Column(0).DropdownBearbeitungErlaubt = True
        DB(0).Column(1).DropdownBearbeitungErlaubt = True





        For z As Integer = DB.GetUpperBound(0) To 1 Step -1 ' Rückwärts, um dir richtigen Fehlermeldungen zu bekommen
            'DB(z).CopyLayout(DB(0), False)


            Dim r As RowItem

            ' Werte der Stamm-Datenbank in die zweite Datenbank übernehmen
            For Each ThisRowItem As RowItem In DB(0).Row
                If ThisRowItem IsNot Nothing Then

                    r = DB(z).Row(New FilterItem(DB(z).Column(0), enFilterType.Istgleich, ThisRowItem.CellGetString(ThisRowItem.Database.Column(0))),
                                 New FilterItem(DB(z).Column(1), enFilterType.Istgleich, ThisRowItem.CellGetString(ThisRowItem.Database.Column(1))))

                    If r Is Nothing Then
                        r = DB(z).Row.Add(ThisRowItem.CellGetString(ThisRowItem.Database.Column(0)))
                        r.CellSet(r.Database.Column(0), ThisRowItem.CellGetString(ThisRowItem.Database.Column(1)))
                    End If

                    If ThisRowItem.CellIsNullOrEmpty("Font") Then
                        r.CellSet("Font", String.Empty)
                    Else
                        If r.CellIsNullOrEmpty("Font") Then
                            Notification.Show(DB(z).Filename.FileNameWithoutSuffix & "<br>" & ThisRowItem.CellGetString(ThisRowItem.Database.Column(0)) & "<br>" & ThisRowItem.CellGetString(ThisRowItem.Database.Column(1)) & "<br>Schrift prüfen", enImageCode.Warnung)
                        End If
                    End If





                End If
            Next

            ' Alte Werte der zweit datenbank löschen
            Dim Again As Boolean

            '  DB(z).Editablexx("")
            Do
                Again = False

                For Each thisRow As RowItem In DB(z).Row


                    If thisRow IsNot Nothing Then
                        If DB(0).Row(New FilterItem(DB(0).Column(0), enFilterType.Istgleich, thisRow.CellGetString(thisRow.Database.Column(0))),
                                     New FilterItem(DB(0).Column(1), enFilterType.Istgleich, thisRow.CellGetString(thisRow.Database.Column(1)))) Is Nothing Then
                            DB(z).Row.Remove(thisRow)
                            Again = True
                            Exit For
                        End If
                    End If
                Next

                If Not Again Then Exit Do

            Loop


        Next z


        TabView.Arrangement = 1



        Dis0.Text = DB(0).Tags(0)
        Dis0.Item.AddRange(GetType(enImageCodeEffect))


        Dis1.Text = DB(1).Tags(0)
        Dis1.Item.AddRange(GetType(enImageCodeEffect))

        F0.Database = DB(0)
        F1.Database = DB(1)

    End Sub




    Private Sub SK_ContextMenu_Init(sender As Object, e As ContextMenuInitEventArgs) Handles TabView.ContextMenuInit
        Dim CellKey As String = CStr(e.Tag)
        If (String.IsNullOrEmpty(CellKey)) Then Exit Sub
        Dim Column As ColumnItem = Nothing
        Dim Row As RowItem = Nothing
        TabView.Database.Cell.DataOfCellKey(CellKey, Column, Row)



        If Row IsNot Nothing AndAlso Column IsNot Nothing Then

            If Column.Name <> "CONTROL" AndAlso Column.Name <> "STATUS" Then

                e.UserMenu.Add(enContextMenuComands.SpaltenEigenschaftenBearbeiten)

                e.UserMenu.Add(New BlueControls.ItemCollection.LineListItem)
                e.UserMenu.Add(enContextMenuComands.InhaltLöschen)
                e.UserMenu.Add(enContextMenuComands.SuchenUndErsetzen)

                '    UserMenu.Add(New textitem("überall", "Wert-Massen-Änderung", BlueQuickImage.Get("Pfeil_Oben|16||4")))

                'e.UserMenu.Add(New TextListItem(enContextMenuComands.ZellenInhaltKopieren))
                'e.UserMenu.Add(New TextListItem(enContextMenuComands.ZellenInhaltPaste))

            Else
                e.UserMenu.Add(enContextMenuComands.ZeileLöschen, True)
                'e.UserMenu.Add(New TextListItem("ZeileCopy", "Zeileninhalt kopieren", QuickImage.Get("Copy|16|||FF0000")))
                'e.UserMenu.Add(New TextListItem("ZeilePaste", "Zeileninhalt einfügen", QuickImage.Get("Clipboard|16|||FF0000"), copyrow IsNot Nothing))
            End If

            e.UserMenu.Add(enContextMenuComands.SpaltenEigenschaftenBearbeiten)


        End If





    End Sub

    Private Sub SK_ContextMenu_ItemClicked(sender As Object, e As ContextMenuItemClickedEventArgs) Handles TabView.ContextMenuItemClicked
        Dim CellKey As String = CStr(e.Tag)
        If (String.IsNullOrEmpty(CellKey)) Then Exit Sub
        Dim Column As ColumnItem = Nothing
        Dim Row As RowItem = Nothing
        TabView.Database.Cell.DataOfCellKey(CellKey, Column, Row)


        Select Case e.ClickedComand.Internal
            Case Is = "ZeileLöschen"
                If Row IsNot Nothing Then
                    If MessageBox.Show("Zeile löschen?", enImageCode.Frage, "Ja", "Nein") = 0 Then
                        TabView.Database.Row.Remove(Row)
                    End If
                End If



            'Case Is = "ZeileCopy"
            '    copyrow = Nothing
            '    If Row IsNot Nothing Then copyrow = Row


            'Case Is = "ZeilePaste"
                'If Row Is Nothing Then Exit Sub
                'For z As Integer = 2 To TabView.Database.Column.Count - 1
                '    If TabView.Database.Column(z).Format.CanBeChangedByRules Then
                '        TabView.Database.Cell(z, Row).Set(TabView.Database.Cell(z, copyrow).String)
                '    End If
                'Next


            Case Is = "ZellenInhaltKopieren"
                Table.CopyToClipboard(Column, Row, True)

            Case Is = "ZellenInhaltPaste"
                If Not System.Windows.Forms.Clipboard.GetDataObject.GetDataPresent(
                       System.Windows.Forms.DataFormats.Text) Then Exit Sub


                Dim nt As String = CStr(System.Windows.Forms.Clipboard.GetDataObject.GetData(System.Windows.Forms.DataFormats.Text))
                Row.CellSet(Column, nt)


            Case Is = "SuchenUndErsetzen"
                TabView.OpenSearchAndReplace()

            Case Is = "InhaltLöschen"
                Row.CellSet(Column, String.Empty)

            Case Else
                Develop.DebugPrint(e.ClickedComand)


        End Select

    End Sub

    Private Sub BlueButton3_Click(ByVal Sender As Object, ByVal e As EventArgs) Handles BlueButton3.Click
        Dim w As New frmTest
        w.Show()
    End Sub



    Private Sub SK_CursorPos_Changed(ByVal Sender As Object, e As CellEventArgs) Handles TabView.CursorPosChanged


        If e.Row Is Nothing Then Exit Sub


        Dim r1 As RowItem = DB(0).Row(New FilterItem(DB(0).Column(0), enFilterType.Istgleich_GroßKleinEgal, e.Row.CellGetString(e.Row.Database.Column(0))),
                                      New FilterItem(DB(0).Column(1), enFilterType.Istgleich_GroßKleinEgal, e.Row.CellGetString(e.Row.Database.Column(1))))

        F0.ShowingRowKey = r1.Key


        Dim r2 As RowItem = DB(1).Row(New FilterItem(DB(1).Column(0), enFilterType.Istgleich_GroßKleinEgal, e.Row.CellGetString(e.Row.Database.Column(0))),
                                      New FilterItem(DB(1).Column(1), enFilterType.Istgleich_GroßKleinEgal, e.Row.CellGetString(e.Row.Database.Column(1))))



        If r2 IsNot Nothing Then
            F1.ShowingRowKey = r2.Key
        Else
            F1.ShowingRowKey = -1
        End If



        BerechneBindung(r1, Bindung0)

    End Sub


    Private Sub BerechneBindung(ByVal Vorlage As RowItem, ByVal bindung As Caption)

        bindung.Text = "einzigartig"

        Dim Ident As Boolean = False


        If Vorlage Is Nothing Then Exit Sub


        For Each ThisRowItem As RowItem In TabView.Database.Row

            If ThisRowItem IsNot Nothing Then
                ThisRowItem.CellSet(TabView.Database.Column.SysLocked, "-")
            End If
        Next


        Dim ZielR As RowItem





        For Each ThisRowItem As RowItem In Vorlage.Database.Row
            If ThisRowItem IsNot Nothing Then

                Ident = True
                For Each ThisColumnItem As ColumnItem In Vorlage.Database.Column
                    If String.IsNullOrEmpty(ThisColumnItem.Identifier) AndAlso ThisColumnItem.Index > 1 Then
                        If Vorlage.CellGetString(ThisColumnItem.Name) <> ThisRowItem.CellGetString(ThisColumnItem.Name) Then Ident = False
                    End If
                Next


                If Ident Then
                    ZielR = TabView.Database.Row(New FilterItem(TabView.Database.Column(0), enFilterType.Istgleich_GroßKleinEgal, ThisRowItem.CellGetString(ThisRowItem.Database.Column(0))),
                                                 New FilterItem(TabView.Database.Column(1), enFilterType.Istgleich_GroßKleinEgal, ThisRowItem.CellGetString(ThisRowItem.Database.Column(1))))

                    If ZielR IsNot Nothing Then
                        ZielR.CellSet(TabView.Database.Column.SysLocked, "+")
                        If ThisRowItem IsNot Vorlage Then bindung.Text = "Es gibt Doppelte!"
                    End If
                End If

            End If


        Next




    End Sub





    Private Sub Dis_Zone_Click(ByVal Sender As Object, E As BasicListItemEventArgs) Handles Dis0.ItemClicked, Dis1.ItemClicked
        DB(0).Tags(0) = Dis0.Text
        DB(1).Tags(0) = Dis1.Text
    End Sub




    Private Sub FilterAus_Click(ByVal Sender As Object, ByVal e As EventArgs) Handles FilterAus.Click
        TabView.Filter.Clear()
    End Sub








    Public Sub ToReadable(ByVal Col As ColumnItem, ByRef t As System.Type)
        Col.ShowUndo = False

        Col.Replacer.Clear()

        Dim items As Array
        items = System.Enum.GetValues(t)
        Dim item As String
        For Each item In items

            Dim l As Long = 0
            If Long.TryParse(item, l) Then
                Dim te As String = System.Enum.GetName(t, l)

                If Not String.IsNullOrEmpty(te) Then
                    Col.Replacer.Add(item & "|" & te)
                End If

            End If


        Next

        Col.ShowUndo = True

        'Dim x As String() = t.GetEnumNames()
        'Dim x2 As String() = t.GetEnumValues()

        'Dim te As String

        'For Each ThisRowItem As RowItem In Col.Database.Row
        '    If ThisRowItem IsNot Nothing Then
        '        If Not ThisRowItem.CellIsNullOrEmpty(Col) Then


        '            Dim l As Long = 0

        '            If Long.TryParse(ThisRowItem.CellGetString(Col), l) Then
        '                te = System.Enum.GetName(t, ThisRowItem.CellInteger(Col))
        '                If Not String.IsNullOrEmpty(te) Then ThisRowItem.CellSet(Col, te)
        '            End If

        '        End If
        '    End If
        'Next


        'Col.DropDownItems.Clear()
        'Col.DropDownItems.AddRange(System.Enum.GetNames(t))


    End Sub







    'Private Sub BlueButton1_Click(Sender As System.Object, e As EventArgs) Handles LöAn.Click
    '    TabView.Database.Row.Remove(TabView.Filter, True)
    'End Sub

    'Private Sub Zeig0_Click(Sender As System.Object, e As EventArgs) Handles Zeig0.Click
    '    Dim s As String = TabView.ViewToString
    '    TabView.Database = DB(0)
    '    TabView.ParseView(s)
    'End Sub

    'Private Sub Zeig1_Click(Sender As System.Object, e As EventArgs) Handles Zeig1.Click
    '    Dim s As String = TabView.ViewToString
    '    TabView.Database = DB(1)
    '    TabView.ParseView(s)
    'End Sub



    Private Sub Kopf1_Click(sender As Object, e As EventArgs) Handles Kopf1.Click
        BlueControls.BlueDatabaseDialogs.tabAdministration.OpenDatabaseHeadEditor(DB(0))

    End Sub

    Private Sub Kopf2_Click(sender As Object, e As EventArgs) Handles Kopf2.Click

        MessageBox.Show("Diese Datenbank wird mit den Kopfeigenschaften von Windows 10 Skin überschrieben.<br>Nur die Binärdaten können hier abgeändert werden.", enImageCode.Information, "OK")

        BlueControls.BlueDatabaseDialogs.tabAdministration.OpenDatabaseHeadEditor(DB(0))
    End Sub

End Class
