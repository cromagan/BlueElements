﻿// Authors:
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

public abstract class ParsebleItem : IParseable, IPropertyChangedFeedback {

    #region Events

    public event EventHandler? PropertyChanged;

    #endregion

    #region Properties

    public abstract string MyClassId { get; }

    #endregion

    #region Methods

    public static T? NewByParsing<T>(string toParse) where T : ParsebleItem {
        var typeName = string.Empty;

        if (toParse.StartsWith("[I]")) { toParse = toParse.FromNonCritical(); }

        var x = toParse.GetAllTags();

        foreach (var thisIt in x) {
            switch (thisIt.Key) {
                case "type":
                case "classid":
                    typeName = thisIt.Value;
                    break;
            }
        }

        var ni = NewByTypeName<T>(typeName);
        if (ni == null) { return default; }
        ni.Parse(toParse);
        return ni;
    }

    public static T? NewByTypeName<T>(string typname) where T : ParsebleItem {
        var types = Generic.GetEnumerableOfType<T>();

        if (types.Count == 0) {
            return default;
        }

        if (string.IsNullOrEmpty(typname)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Typ unbekannt: " + typname);
            return default;
        }

        //if (string.IsNullOrEmpty(name)) {
        //    Develop.DebugPrint(FehlerArt.Fehler, "Name unbekannt: " + name);
        //    return default;
        //}

        foreach (var thist in types) {
            if (thist != null) {
                var v = (string)thist.GetProperty("ClassId")?.GetValue(null, null);

                if (v.Equals(typname, StringComparison.OrdinalIgnoreCase)) {
                    var ni = (T)Activator.CreateInstance(thist);
                    return ni;
                }
            }
        }
        return default;
    }

    public virtual void OnPropertyChanged() => PropertyChanged?.Invoke(this, System.EventArgs.Empty);

    public virtual void ParseFinished(string parsed) { }

    public abstract bool ParseThis(string key, string value);

    public virtual string ToParseableString() {
        List<string> result = [];

        var ci = (string?)GetType().GetProperty("ClassId")?.GetValue(null, null);
        if (ci != null) {
            result.ParseableAdd("ClassId", ci);
        }

        return result.Parseable();
    }

    public override string ToString() => ToParseableString();

    #endregion
}