﻿// Authors:
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;

namespace BlueDatabase;

public sealed class ImportScript : IParseable, IReadableTextWithChanging, IDisposableExtended, ICloneable, IErrorCheckable {

    #region Fields

    //private string _lastUsed;
    private string _name;

    private string _script;

    #endregion

    #region Constructors

    public ImportScript(DatabaseAbstract database, string name, string script) : this(database) {
        _name = name;
        _script = script;
    }

    public ImportScript(DatabaseAbstract database, string toParse) : this(database) => Parse(toParse);

    public ImportScript(DatabaseAbstract database) {
        Database = database;
        Database.Disposing += Database_Disposing;
        _name = string.Empty;
        _script = string.Empty;
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~ImportScript() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }
    public bool IsDisposed { get; private set; }

    public bool IsParsing { get; private set; }

    public string Name {
        get => _name;
        set {
            if (_name == value) { return; }
            _name = value;
            OnChanged();
        }
    }

    public string Script {
        get => _script;
        set {
            if (_script == value) { return; }
            _script = value;
            OnChanged();
        }
    }

    #endregion

    #region Methods

    public object Clone() => new ImportScript(Database, ToString());

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (Database?.IsDisposed ?? true) { return "Datenbank verworfen"; }

        if (string.IsNullOrEmpty(_script)) {
            return "Kein Skript angegeben.";
        }

        if (string.IsNullOrEmpty(_name)) {
            return "Kein Name angegeben.";
        }

        return string.Empty;
    }

    public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void Parse(string toParse) {
        IsParsing = true;

        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "name":
                    _name = pair.Value.FromNonCritical();
                    break;

                case "script":

                    _script = pair.Value.FromNonCritical();
                    break;

                //case "lastdone":

                //    _lastUsed = pair.Value.FromNonCritical();
                //    break;

                case "database":
                    Database = DatabaseAbstract.GetById(new ConnectionInfo(pair.Value.FromNonCritical(), null), null);
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }

        IsParsing = false;
    }

    public string ReadableText() {
        var t = ErrorReason();
        if (!string.IsNullOrEmpty(t)) {
            return "Fehler: " + t;
        }

        return Name;
    }

    public QuickImage? SymbolForReadableText() {
        if (!IsOk()) { return QuickImage.Get(ImageCode.Kritisch); }

        return null;
    }

    public override string ToString() {
        try {
            var result = "{";
            result = result + "Database=" + Database?.ConnectionData.UniqueID.ToNonCritical() + ", ";
            result = result + "Name=" + Name.ToNonCritical() + ", ";
            result = result + "Script=" + Script.ToNonCritical() + ", ";
            //result = result + "LastDone=" + _lastUsed.ToNonCritical() + ", ";

            return result.TrimEnd(", ") + "}";
        } catch {
            return ToString();
        }
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            if (Database != null) { Database.Disposing -= Database_Disposing; }
            Database = null;

            IsDisposed = true;
        }
    }

    #endregion
}