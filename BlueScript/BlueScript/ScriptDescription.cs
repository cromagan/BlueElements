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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static BlueBasics.Constants;

namespace BlueScript;

public abstract class ScriptDescription : IParseable, IReadableTextWithPropertyChangingAndKey, IDisposableExtended, IErrorCheckable, IComparable {

    #region Constructors

    public ScriptDescription(string adminInfo, string image, string name, string quickInfo, string script, ReadOnlyCollection<string> userGroups) {
        AdminInfo = adminInfo;
        Image = image;
        KeyName = name;
        ColumnQuickInfo = quickInfo;
        Script = script;
        UserGroups = userGroups;
    }

    protected ScriptDescription(string name, string script) : this(string.Empty, string.Empty, name, string.Empty, script, EmptyReadOnly) { }

    protected ScriptDescription() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, EmptyReadOnly) { }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~ScriptDescription() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string AdminInfo { get; private set; }

    public string ColumnQuickInfo { get; private set; }
    public string CompareKey => KeyName;

    public string Image { get; private set; }

    public bool IsDisposed { get; private set; }

    public string KeyName { get; private set; }
    public string Script { get; private set; }

    public ReadOnlyCollection<string> UserGroups { get; private set; }

    #endregion

    #region Methods

    public abstract List<string> Attributes();

    public abstract int CompareTo(object obj);

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual string ErrorReason() {
        if (string.IsNullOrEmpty(KeyName)) { return "Kein Name angegeben."; }
        if (!KeyName.IsFormat(FormatHolder.Text)) { return "Ungültiger Name"; }
        return string.Empty;
    }

    public virtual List<string> ParseableItems() {
        try {
            if (IsDisposed) { return []; }
            List<string> result = [];

            result.ParseableAdd("Name", KeyName);
            result.ParseableAdd("Script", Script);
            result.ParseableAdd("QuickInfo", ColumnQuickInfo);
            result.ParseableAdd("AdminInfo", AdminInfo);
            result.ParseableAdd("Image", Image);
            result.ParseableAdd("UserGroups", UserGroups, false);

            return result;
        } catch {
            Develop.CheckStackOverflow();
            return ParseableItems();
        }
    }

    public void ParseFinished(string parsed) { }

    public virtual bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
                KeyName = value.FromNonCritical();
                return true;

            case "script":

                Script = value.FromNonCritical();
                return true;

            case "manualexecutable":
                //if (value.FromPlusMinus()) {
                //    UserGroups.Add(Constants.Administrator);
                //    UserGroups = UserGroups.SortedDistinctList();
                //}

                return true;

            case "quickinfo":
                ColumnQuickInfo = value.FromNonCritical();
                return true;

            case "admininfo":
                AdminInfo = value.FromNonCritical();
                return true;

            case "image":
                Image = value.FromNonCritical();
                return true;

            case "usergroups":
                UserGroups = value.FromNonCritical().SplitAndCutByCrToList().SortedDistinctList().AsReadOnly();
                return true;

            case "changevalues": // Todo: 08.10.2024
                //_changeValues = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public string ReadableText() {
        var t = ErrorReason();
        return !string.IsNullOrEmpty(t) ? "Fehler: " + t : KeyName;
    }

    public virtual QuickImage? SymbolForReadableText() {
        if (!this.IsOk()) { return QuickImage.Get(ImageCode.Kritisch); }
        if (!string.IsNullOrEmpty(Image)) {
            if (UserGroups.Count > 0) {
                return QuickImage.Get(Image + "|16");
            }

            return QuickImage.Get(Image + "|16|||||170");
        }

        return null;
    }

    public override string ToString() => ParseableItems().FinishParseable();

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            //if (Database != null && !Database.IsDisposed) { Database.DisposingEvent -= _database_Disposing; }
            //Database = null;

            IsDisposed = true;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}