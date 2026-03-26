// Authors:
// Christian Peter
//
// Copyright (c) 2026 Christian Peter
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

using BlueControls.Enums;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using static BlueControls.Enums.Design;
using static BlueControls.Enums.Kontur;
using static BlueControls.Enums.HintergrundArt;
using static BlueControls.Enums.RahmenArt;
using System;

namespace BlueControls.Classes;

public static class SkinWin11 {

    #region Methods

    public static void Load(Dictionary<Design, Dictionary<States, SkinDesign>> design) {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("BlueControls.Ressources.SkinWin11.xml");
        if (stream == null) { return; }

        var doc = new XmlDocument();
        doc.Load(stream);

        var root = doc.DocumentElement;
        if (root == null) { return; }

        foreach (XmlNode node in root.ChildNodes) {
            if (node.Name != "Entry") { continue; }

            var d = GetEnum<Design>(node.Attributes?["Design"]?.Value);
            var s = GetEnum<States>(node.Attributes?["State"]?.Value);

            var font = node.Attributes?["Font"]?.Value ?? string.Empty;
            var kontur = GetEnum<Kontur>(node.Attributes?["Kontur"]?.Value);
            var x1 = GetInt(node.Attributes?["X1"]?.Value);
            var y1 = GetInt(node.Attributes?["Y1"]?.Value);
            var x2 = GetInt(node.Attributes?["X2"]?.Value);
            var y2 = GetInt(node.Attributes?["Y2"]?.Value);
            var hint = GetEnum<HintergrundArt>(node.Attributes?["Hint"]?.Value);
            var bc1 = node.Attributes?["BC1"]?.Value ?? string.Empty;
            var bc2 = node.Attributes?["BC2"]?.Value ?? string.Empty;
            var rahm = GetEnum<RahmenArt>(node.Attributes?["Rahm"]?.Value);
            var boc1 = node.Attributes?["BOC1"]?.Value ?? string.Empty;
            var boc2 = node.Attributes?["BOC2"]?.Value ?? string.Empty;
            var pic = node.Attributes?["PIC"]?.Value ?? string.Empty;

            design.Add(d, s, font, kontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic);
        }
    }

    private static T GetEnum<T>(string? value) where T : struct, Enum {
        if (string.IsNullOrEmpty(value)) { return default; }
        return Enum.TryParse<T>(value, out var result) ? result : default;
    }

    private static int GetInt(string? value) {
        if (string.IsNullOrEmpty(value)) { return 0; }
        return int.TryParse(value, out var result) ? result : 0;
    }

    #endregion
}