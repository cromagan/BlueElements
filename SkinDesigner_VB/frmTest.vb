Imports System
Imports System.Collections.Generic
Imports BlueBasics
Imports BlueBasics.Enums
Imports BlueControls
Imports BlueControls.Enums
Imports BlueControls.DialogBoxes
Imports BlueControls.EventArgs
Imports BlueControls.ItemCollection

Public Class frmTest



    Private Sub BlueButton26_Click(ByVal Sender As System.Object, ByVal e As EventArgs) Handles BlueButton26.Click
        InputBox.Show("Hallo Welt!", "", enDataFormat.Text)
    End Sub

    Private Sub BlueButton29_Click(ByVal Sender As System.Object, ByVal e As EventArgs) Handles BlueButton29.Click
        Notification.Show("Hallo Welt!", enImageCode.Wolke)
    End Sub

    Private Sub BlueButton30_Click(ByVal Sender As System.Object, ByVal e As EventArgs) Handles BlueButton30.Click

        BlueTabControl1.SelectedIndex = 2

        BlueButton24.Checked = True

    End Sub

    Private Sub frmTest_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        BlueTabPage3.Enabled = False


        BlueSelectBox1.Item.Add(New TextListItem("Test 1", False))
        BlueSelectBox1.Item.Add(New TextListItem("Test 2", False))
        BlueSelectBox1.Item.Add(New TextListItem("Test 3", False))
        BlueSelectBox1.Text = "Test 1"


        BlueSelectBox3.Item.Add(New TextListItem("Test 1", "Test 1", True))
        BlueSelectBox3.Item.Add(New TextListItem("Test 2", False))
        BlueSelectBox3.Item.Add(New TextListItem("Test 3", False))
        BlueSelectBox3.Text = "Test 1"

        BlueListBox2.Item.Add(New TextListItem("Test 1", "Test 1", True))
        BlueListBox2.Item.Add(New TextListItem("Test 2", False))
        BlueListBox2.Item.Add(New TextListItem("Test 3", False))




        'BlueEasyPic1.SetBitmap(QuickImage.Get(enImageCode.Sonne, 32).BMP)
        'BlueEasyPic2.SetBitmap(QuickImage.Get(enImageCode.Wolke, 32).BMP)



        Dim x As New DimensionPadItem("Test", New PointDF(10, 200), New PointDF(20, 200), 10, 200)
        'x.Point2 = New clsNamedPoint(x, 20, 10)
        'x.Distance(10)
        ' x.Text = "10"

        BlueCreativePad1.Item.Add(x)


        Dim x2 As New DimensionPadItem("Test", New PointDF(10, 200), New PointDF(20, 200), 10, 200)

        BlueCreativePad2.Item.Add(x2)


    End Sub

    Private Sub BlueButton2_Click(ByVal Sender As System.Object, ByVal e As EventArgs) Handles BlueButton2.Click

        BlueCaption8.ContextMenu_Show(Sender, New MouseEventArgs(MouseButtons.None, 0, 0, 0, 0))

    End Sub

    Private Sub BlueCaption8_ContextMenu_Init(sender As Object, e As ContextMenuInitEventArgs) Handles BlueCaption8.ContextMenuInit
        e.UserMenu.Add(New TextListItem("Disabled", True))
        e.UserMenu.Add(enContextMenuComands.Speichern, False)
        e.UserMenu.Add(New TextListItem("Enabled", True))
        e.UserMenu.Add(enContextMenuComands.InhaltLöschen)
        e.UserMenu.Add(enContextMenuComands.Umbenennen)
    End Sub

    Private Sub BlueButton15_Click(ByVal Sender As System.Object, ByVal e As EventArgs) Handles BlueButton15.Click
        MessageBox.Show("Test!", enImageCode.Sonne, "OK")
    End Sub

    Private Sub BlueButton27_Click(ByVal Sender As System.Object, ByVal e As EventArgs) Handles BlueButton27.Click

        Static isin As Boolean = False

        If isin Then Exit Sub
        isin = True

        BlueButton27.Enabled = False


        ' For z As Integer = 0 To 15
        Dim X As Progressbar = Progressbar.Show("Test, 15 Sekunden")

        '    Pause(1, True)

        'Next

        X.Close()

        isin = False

        BlueButton27.Enabled = True

    End Sub
End Class