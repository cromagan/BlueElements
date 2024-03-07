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

using System;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueScript;

public abstract class ScriptDescription : IParseable, IReadableTextWithChangingAndKey, IDisposableExtended, IErrorCheckable, IHasKeyName, IChangedFeedback, IComparable {

    #region Fields

    private string _admininfo;

    private bool _changeValues;

    private string _image;

    private string _keyName;

    private string _quickinfo;

    private string _scriptText;

    private List<string> _usergroups;

    #endregion

    #region Constructors

    protected ScriptDescription(string name, string script) {
        _keyName = name;
        _scriptText = script;

        _admininfo = string.Empty;
        _quickinfo = string.Empty;
        _usergroups = [];
        _image = string.Empty;
    }

    protected ScriptDescription() : this(string.Empty, string.Empty) { }

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
        //TODO: Implementieren
        get => _admininfo;
        set {
            if (IsDisposed) { return; }
            if (_admininfo == value) { return; }
            _admininfo = value;
            OnChanged();
        }
    }

    public bool ChangeValues {
        get => _changeValues;
        set {
            if (IsDisposed) { return; }
            if (_changeValues == value) { return; }
            _changeValues = value;
            OnChanged();
        }
    }

    public string CompareKey => KeyName;

    public string Image {
        get => _image;
        set {
            if (IsDisposed) { return; }
            if (_image == value) { return; }
            _image = value;
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    public string KeyName {
        get => _keyName;
        set {
            if (IsDisposed) { return; }
            if (_keyName == value) { return; }
            _keyName = value;
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
        get => _scriptText;
        set {
            if (IsDisposed) { return; }
            if (_scriptText == value) { return; }
            _scriptText = value;
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

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void ParseFinished(string parsed) { }

    public virtual bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
                _keyName = value.FromNonCritical();
                return true;

            case "script":

                _scriptText = value.FromNonCritical();
                return true;

            case "manualexecutable":
                if (value.FromPlusMinus()) {
                    _usergroups.Add(Constants.Administrator);
                    _usergroups = _usergroups.SortedDistinctList();
                }

                return true;

            case "quickinfo":
                _quickinfo = value.FromNonCritical();
                return true;

            case "admininfo":
                _admininfo = value.FromNonCritical();
                return true;

            case "image":
                _image = value.FromNonCritical();
                return true;

            case "usergroups":
                _usergroups.AddRange(value.FromNonCritical().SplitAndCutByCrToList());
                _usergroups = _usergroups.SortedDistinctList();
                return true;

            case "changevalues":
                _changeValues = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public string ReadableText() {
        var t = ErrorReason();
        if (!string.IsNullOrEmpty(t)) {
            return "Fehler: " + t;
        }

        return KeyName;
    }

    public virtual QuickImage? SymbolForReadableText() {
        if (!this.IsOk()) { return QuickImage.Get(ImageCode.Kritisch); }
        if (!string.IsNullOrEmpty(_image)) {
            if (_usergroups.Count > 0) {
                return QuickImage.Get(_image + "|16");
            } else {
                return QuickImage.Get(_image + "|16|||||170");
            }
        }
        //if (_manualexecutable) {            return QuickImage.Get(ImageCode.Person, 16, Color.Yellow, Color.Transparent);        }
        return null;
    }

    public override string ToString() {
        try {
            if (IsDisposed) { return string.Empty; }
            List<string> result = [];

            result.ParseableAdd("Name", _keyName);
            result.ParseableAdd("Script", _scriptText);
            //result.ParseableAdd("ManualExecutable", _manualexecutable);
            result.ParseableAdd("ChangeValues", _changeValues);
            result.ParseableAdd("QuickInfo", _quickinfo);
            result.ParseableAdd("AdminInfo", _admininfo);
            result.ParseableAdd("Image", _image);
            result.ParseableAdd("UserGroups", _usergroups);

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
            //if (Database != null && !Database.IsDisposed) { Database.DisposingEvent -= _database_Disposing; }
            //Database = null;

            IsDisposed = true;
        }
    }

    #endregion
}