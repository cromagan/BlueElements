﻿// Authors:
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

using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using static BlueBasics.Develop;
using BlueDatabase;
using BlueBasics;
using static BlueBasics.Converter;
using BlueBasics.Interfaces;
using BlueControls.ItemCollection;
using static BlueBasics.FileOperations;

namespace BlueControls.ConnectedFormula {

    public class ConnectedFormula : BlueBasics.MultiUserFile.MultiUserFile, IChangedFeedback {

        #region Fields

        public const string Version = "0.01";
        public static readonly float StandardHöhe = 3.5f;

        //public static readonly float Umrechnungsfaktor = Converter.PixelToMm(1, 300);
        public static readonly float Umrechnungsfaktor2 = MmToPixel(StandardHöhe, 300) / 44;

        public readonly ListExt<string> DatabaseFiles = new();
        //public readonly List<Database?> Databases = new();

        public readonly ListExt<string> NotAllowedChilds = new();
        private string _createDate = string.Empty;
        private string _creator = string.Empty;
        private int _id = -1;
        private ItemCollectionPad _padData;

        private bool _saved = true;
        private bool _saving = false;

        #endregion

        #region Constructors

        public ConnectedFormula() : this(string.Empty) {
        }

        private ConnectedFormula(string filename) : base(false, false) {
            _createDate = DateTime.Now.ToString(Constants.Format_Date5);
            _creator = Generic.UserName();
            PadData = new ItemCollectionPad();

            if (FileExists(filename)) {
                Load(filename, true);
            }
            //if (notallowedchilds != null) {
            //    NotAllowedChilds.AddIfNotExists(notallowedchilds);
            //}
            _saved = true;

            DatabaseFiles.Changed += DatabaseFiles_Changed;
            NotAllowedChilds.Changed += NotAllowedChilds_Changed;
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public ItemCollectionPad PadData {
            get => _padData;
            private set {
                if (_padData == value) { return; }

                if (_padData != null) {
                    PadData.Changed -= PadData_Changed;
                }
                _padData = value;
                if (_padData != null) {
                    PadData.Changed += PadData_Changed;
                }

                if (_saving || IsParsing || IsLoading) { return; }

                _saved = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="checkOnlyFilenameToo"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public static ConnectedFormula? GetByFilename(string filename) {
            var tmpCF = GetByFilename(filename, false);

            if (tmpCF is ConnectedFormula cf) { return cf; }

            if (tmpCF != null) { return null; }//  Daten im Speicher, aber keine Datenbank!

            return !FileExists(filename) ? null : new ConnectedFormula(filename);
        }

        //public string PadData {
        //    get => _padData;
        //    set {
        //        if (value == _padData) { return; }
        //        _padData = value;
        //        _saved = false;
        //        OnChanged();
        //    }
        //}
        public override void DiscardPendingChanges() => _saved = true;

        public override bool HasPendingChanges() => !_saved;

        public int NextID() {
            _id++;
            _saved = false;
            return _id;
        }

        public void OnChanged() {
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public override void RepairAfterParse() { }

        protected override void DoBackGroundWork(BackgroundWorker listenToMyCancel) { }

        protected override void DoWorkAfterSaving() => _saved = true;

        protected override bool IsThereBackgroundWorkToDo() => false;

        protected override void ParseExternal(byte[] bLoaded) {
            var ToParse = bLoaded.ToStringWin1252();
            if (string.IsNullOrEmpty(ToParse)) { return; }

            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key.ToLower()) {
                    case "type":
                        break;

                    case "version":
                        break;

                    //case "filepath":
                    //    FilePath = pair.Value.FromNonCritical();
                    //    break;

                    case "databasefiles":
                        DatabaseFiles.Clear();
                        DatabaseFiles.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                        break;

                    case "notallowedchilds":
                        NotAllowedChilds.Clear();
                        NotAllowedChilds.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                        break;

                    case "createdate":
                        _createDate = pair.Value.FromNonCritical();
                        break;

                    case "createname":
                        _creator = pair.Value.FromNonCritical();
                        break;

                    case "paditemdata":
                        PadData = new ItemCollectionPad(pair.Value.FromNonCritical(), string.Empty);
                        break;

                    case "lastusedid":
                        _id = IntParse(pair.Value);
                        break;

                    default:
                        DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        protected override byte[] ToListOfByte() {

            #region ein bischen aufräumen zuvor

            _saving = true;
            PadData.Sort();

            _id = -1;

            DatabaseFiles.Clear();

            foreach (var thisit in PadData) {
                if (thisit is RowWithFilterPaditem rwf) {
                    DatabaseFiles.AddIfNotExists(rwf.Database.Filename);
                    _id = Math.Max(_id, rwf.Id);
                }
            }
            _saving = false;

            #endregion

            var t = new List<string>();

            t.Add("Type=ConnectedFormula");
            t.Add("Version=" + Version);
            t.Add("CreateDate=" + _createDate.ToNonCritical());
            t.Add("CreateName=" + _creator.ToNonCritical());
            //t.Add("FilePath=" + FilePath.ToNonCritical());
            t.Add("LastUsedID=" + _id.ToString());
            t.Add("DatabaseFiles=" + DatabaseFiles.JoinWithCr().ToNonCritical());
            t.Add("NotAllowedChilds=" + NotAllowedChilds.JoinWithCr().ToNonCritical());
            t.Add("PadItemData=" + PadData.ToString().ToNonCritical());

            return ("{" + t.JoinWith(", ").TrimEnd(", ") + "}").WIN1252_toByte();
        }

        private void DatabaseFiles_Changed(object sender, System.EventArgs e) {
            if (_saving || IsParsing || IsLoading) { return; }

            foreach (var thisfile in DatabaseFiles) {
                Database.GetByFilename(thisfile, false, false);
            }

            _saved = false;
        }

        private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
            if (_saving || IsParsing || IsLoading) { return; }
            _saved = false;
        }

        private void PadData_Changed(object sender, System.EventArgs e) {
            if (_saving || IsParsing ||IsLoading) { return; }

            _saved = false;
        }

        #endregion
    }
}