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

using BlueBasics.ClassesStatic;
using System;
using System.Text;
using System.Text.Json.Nodes;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes.FileSystemCaching;

public sealed class CachedBlockFile : CachedFile {

    #region Fields

    private static readonly object _forLock = new();

    #endregion

    #region Constructors

    private CachedBlockFile(string filename) : base(filename) { }

    #endregion

    #region Properties

    public double AgeSeconds {
        get {
            var fi = GetFileInfo(Filename, false, 0.1f);
            if (fi == null) { return -1; }

            return Math.Max(0, DateTime.UtcNow.Subtract(fi.CreationTimeUtc).TotalSeconds);
        }
    }

    public string App { get; private set; } = string.Empty;
    public override bool ExtendedSave => false;
    public string Id { get; private set; } = string.Empty;
    public bool IsExpired => AgeSeconds is < 0 or > 3600;
    public string MachineName { get; private set; } = string.Empty;
    public override bool MustZipped => false;
    public int ThreadId { get; private set; }

    public DateTime TimeUtc { get; private set; } = DateTime.MinValue;

    public string User { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public static void AcquireWriteAccessFor(string filename) {
        lock (_forLock) {
            var bf = For(filename);
            bf.Write();
            CachedFileSystem.Register(bf);
        }
    }

    public static CachedBlockFile For(string filename) {
        var blkName = GetBlockFilename(filename);

        lock (_forLock) {
            var existing = CachedFileSystem.Get<CachedBlockFile>(blkName);
            if (existing is { IsDisposed: false }) {
                if (existing.IsStale()) { existing.Invalidate(); }
                return existing;
            }

            if (!FileExists(blkName)) { return new CachedBlockFile(blkName); }

            return CachedFileSystem.Register(new CachedBlockFile(blkName));
        }
    }

    public static string GetBlockFilename(string filename) =>
        string.IsNullOrEmpty(filename) ? string.Empty :
        filename.FilePath() + filename.FileNameWithoutSuffix() + ".blk";

    public static void RevokeWriteAccessFor(string filename) {
        var blkName = GetBlockFilename(filename);
        DeleteFile(blkName, false);
    }

    public void Delete() {
        DeleteFile(GetBlockFilename(Filename), false);
        User = string.Empty;
        TimeUtc = DateTime.MinValue;
        MachineName = string.Empty;
        App = string.Empty;
        Id = string.Empty;
        ThreadId = 0;
    }

    public override string ReadableText() => $"BlockFile '{Filename}'";

    public void Write() {
        User = UserName;
        TimeUtc = DateTime.UtcNow;
        ThreadId = Environment.CurrentManagedThreadId;
        MachineName = Environment.MachineName;
        App = Develop.AppExe();
        Id = MyId;

        var json = new JsonObject {
            ["user"] = User,
            ["timeUtc"] = TimeUtc,
            ["threadId"] = ThreadId,
            ["machineName"] = MachineName,
            ["app"] = App,
            ["id"] = Id
        }.ToJsonString();

        Content = Encoding.UTF8.GetBytes(json);
        _ = Save().GetAwaiter().GetResult();
    }

    protected override void OnLoaded() {
        User = string.Empty;
        TimeUtc = DateTime.MinValue;
        MachineName = string.Empty;
        App = Develop.AppExe();
        Id = MyId;
        ThreadId = 0;

        var content = Content;
        if (content.Length == 0) { return; }

        try {
            var json = Encoding.UTF8.GetString(content);
            var data = JsonNode.Parse(json) as JsonObject;
            if (data == null) { return; }
            User = data["user"]?.GetValue<string>() ?? string.Empty;
            TimeUtc = data["timeUtc"]?.GetValue<DateTime>() ?? DateTime.MinValue;
            MachineName = data["machineName"]?.GetValue<string>() ?? string.Empty;
            App = data["app"]?.GetValue<string>() ?? string.Empty;
            Id = data["id"]?.GetValue<string>() ?? string.Empty;
            ThreadId = data["threadId"]?.GetValue<int>() ?? 0;
        } catch {
            TimeUtc = DateTime.MinValue;
        }
    }

    #endregion
}