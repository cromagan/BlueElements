// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Drawing;
using System.Text.Json.Nodes;

namespace CodeOptimizer.Plugins;

/// <summary>
/// Ersetzt Texte in allen .cs-Dateien. Die Regeln (Alt / Neu / Regex-Replace) werden in den Optionen als Tabelle gepflegt.
/// </summary>
public class ReplacePlugin : CodeOptimizerPlugin {

    #region Fields

    private const int RowHeight = 27;

    private const int RegexButtonWidth = 70;

    private const int DeleteButtonWidth = 32;

    /// <summary>
    /// Zeitlimit für reguläre Ausdrücke, damit keine Datei durch Rückverfolgung blockiert wird.
    /// </summary>
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

    private readonly List<ReplaceRule> _rules = [];

    #endregion

    #region Properties

    public override string Name => "Replace";
    public override ImageCode Symbol => ImageCode.Tabelle;
    public override string QuickInfo => "Ersetzt Texte in allen .cs-Dateien. Pro Regel werden Suchtext und Ersetzung angegeben; der Suchtext kann als regulärer Ausdruck behandelt werden.";
    public override int Order => 50;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var (rules, errors) = WorkingRules();
        var counterFiles = 0;
        var counterReplacements = 0;
        var skipped = new List<string>();

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            // Dateien ohne gültiges UTF-8 dürfen nicht umgeschrieben werden - das übernimmt das Encoding-Modul
            if (!SourceFiles.IsUtf8(file)) {
                skipped.Add(file);
                continue;
            }

            var content = File.ReadAllText(file);
            string newContent;
            int replacements;

            try {
                newContent = ReplaceAll(content, rules, out replacements);
            } catch (RegexMatchTimeoutException) {
                errors.Add(file + ": Timeout bei der Ersetzung");
                continue;
            }

            if (replacements == 0) { continue; }

