// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueScript;
using BlueScript.Enums;
using BlueDatabase.Interfaces;
using BlueDatabase.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_RowUniqueAndInvalidate : Method_Database, IUseableForButton {

    #region Fields

    public static List<RowItem> DidRows = new();
    public static List<RowItem> InvalidatedRows = new();

    #endregion

    #region Properties

    public override List<List<string>> Args => [FilterVar];

    public List<List<string>> ArgsForButton => [];

    public List<string> ArgsForButtonDescription => [];

    public ButtonArgs ClickableWhen => ButtonArgs.Keine_Zeile;

    public override string Command => "rowuniqueandinvalidate";

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                          "Wird keine Zeile gefunden, wird eine neue Zeile erstellt.\r\n" +
                                          "Werden mehrere Zeilen gefunden, werden diese zusammengefasst (maximal 5!).\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen\r\n" +
                                           "Zusätzlich markiert, dass sie neu berechnet werden muss.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodType => MethodType.Database | MethodType.IO;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Eine neue Zeile mit den eingehenden Filterwerten anlegen, wenn diese noch nicht vorhanden ist. Aber immer invalidieren.";

    // Auch nur zum Zeilen Anlegen
    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "RowUniqueAndInvalidate(Filter, ...)";

    #endregion

    #region Methods

    public static void DoAllRows(RowItem? masterRow) {
        if (Database.ExecutingScriptAnyDatabase != 0 || DidRows.Count > 0) { return; }

        var ra = 0;
        var n = 0;

        DidRows.Clear();
        try {
            while (InvalidatedRows.Count > 0) {
                n++;
                var r = InvalidatedRows[0];
                    InvalidatedRows.RemoveAt(0);

           

                if (InvalidatedRows.Count > ra) {
                    masterRow?.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, $"{InvalidatedRows.Count - ra} neue Einträge zum Abarbeiten ({InvalidatedRows.Count + DidRows.Count } insgesamt)");
                    ra = InvalidatedRows.Count;
                }

                if (r != null && !r.IsDisposed && r.Database != null && !r.Database.IsDisposed && !DidRows.Contains(r)) {
                    DidRows.Add(r);
                    if (masterRow?.Database != null) {
                        r.UpdateRow(false, true, true, "Update von " + masterRow.CellFirstString());
                        masterRow.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, $"[{n.ToStringInt2()}] Aktualisiere {r.Database.Caption} / {r.CellFirstString()}");
                    } else {
                        r.UpdateRow(false, true, true, "Normales Update");
                    }
                }
            }
        } catch { }

        DidRows.Clear();
        masterRow?.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, "Updates abgearbeitet");
    }

    public static DoItFeedback UniqueRow(LogData ld, FilterCollection allFi, ScriptProperties scp, string coment) {
        Develop.CheckStackForOverflow();
        var r = allFi.Rows;

        if (r.Count > 5) {
            return new DoItFeedback(ld, "RowUniqueAndInvalidate gescheitert, da bereits zu viele Zeilen vorhanden sind: " + allFi.ReadableText());
        }

        if (r.Count > 1) {
            if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Zeile anlegen im Testmodus deaktiviert."); }

            r[0].Database?.Row.Combine(r);
            r[0].Database?.Row.RemoveYoungest(r, true);
            r = allFi.Rows;
            if (r.Count > 1) {
                return new DoItFeedback(ld, "RowUniqueAndInvalidate gescheitert, Aufräumen fehlgeschlagen: " + allFi.ReadableText());
            }
        }

        RowItem? myRow;

        if (r.Count == 0) {
            if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Zeile anlegen im Testmodus deaktiviert."); }
            var (newrow, message) = RowCollection.GenerateAndAdd(allFi, coment);
            if (newrow == null) { return new DoItFeedback(ld, "Neue Zeile konnte nicht erstellt werden: " + message); }
            myRow = newrow;
        } else {
            myRow = r[0];
        }

        if (myRow.Database is not Database db) { return new DoItFeedback(ld, "Interner Fehler"); }

        if (db.Column.SysRowState is ColumnItem srs) {
            var v = myRow.CellGetLong(srs);

            if (v > 0) {
                var lastchange = RowItem.TimeCodeToUTCDateTime(v);

                if (DateTime.UtcNow.Subtract(lastchange).TotalMinutes > 15) {
                    //return new DoItFeedback(ld, $"Fehlgeschlagen, da eine Zeile {myRow.CellFirstString()} erst durchgerechnet wurde und der Intervall zu kurz ist (15 Minuten)");
                    myRow.CellSet(srs, string.Empty, coment);
                    InvalidatedRows.Add(myRow);
                }
            }
        } else {
            return new DoItFeedback(ld, $"Der Tabelle {db.Caption} fehlt die Spalte Zeilenstatus");
        }

        return Method_Row.RowToObjectFeedback(myRow);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null || allFi.Count == 0) {
            return new DoItFeedback(ld, "Fehler im Filter");
        }

        foreach (var thisFi in allFi) {
            if (thisFi.Column is not ColumnItem c) {
                return new DoItFeedback(ld, "Fehler im Filter, Spalte ungültig");
            }

            if (thisFi.FilterType is not Enums.FilterType.Istgleich and not Enums.FilterType.Istgleich_GroßKleinEgal) {
                return new DoItFeedback(ld, "Fehler im Filter, nur 'is' ist erlaubt");
            }

            if (thisFi.SearchValue.Count != 1) {
                return new DoItFeedback(ld, "Fehler im Filter, ein einzelner Suchwert wird benötigt");
            }
            var l = allFi.InitValue(c, true);
            if (thisFi.SearchValue[0] != l) {
                return new DoItFeedback(ld, "Fehler im Filter, Wert '" + thisFi.SearchValue[0] + "' kann nicht gesetzt werden (-> '" + l + "')");
            }
        }
        var mydb = MyDatabase(scp);
        if (mydb == null) { return new DoItFeedback(ld, "Interner Fehler"); }

        return UniqueRow(ld, allFi, scp, $"Script-Befehl: 'UniqueRow' der Tabelle {mydb.Caption}, Skript {scp.ScriptName}");
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string filterarg, string rowarg) => filterarg;

    #endregion
}