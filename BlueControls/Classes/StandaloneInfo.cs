// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.Enums;
using System;

namespace BlueControls.Classes;

public class StandaloneInfo : Attribute {

    #region Constructors

    public StandaloneInfo(string name, ImageCode symbol, string kategorie, string quickInfo, int sort) {
        Name = name;
        Image = QuickImage.Get(symbol, 48);
        Kategorie = kategorie;
        Sort = sort;
        QuickInfo = quickInfo;
    }

    public StandaloneInfo(string name, QuickImage image, string kategorie, string quickInfo, int sort) {
        Name = name;
        Image = image;
        Kategorie = kategorie;
        Sort = sort;
        QuickInfo = quickInfo;
    }

    public StandaloneInfo(string name, string image, string kategorie, string quickInfo, int sort) {
        Name = name;
        Image = QuickImage.Get(image);
        Kategorie = kategorie;
        Sort = sort;
        QuickInfo = quickInfo;
    }

    #endregion

    #region Properties

    public QuickImage Image { get; }
    public string Kategorie { get; }
    public string Name { get; }
    public string QuickInfo { get; }
    public int Sort { get; }

    #endregion
}