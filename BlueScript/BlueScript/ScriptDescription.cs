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
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.Converter;

namespace BlueDatabase;

public abstract class ScriptDescription : IParseable, IReadableTextWithChangingAndKey, IDisposableExtended, IErrorCheckable, IHasKeyName, IChangedFeedback {

    #region Fields

    private string _admininfo;
    private string _imagecode;
    private bool _manualexecutable;
    private string _quickinfo;
    private string _script;
    private List<string> _usergroups;

    #endregion

    #region Constructors

    public ScriptDescription(string name, string script) : this() {
        KeyName = name;
        _script = script;
    }

    public ScriptDescription() {
        KeyName = string.Empty;
        _script = string.Empty;
        _manualexecutable = false;
        _admininfo = string.Empty;
        _quickinfo = string.Empty;
        _usergroups = new List<string>();
        _imagecode = string.Empty;
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~ScriptDescription() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public string AdminInfo {
        get => _admininfo;
        set {
            if (IsDisposed) { return; }
            if (_admininfo == value) { return; }
            _admininfo = value;
            OnChanged();
        }
    }

    public string ImageCode {
        get => _imagecode;
        set {
            if (IsDisposed) { return; }
            if (_imagecode == value) { return; }
            _imagecode = value;
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    public string KeyName {
        get => KeyName;
        private set {
            if (IsDisposed) { return; }
            if (KeyName == value) { return; }
            KeyName = value;
            OnChanged();
        }
    }

    public bool ManualExecutable {
        get => _manualexecutable;
        set {
            if (IsDisposed) { return; }
            if (_manualexecutable == value) { return; }
            _manualexecutable = value;
            OnChanged();
        }
    }

    public string QuickInfo {
        get => _quickinfo;
        set {
            if (IsDisposed) { return; }
            if (_quickinfo == value) { return; }
            _quickinfo = value;
            OnChanged();
        }
    }

    public string ScriptText {
        get => _script;
        set {
            if (IsDisposed) { return; }
            if (_script == value) { return; }
            _script = value;
            OnChanged();
        }
    }

    public List<string> UserGroups {
        get => _usergroups;
        set {
            if (IsDisposed) { return; }
            if (!_usergroups.IsDifferentTo(value)) { return; }
            _usergroups = value;
            OnChanged();
        }
    }

    #endregion

    #region Methods

    public abstract List<string> Attributes();

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (string.IsNullOrEmpty(KeyName)) { return "Kein Name angegeben."; }

        if (!KeyName.IsFormat(FormatHolder.Text)) { return "Ungültiger Name"; }

        return string.Empty;
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public virtual void Parse(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "name":
                    KeyName = pair.Value.FromNonCritical();
                    break;

                case "script":

                    _script = pair.Value.FromNonCritical();
                    break;

                case "manualexecutable":

                    _manualexecutable = pair.Value.FromPlusMinus();
                    break;

                case "quickinfo":
                    _quickinfo = pair.Value.FromNonCritical();
                    break;

                case "admininfo":
                    _admininfo = pair.Value.FromNonCritical();
                    break;

                case "image":
                    _imagecode = pair.Value.FromNonCritical();
                    break;

                case "usergroups":
                    _usergroups = pair.Value.FromNonCritical().SplitAndCutByCrToList();
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    public string ReadableText() {
        var t = ErrorReason();
        if (!string.IsNullOrEmpty(t)) {
            return "Fehler: " + t;
        }

        return KeyName;
    }

    public QuickImage SymbolForReadableText() {
        if (!this.IsOk()) { return QuickImage.Get(BlueBasics.Enums.ImageCode.Kritisch); }

        var symb = BlueBasics.Enums.ImageCode.Formel;
        var c = Color.Transparent;

        if (_manualexecutable) {
            c = Color.Yellow;
            symb = BlueBasics.Enums.ImageCode.Person;
        }

        return QuickImage.Get(symb, 16, c, Color.Transparent);
    }

    public override string ToString() {
        try {
            if (IsDisposed) { return string.Empty; }
            List<string> result = new();

            result.ParseableAdd("Name", KeyName);
            result.ParseableAdd("Script", ScriptText);
            result.ParseableAdd("ManualExecutable", ManualExecutable);
            result.ParseableAdd("QuickInfo", QuickInfo);
            result.ParseableAdd("AdminInfo", AdminInfo);
            result.ParseableAdd("Image", ImageCode);
            result.ParseableAdd("UserGroups", UserGroups);

            return result.Parseable();
        } catch {
            Develop.CheckStackForOverflow();
            return ToString();
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            //if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
            //Database = null;

            IsDisposed = true;
        }
    }

    #endregion
}