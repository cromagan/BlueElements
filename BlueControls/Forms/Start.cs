// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using System.Reflection;

namespace BlueControls.Forms;

public partial class Start : FormWithStatusBar, IUniqueWindow {

    #region Constructors

    public Start() : base() {
        InitializeComponent();

        //var types = Generic.GetTypesOfType<IIsStandalone>();

        var methods = Generic.GetMethodsWithAttribute<StandaloneInfo>();

        foreach (var thisType in methods) {
            var name = thisType.Name;
            var i = QuickImage.Get(ImageCode.Fragezeichen);
            var kat = "Sonstiges";
            var sort = 200;
            var quickInfo = string.Empty;

            if (thisType.GetCustomAttribute<StandaloneInfo>() is { } attr) {
                name = attr.Name;
                i = attr.Image;
                kat = attr.Kategorie;
                sort = attr.Sort;
                quickInfo = attr.QuickInfo;
            }

            if (Forms[kat] is { } cap) {
                var tmp = Math.Min(IntParse(cap.UserDefCompareKey) / 10, sort);
                cap.UserDefCompareKey = tmp.ToString10() + "0";
            } else {
                var pk = new TextListItem(kat, kat, null, true, true, quickInfo, sort.ToString10() + "0");
                Forms.ItemAdd(pk);
            }

            var methodKey = (thisType.DeclaringType?.FullName ?? thisType.DeclaringType?.Name ?? string.Empty) + "." + thisType.Name;
            var bli = new BitmapListItem(i, methodKey, name, quickInfo) {
                Padding = 5,
                UserDefCompareKey = sort.ToString10() + "1" + name
            };

            //var p = new TextListItem(name, string.Empty, QuickImage.Get(i, 24), false, true, sort.ToString10() + "1" + name) {
            //    Tag = thisType
            //};
            Forms.ItemAdd(bli);
        }
    }

    #endregion

    #region Properties

    public object? Object { get; set; }

    #endregion

    #region Methods

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        BringToFront();
    }

    private void Forms_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        if (e.Item is BitmapListItem bli) {
            var methodInfo = Generic.GetMethodsWithAttribute<StandaloneInfo>().FirstOrDefault(m =>
                (m.DeclaringType?.FullName ?? m.DeclaringType?.Name ?? string.Empty) + "." + m.Name == bli.KeyName);
            if (methodInfo is { } mi) {
                var result = mi.Invoke(null, null);
                if (result is Form form) {
                    FormManager.RegisterForm(form);
                    form.Show();
                    Close();
                    form.BringToFront();
                }
            }
        }
    }

    #endregion
}