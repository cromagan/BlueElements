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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.Collections.Generic;
using System.Linq;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.Forms;

public partial class RelationDiagram : PadEditor {

    #region Fields

    private readonly ColumnItem? _column;
    private Database? _database;

    #endregion

    #region Constructors

    //private bool RelationsValid;
    //   Dim ItS As New Size(60, 80)
    public RelationDiagram(Database? database) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _database = database;
        _database.Disposing += Database_Disposing;
        foreach (var thisColumnItem in _database.Column.Where(thisColumnItem => thisColumnItem != null && thisColumnItem.Format == DataFormat.RelationText)) {
            _column = thisColumnItem;
            break;
        }
    }

    #endregion

    #region Methods

    public RowFormulaPadItem? AddOne(string what, int xPos, int ypos, string layoutId) {
        if (string.IsNullOrEmpty(what)) { return null; }
        if (Pad.Item[what] != null) { return null; }
        var r = _database.Row[what];
        if (r == null) {
            MessageBox.Show("<b>" + what + "</B> konnte nicht hinzugefügt werden.", ImageCode.Information, "OK");
            return null;
        }
        if (ItemOfRow(r) != null) { return null; }
        RowFormulaPadItem i2 = new(_database, r.Key, layoutId);
        Pad.AddCentered(i2);
        //  Pad.Invalidate()
        i2.SetLeftTopPoint(xPos, ypos);
        i2.InDenVordergrund();
        //RelationsValid = false;
        return i2;
    }

    public RowFormulaPadItem? ItemOfRow(RowItem r) {
        foreach (var thisItem in Pad.Item) {
            if (thisItem is RowFormulaPadItem tempVar && tempVar.Row == r) { return tempVar; }
        }
        return null;
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (_database != null) {
            _database.Disposing -= Database_Disposing;
        }

        _database = null;
    }

    private void BezPlus(RowFormulaPadItem? initialItem) {
        // Den Beziehungstext holen
        var t = initialItem.Row.CellGetString(_column).ToUpper();
        if (string.IsNullOrEmpty(t)) { return; }
        // Alle möglichen Namen holen
        List<string> names = new();
        names.AddRange(_database.Column[0].GetUcaseNamesSortedByLenght());
        // Namen ermitteln, die relevant sind
        List<string> bez = new();
        foreach (var thisN in names.Where(thisN => t.Contains(thisN))) {
            bez.AddIfNotExists(thisN);
            t = t.Replace(thisN, string.Empty);
        }
        if (bez.Count == 0) { return; }
        // Namen in der Übersicht hinzufügen
        var lastit = initialItem;
        foreach (var thisn in bez) {
            var ro = _database.Row[thisn];
            var it = ItemOfRow(ro);
            if (it == null) {
                //lastit = AddOne(thisn, (int)lastit.p_RO.X, (int)lastit.p_RO.Y, lastit.Layout_ID);
                lastit = AddOne(thisn, 0, 0, lastit.Layout_Id);
            }
        }
        //var Plus = 0;
        //foreach (var thisR in l)
        //{
        //    {
        //        Plus++;
        //        var Rel = new clsRelation(Col, I.Row, thisR);
        //        switch (Rel.Status)
        //        {
        //            case clsRelation.enRelationStatus.Gleich:
        //            case clsRelation.enRelationStatus.OhneText:
        //            case clsRelation.enRelationStatus.Identisch:
        //                if (Welche == -1 || Welche == 0)
        //                {
        //                    AddOne(Rel.Sec, (int)(I.UsedArea().X + I.UsedArea().Width * (Plus + 0.5m)), (int)I.UsedArea().Y);
        //                }
        //                break;
        //            case clsRelation.enRelationStatus.Uber:
        //                if (Welche == -1 || Welche == 1)
        //                {
        //                    AddOne(Rel.Sec, (int)(I.UsedArea().X + I.UsedArea().Width * (Plus + 0.5m)), (int)(I.UsedArea().Y - I.UsedArea().Height * 1.5m));
        //                }
        //                break;
        //            case clsRelation.enRelationStatus.Unter:
        //                if (Welche == -1 || Welche == 2)
        //                {
        //                    AddOne(Rel.Sec, (int)(I.UsedArea().X + I.UsedArea().Width * (Plus + 0.5m)), (int)(I.UsedArea().Y + I.UsedArea().Height * 1.5m));
        //                }
        //                break;
        //            default:
        //                Develop.DebugPrint("Unbekannter Status: " + Rel.Status);
        //                break;
        //        }
        //    }
        //}
    }

    //private void RepairLinesAndFullProcessing() {
    //    //Develop.DebugPrint_NichtImplementiert();
    //    //if (RelationsValid)
    //    //{
    //    //    return;
    //    //}
    //    //RelationsValid = true;
    //    //var z = 0;
    //    //do
    //    //{
    //    //    if (z >= Pad.Item.Count) { break; }
    //    //    if (Pad.Item[z] is LinePadItem)
    //    //    {
    //    //        Pad.Item.RemoveAt(z);
    //    //    }
    //    //    else
    //    //    {
    //    //        z++;
    //    //    }
    //    //} while (true);
    //    //var it = new List<BasicItem>();
    //    //it.AddRange(Pad.Item);
    //    //if (it == null || it.Count < 2) { return; }
    //    //foreach (BasicPadItem ThisItem in it)
    //    //{
    //    //    if (ThisItem != null && ThisItem is RowFormulaPadItem tempVar)
    //    //    {
    //    //        var l = Database.Cell.GetList(Col, tempVar.Row);
    //    //        if (l.Count > 0)
    //    //        {
    //    //            foreach (var thisR in l)
    //    //            {
    //    //                if (thisR.Contains("{ID=Relation") || !thisR.StartsWith("{"))
    //    //                {
    //    //                    AddVerbinder(tempVar, new clsRelation(Col, tempVar.Row, thisR));
    //    //                }
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //}
    //private void AddVerbinder(RowFormulaPadItem Von, string NachRelation) {
    //    Develop.DebugPrint_NichtImplementiert();
    //    //if (Von == null)
    //    //{
    //    //    return;
    //    //}
    //    //var NachR = Database.Row[NachRelation.Sec];
    //    //if (NachR == null) { return; }
    //    //var nach = ItemOfRow(NachR);
    //    //if (nach == null) { return; }
    //    //if (Von == nach) { return; }
    //    //var id = NachRelation.ID();
    //    //foreach (var ThisItem in Pad.Item)
    //    //{
    //    //    if (ThisItem is LinePadItem)
    //    //    {
    //    //        if (ThisItem.Internal == id) { return; }
    //    //    }
    //    //}
    //    //PointM P1 = null;
    //    //PointM P2 = null;
    //    //switch (NachRelation.Status)
    //    //{
    //    //    case clsRelation.enRelationStatus.Uber:
    //    //        P1 = Von.PointOf(enAlignment.Top_HorizontalCenter);
    //    //        P2 = nach.PointOf(enAlignment.Bottom_HorizontalCenter);
    //    //        break;
    //    //    case clsRelation.enRelationStatus.Unter:
    //    //        P1 = nach.PointOf(enAlignment.Top_HorizontalCenter);
    //    //        P2 = Von.PointOf(enAlignment.Bottom_HorizontalCenter);
    //    //        break;
    //    //    case clsRelation.enRelationStatus.Identisch:
    //    //    case clsRelation.enRelationStatus.Gleich:
    //    //    case clsRelation.enRelationStatus.OhneText:
    //    //        P1 = nach.PointOf(enAlignment.VerticalCenter_Left);
    //    //        P2 = Von.PointOf(enAlignment.VerticalCenter_Right);
    //    //        break;
    //    //    default:
    //    //        P1 = nach.PointOf(enAlignment.VerticalCenter_Left);
    //    //        P2 = Von.PointOf(enAlignment.VerticalCenter_Right);
    //    //        break;
    //    //}
    //    //var i = new LinePadItem(id, PadStyles.Style_Standard, enConectorStyle.Ausweichenx, P1, P2);
    //    //Pad.Item.Add(i);
    //    //i.InDenHintergrund();
    //    //Pad.Relation_Add(enRelationType.PositionZueinander, P1, i.Point1);
    //    //Pad.Relation_Add(enRelationType.PositionZueinander, P2, i.Point2);
    //}
    //private void btnBilderExport_Click(object sender, System.EventArgs e) {
    //    System.Windows.Forms.FolderBrowserDialog fl = new();
    //    fl.ShowDialog();
    //    foreach (var thisR in Pad.Item) {
    //        if (thisR is RowFormulaPadItem r) {
    //            var no = r.Row.CellFirstString();
    //            no = no.Replace(" ", "_");
    //            no = no.Replace(",", "_");
    //            no = no.Replace("__", "_");
    //            var newn = IO.TempFile(fl.SelectedPath, no, "png");
    //            r.GeneratedBitmap.Save(newn, System.Drawing.Imaging.ImageFormat.Png);
    //            foreach (var thisc in r.Row.Database.Column) {
    //                if (thisc.Format == DataFormat.Link_To_Filesystem) {
    //                    var l = r.Row.CellGetList(thisc);
    //                    foreach (var thiss in l) {
    //                        var f = thisc.BestFile(thiss, false);
    //                        if (IO.FileExists(f)) {
    //                            var n2 = r.Row.CellFirstString() + "-" + thisc.Caption;
    //                            n2 = n2.Replace(" ", "_");
    //                            n2 = n2.Replace(",", "_");
    //                            n2 = n2.Replace("__", "_");
    //                            var newn2 = IO.TempFile(fl.SelectedPath, n2, f.FileSuffix());
    //                            IO.CopyFile(f, newn2, true);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    private void btnTextExport_Click(object sender, System.EventArgs e) {
        System.Windows.Forms.FolderBrowserDialog fl = new();
        fl.ShowDialog();
        List<string> l = new();
        foreach (var thisR in Pad.Item) {
            if (thisR is RowFormulaPadItem r) {
                r.Row.DoAutomatic(true, true, "to be sure");
                l.Add("#######################################################################");
                l.Add(" ");
                l.Add(r.Row.CellFirstString());
                l.Add(" ");
                var t = r.Row.CellGetList(_column);
                l.AddRange(t);
                l.Add(" ");
                l.Add(" ");
                l.Add(" ");
                l.Add(" ");
                l.Add(" ");
                var no = r.Row.CellFirstString();
                no = no.Replace(" ", "_");
                no = no.Replace(",", "_");
                no = no.Replace("__", "_");
                var newn = IO.TempFile(fl.SelectedPath, no, "txt");
                t.Save(newn, Constants.Win1252, false);
            }
        }
        var newn2 = IO.TempFile(fl.SelectedPath, "+++ALLES+++", "txt");
        l.Save(newn2, Constants.Win1252, true);
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Close();

    private void Hinzu_Click(object sender, System.EventArgs e) {
        ItemCollectionList il = new();
        il.AddRange(_database.Column[0].Contents());
        il.Sort();
        il.CheckBehavior = CheckBehavior.SingleSelection;
        var i = InputBoxListBoxStyle.Show("Objekt hinzufügen:", il, AddType.None, true);
        if (i == null || i.Count != 1) {
            return;
        }
        AddOne(i[0], 0, 0, string.Empty);
        if (Pad.Item.Count < 10) {
            Pad.ZoomFit();
        }
        //RepairLinesAndFullProcessing();
    }

    private void Pad_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        //Dim i As BasicItem = DirectCast(MouseOver, BasicItem)
        if (e.HotItem is not RowFormulaPadItem) { return; }
        e.UserMenu.Add("Alle Einträge hinzufügen, die mit diesem hier Beziehungen haben", "Bez+", ImageCode.PlusZeichen);
    }

    private void Pad_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (e.HotItem is not RowFormulaPadItem i) { return; }

        switch (e.ClickedComand) {
            case "Bez+":
                BezPlus(i);
                break;

            default:
                Develop.DebugPrint(e);
                break;
        }
        //RepairLinesAndFullProcessing();
    }

    #endregion

    //Private Sub Entwirren()
    //    CheckStackForOverflow()
    //    ' Dim Wirr As Boolean = False
    //    For P1 As Integer = 0 To Pad.Item.Lastx
    //        If Pad.Item(P1) IsNot Nothing AndAlso TypeOf Pad.Item(P1).Object Is clsExtTxt Then
    //            For P2 As Integer = Skin.P1 + 1 To Pad.Item.Lastx
    //                If Pad.Item(2) IsNot Nothing AndAlso TypeOf Pad.Item(2).Object Is clsExtTxt Then
    //                    If Pad.Item(P1).UsedArea.IntersectsWith(Pad.Item(2).UsedArea) Then
    //                        Entwirren(Pad.Item(P1), Pad.Item(2))
    //                        Entwirren()
    //                        Exit Sub
    //                    End If
    //                End If
    //            Next
    //        End If
    //    Next
    //    '   If Wirr Then Entwirren()
    //End Sub
    //Private Sub Entwirren(I1 As BasicItem, I2 As BasicItem)
    //    If I1.UsedArea.Left >= I2.UsedArea.Left Then
    //        I1.UsedArea = New Rectangle(I1.Coordinates.Left + Pad.Raster.Width,
    //                                      I1.Coordinates.Top,
    //                                      I1.Coordinates.Width,
    //                                      I1.Coordinates.Height)
    //        I2.UsedArea = New Rectangle(I2.Coordinates.Left - Pad.Raster.Width,
    //                          I2.Coordinates.Top,
    //                          I2.Coordinates.Width,
    //                          I2.Coordinates.Height)
    //    Else
    //        I1.UsedArea = New Rectangle(I1.Coordinates.Left - Pad.Raster.Width,
    //                                      I1.Coordinates.Top,
    //                                      I1.Coordinates.Width,
    //                                      I1.Coordinates.Height)
    //        I2.UsedArea = New Rectangle(I2.Coordinates.Left + Pad.Raster.Width,
    //                          I2.Coordinates.Top,
    //                          I2.Coordinates.Width,
    //                          I2.Coordinates.Height)
    //    End If
    //End Sub
    //Private Function Abbrechen() As Boolean
    //    If Not Pad.Changed Then Return False
    //   switch (MessageBox.Show("Sollen ihre Änderungen gespeichert werden?", ImageCode.Frage, "Speichern", "Nicht speichern", "Abbrechen")
    //        Case Is = 0
    //            Save_Click(Nothing, Nothing)
    //            Return False
    //        Case Is = 1
    //            Return False
    //        Case Else
    //            Return True
    //    End Select
    //End Function
    //Private Sub Neu_Click(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Neu.Click
    //    If Abbrechen() Then Exit Sub
    //    Pad.Item.Clear()
    //    Pad.ZoomFit()
    //    Pad.Changed = False
    //End Sub
    //Private Sub SaveFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog1.FileOk
    //    ' Stop
    //    'Dim l As New List(Of String)
    //    'For z As Integer = 0 To Pad.Item.Lastx
    //    '    If Pad.Item(z) IsNot Nothing AndAlso TypeOf Pad.Item(z).Object Is clsExtTxt Then
    //    '        l.Add(Pad.Item(z).Internal & "|" & Pad.Item(z).UsedArea.Left & "|" & Pad.Item(z).UsedArea.Top)
    //    '    End If
    //    'Next
    //    'SaveToDiskx(SaveFileDialog1.FileName, l.JoinWithCr(), False)
    //    'Pad.Changed = False
    //End Sub
    //Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
    //    Pad.Item.Clear()
    //    Dim l As String = LoadFromDisk(OpenFileDialog1.FileName)
    //    Dim n() As String = l.SplitAndCutByCr
    //    Dim p() As String
    //    For z As Integer = 0 To n.GetUpperBound(0)
    //        p = n(z).SplitAndCutBy("|")
    //        AddOne(p(0), Integer.Parse(p(1)) * 15, Integer.Parse(p(2)) * 15)
    //    Next
    //    RepairLinesAndFullProcessing()
    //    Pad.ZoomFit()
    //    Pad.Changed = False
    //    '  Stop
    //End Sub
    //protected override void OnShown(System.EventArgs e)
    //{
    //    base.OnShown(e);
    //    Pad.Changed = false;
    //}
}