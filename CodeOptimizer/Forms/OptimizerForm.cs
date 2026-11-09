// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Forms;

public sealed partial class OptimizerForm : Form {

    #region Constructors

    public OptimizerForm() {
        InitializeComponent();

        FillPluginList();
        txbPath.Text = Environment.CurrentDirectory;
    }

    #endregion

    #region Methods

    private void BtnStart_Click(object? sender, EventArgs e) {
        if (txbPath.Text is not { Length: > 0 } rootPath || !Directory.Exists(rootPath)) {
            MessageBox.Show("Bitte einen gültigen Ordner angeben.", ImageCode.Warnung, "Hinweis");
            return;
        }

        var plugins = CheckedPlugins();
        if (plugins.Count == 0) {
            MessageBox.Show("Kein Modul angewählt.", ImageCode.Warnung, "Hinweis");
            return;
        }

        var report = new StringBuilder();

        foreach (var plugin in plugins) {
            report.AppendLine();
            report.AppendLine(plugin.Execute(rootPath));
        }

        MessageBox.Show(report.ToString(), ImageCode.Information, "Fertig");
    }

    private List<CodeOptimizerPlugin> CheckedPlugins() {
        var result = new List<CodeOptimizerPlugin>();

        foreach (var item in lstPlugins.CheckedItems) {
            if (item is ReadableListItem { Item: CodeOptimizerPlugin plugin }) { result.Add(plugin); }
        }

        result.Sort();
        return result;
    }

    private void FillPluginList() {
        var plugins = CodeOptimizerPlugin.AllPlugins.Instances.ToList();
        plugins.Sort();

        foreach (var plugin in plugins) {
            lstPlugins.ItemAdd(new ReadableListItem(plugin, true, plugin.KeyName));
        }
    }

    private void LstPlugins_ItemClicked(object? sender, ListItemEventArgs e) {
        if (e.Item is not ReadableListItem { Item: CodeOptimizerPlugin plugin }) { return; }

        ShowOptions(plugin);
    }

    private void ShowOptions(CodeOptimizerPlugin plugin) {
        for (var i = pnlOptions.Controls.Count - 1; i >= 0; i--) {
            var control = pnlOptions.Controls[i];
            pnlOptions.Controls.RemoveAt(i);
            control.Dispose();
        }

        var options = plugin.GetOptions(pnlOptions.Width);

        if (options.Count == 0) {
            options.Add(new FlexiControl("Beschreibung:", pnlOptions.Width, true));
            options.Add(new FlexiControl(plugin.QuickInfo, pnlOptions.Width, false));
        }

        pnlOptions.ChildLayout = ChildLayout.StackVertical | ChildLayout.FullWidth | ChildLayout.Slider;

        foreach (var option in options) {
            if (option is { IsDisposed: false }) {
                pnlOptions.Controls.Add(option);
            }
        }
    }

    #endregion
}
