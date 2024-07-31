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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;

namespace BlueBasics;

public abstract class ParsebleItem : IHasKeyName, IParseable, IPropertyChangedFeedback {

    #region Fields

    private string _keyName = string.Empty;

    #endregion

    #region Constructors

    protected ParsebleItem(string keyName) {
        KeyName = string.IsNullOrEmpty(keyName) ? Generic.GetUniqueKey() : keyName;
        if (string.IsNullOrEmpty(KeyName)) { Develop.DebugPrint(FehlerArt.Fehler, "Interner Name nicht vergeben."); }
    }

    #endregion

    #region Events

    public event EventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string KeyName {
        get => _keyName;
        set {
            if (_keyName == value) { return; }
            _keyName = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public static T? NewByParsing<T>(string toParse) where T : ParsebleItem {
        var ding = string.Empty;
        var name = string.Empty;

        var types = Generic.GetEnumerableOfType<T>();

        if (types.Count == 0) { return default; }

        if (toParse.StartsWith("[I]")) { toParse = toParse.FromNonCritical(); }

        var x = toParse.GetAllTags();

        foreach (var thisIt in x) {
            switch (thisIt.Key) {
                case "type":
                case "classid":
                    ding = thisIt.Value;
                    break;

                case "key":
                case "keyname":
                case "internalname":
                    name = thisIt.Value;
                    break;
            }
        }
        if (string.IsNullOrEmpty(ding)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Type unbekannt: " + toParse);
            return default;
        }
        if (string.IsNullOrEmpty(name)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Name unbekannt: " + toParse);
            return default;
        }

        foreach (var thist in types) {
            //Type[] typesx = { typeof(string) };
            //Type constructed = thist.MakeGenericType(typesx);
            //FieldInfo[] fieldInfos = constructed.GetFields(
            //    BindingFlags.NonPublic |
            //    BindingFlags.Public |
            //    BindingFlags.Static);

            //foreach (FieldInfo fi in fieldInfos) {
            //    Console.WriteLine("Name: {0}  Value: {1}", fi.Name, fi.GetValue(null));
            //}

            var v = (string)thist.GetProperty("ClassId").GetValue(null, null);

            //var properties = thist.GetProperties();

            //foreach (var property in constructed) {
            //    if (property.Name == "ClassId") {
            if (v.Equals(ding, StringComparison.OrdinalIgnoreCase)) {
                var ni = (T)Activator.CreateInstance(thist, name);
                ni.Parse(toParse);
                return ni;
            }
            //    }
            //}
        }

        return default;
    }

    public virtual void OnPropertyChanged() => PropertyChanged?.Invoke(this, System.EventArgs.Empty);

    public virtual void ParseFinished(string parsed) { }

    public abstract bool ParseThis(string key, string value);

    public override string ToString() => ToParseableString();


    public virtual string ToParseableString() {
        List<string> result = [];

        var ci = (string?)GetType().GetProperty("ClassId")?.GetValue(null, null);
        if (ci != null) {
            result.ParseableAdd("ClassId", ci);
        }
        result.ParseableAdd("Key", KeyName);

        return result.Parseable();
    }

    #endregion
}