            File.WriteAllText(file, newContent, new UTF8Encoding(true));
            counterFiles++;
            counterReplacements += replacements;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Replace ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert: {counterFiles}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Ersetzungen:      {counterReplacements}");
        foreach (var error in errors) {
            report.AppendLine("Fehler: " + error);
        }
        AppendSkipped(report, skipped);
        return report.ToString();
    }

    public override List<GenericControl> GetOptions(int widthOfControl) {
        var result = new List<GenericControl> {
            new FlexiControl("Alt / Neu / Regex-Replace:", widthOfControl, true)
        };

        foreach (var rule in _rules) {
            result.Add(BuildRuleRow(rule, widthOfControl));
        }

        var btnAdd = new Button {
            ImageCode = "PlusZeichen|16",
            Size = new Size(widthOfControl, RowHeight),
            Text = "Regel hinzufügen"
        };
        btnAdd.Click += BtnAdd_Click;
        result.Add(btnAdd);

        return result;
    }

    public override JsonObject SaveSettings() {
        var rules = new JsonArray();
        foreach (var rule in _rules) {
            rules.Add(rule.ToJsonObject());
        }

        var json = new JsonObject();
        json.Set("Rules", rules);
        return json;
    }

    public override void LoadSettings(JsonObject settings) {
        _rules.Clear();

        if (settings["Rules"] is not JsonArray rules) { return; }

        foreach (var rule in rules) {
            if (rule is JsonObject jsonRule) {
                _rules.Add(ReplaceRule.Parse(jsonRule));
            }
        }
    }

    /// <summary>
    /// Baut eine Tabellenzeile mit Suchtext, Ersetzung, Regex-Ja/Nein und Löschen-Knopf.
    /// </summary>
    private ScrollPanel BuildRuleRow(ReplaceRule rule, int width) {
        var contentWidth = width - 12;
        var wCell = (contentWidth - 16 - RegexButtonWidth - DeleteButtonWidth) / 2;
        var pnl = new ScrollPanel {
            Size = new Size(width, RowHeight)
        };

        var txbSearch = new TextBox {
            Location = new Point(0, 0),
            Size = new Size(wCell, RowHeight),
            Tag = rule,
            Text = rule.Alt
        };
        txbSearch.TextChanged += RuleSearch_TextChanged;
        pnl.Controls.Add(txbSearch);

        var txbReplace = new TextBox {
            Location = new Point(wCell + 4, 0),
            Size = new Size(wCell, RowHeight),
            Tag = rule,
            Text = rule.Neu
        };
        txbReplace.TextChanged += RuleReplace_TextChanged;
        pnl.Controls.Add(txbReplace);

        var btnRegex = new Button {
            ButtonStyle = ButtonStyle.Yes_or_No,
            Checked = rule.IsRegex,
            Location = new Point(wCell * 2 + 8, 0),
            Size = new Size(RegexButtonWidth, RowHeight),
            Tag = rule
        };
        btnRegex.Click += RuleRegex_Click;
        pnl.Controls.Add(btnRegex);

        var btnDelete = new Button {
            ImageCode = "MinusZeichen|16",
            Location = new Point(contentWidth - DeleteButtonWidth, 0),
            Size = new Size(DeleteButtonWidth, RowHeight),
            Tag = rule
        };
        btnDelete.Click += RuleDelete_Click;
        pnl.Controls.Add(btnDelete);

        return pnl;
    }

    /// <summary>
    /// Liefert alle Regeln mit gültigem Suchtext; ungültige reguläre Ausdrücke werden gemeldet.
    /// </summary>
    private (List<(ReplaceRule Rule, Regex? Expression)> Rules, List<string> Errors) WorkingRules() {
        var rules = new List<(ReplaceRule, Regex?)>();
        var errors = new List<string>();

        foreach (var rule in _rules) {
            if (rule.Alt is not { Length: > 0 }) { continue; }

            if (!rule.IsRegex) {
                rules.Add((rule, null));
                continue;
            }

            try {
                rules.Add((rule, new Regex(rule.Alt, RegexOptions.CultureInvariant, RegexTimeout)));
            } catch (ArgumentException) {
                errors.Add(rule.Alt);
            }
        }

        return (rules, errors);
    }

    private static string ReplaceAll(string content, List<(ReplaceRule Rule, Regex? Expression)> rules, out int replacements) {
        var result = content;
        replacements = 0;

        foreach (var (rule, expression) in rules) {
            if (expression is not null) {
                replacements += expression.Count(result);
                result = expression.Replace(result, rule.Neu);
            } else {
                replacements += CountOf(result, rule.Alt);
                result = result.Replace(rule.Alt, rule.Neu, StringComparison.Ordinal);
            }
        }

        return result;
    }

    private static int CountOf(string text, string search) {
        var count = 0;
        var pos = text.IndexOf(search, StringComparison.Ordinal);
        while (pos >= 0) {
            count++;
            pos = text.IndexOf(search, pos + search.Length, StringComparison.Ordinal);
        }
        return count;
    }

    private void BtnAdd_Click(object? sender, EventArgs e) {
        _rules.Add(new ReplaceRule(string.Empty, string.Empty, false));
        OnOptionsChanged();
    }

    private static void RuleSearch_TextChanged(object? sender, EventArgs e) {
        if (sender is TextBox { Tag: ReplaceRule rule } txb) {
            rule.Alt = txb.Text;
        }
    }

    private static void RuleReplace_TextChanged(object? sender, EventArgs e) {
        if (sender is TextBox { Tag: ReplaceRule rule } txb) {
            rule.Neu = txb.Text;
        }
    }

    private static void RuleRegex_Click(object? sender, EventArgs e) {
        if (sender is Button { Tag: ReplaceRule rule } btn) {
            rule.IsRegex = btn.Checked;
        }
    }

    private void RuleDelete_Click(object? sender, EventArgs e) {
        if (sender is Button { Tag: ReplaceRule rule }) {
            _rules.Remove(rule);
            OnOptionsChanged();
        }
    }

    #endregion
}
