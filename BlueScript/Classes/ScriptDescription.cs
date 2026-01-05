// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

    protected ScriptDescription(string adminInfo, string image, string name, string quickInfo, string script, ReadOnlyCollection<string> userGroups, string failedReason) {
        if (string.IsNullOrEmpty(name)) {
            name = "New script";
        }

        AdminInfo = adminInfo;
        Image = image;
        KeyName = name;
        QuickInfo = quickInfo;
        Script = script;
        UserGroups = userGroups;
        FailedReason = failedReason;
    }

    protected ScriptDescription(string name, string script) : this(string.Empty, string.Empty, name, string.Empty, script, EmptyReadOnly, string.Empty) { }

    protected ScriptDescription() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, EmptyReadOnly, string.Empty) { }

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
    public string CompareKey => KeyName;
    public string FailedReason { get; private set; }
    public string Image { get; private set; }
    public bool IsDisposed { get; private set; }
    public bool KeyIsCaseSensitive => false;
    public string KeyName { get; private set; }
    public string QuickInfo { get; private set; }
    public string Script { get; private set; }

    public ReadOnlyCollection<string> UserGroups { get; private set; }

    #endregion

    #region Methods

    public static bool IsValidName(string name) {
        if (string.IsNullOrEmpty(name)) { return false; }
        if (!name.IsFormat(FormatHolder.Text)) { return false; }
        if (string.Equals(name, "New script", StringComparison.OrdinalIgnoreCase)) { return false; }
        return true;
    }

    public abstract List<string> Attributes();

    public abstract int CompareTo(object obj);

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual string ErrorReason() {
        if (!IsValidName(KeyName)) { return "Ungültiger Name"; }
        if (!string.IsNullOrEmpty(FailedReason)) { return "Das Skript enthält Syntax-Fehler."; }
        return string.Empty;
    }

    public virtual List<string> ParseableItems() {
        try {
            if (IsDisposed) { return []; }
            List<string> result = [];

            result.ParseableAdd("Name", KeyName.Trim());
            result.ParseableAdd("Script", Script.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("QuickInfo", QuickInfo.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("AdminInfo", AdminInfo.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("Image", Image);
            result.ParseableAdd("UserGroups", UserGroups, false);
            result.ParseableAdd("FailedReason", FailedReason.Replace("\r\n", "\r").TrimEnd(' '));

            return result;
        } catch {
            Develop.AbortAppIfStackOverflow();
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
                QuickInfo = value.FromNonCritical();
                return true;

            case "admininfo":
                AdminInfo = value.FromNonCritical();
                return true;

            case "image":
                Image = value.FromNonCritical();
                return true;

            case "failedreason":
                FailedReason = value.FromNonCritical();
                return true;

            case "usergroups":
                UserGroups = value.FromNonCritical().SplitBy("|").SortedDistinctList().AsReadOnly();
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
            //if (Table != null && !Table.IsDisposed) { Table.DisposingEvent -= _table_Disposing; }
            //Table = null;

            IsDisposed = true;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}