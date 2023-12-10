using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BlueBasics;

public class Html {

    #region Constructors

    // https://www.w3schools.com/html/html_tables.asp
    public Html(string title) =>
        Code = new List<string> {
            "<!DOctypex HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"",
            "\"http://www.w3.org/TR/html4/strict.dtd\">",
            "<html>",
            "  <head>",
            "    <title>" + title + "</title>",
            "    <style type=\"text/css\">",
            "      table {",
            // Code.Add("              border-spacing: 5px;");// If the table has collapsed borders, border-spacing has no effect.
            "              border: 1px solid gray;",
            "              border-collapse: collapse;",
            "            }",
            "      td {",
            "          padding: 3px;",
            "          border: 1px solid black;",
            "          border-collapse: collapse;",
            "          }",
            "      th {",
            "          padding: 3px;",
            "          border: 1px solid black;",
            "          border-collapse: collapse;",
            "          text-align: left;",
            "          text-valign: middle;",
            "          font-family: Arial, Helvetica, sans-serif;",
            "          font-size: 12px;",
            "          }",
            "      p {",
            "          font-family: Arial, Helvetica, sans-serif;",
            "          font-size: 12px;",
            "          }",
            "    </style>",
            "  </head>",
            "<body>"
        };

    #endregion

    #region Properties

    private List<string> Code { get; }

    #endregion

    #region Methods

    public void Add(string what) => Code.Add(what);

    public void AddCaption(string caption) => AddCaption(caption, 1);

    public void AddCaption(string caption, int size) {
        switch (size) {
            case 1:
                Code.Add("  <h1>" + caption + "</h1><br>");
                break;

            case 2:
                Code.Add("  <h2>" + caption + "</h2><br>");
                break;

            case 3:
                Code.Add("  <h3>" + caption + "</h3><br>");
                break;

            default:
                Develop.DebugPrint("Size nicht definert");
                break;
        }
    }

    public void AddFoot() {
        Code.Add("  </body>");
        Code.Add("</html>");
    }

    public void CellAdd(string content) => Code.Add("              <th>" + content + "</th>");

    public void CellAdd(string content, Color c) => Code.Add("        <th  bgcolor=\"#" + c.ToHtmlCode() + "\">" + content + "</th>");

    public void RowBeginn() => Code.Add("      <tr>");

    public void RowEnd() => Code.Add("      </tr>");

    public bool Save(string filename, bool executeafter) => Code.WriteAllText(filename, Encoding.UTF8, executeafter);

    public void TableBeginn() {
        // da.Add("  <Font face=\"Arial\" Size=\"2\"><table border=\"1\" BORDERCOLOR=\"#aaaaaa\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\">");
        Code.Add("<Font face=\"Arial\" Size=\"2\">");
        Code.Add("  <table>");
        // da.Add("  <Font face=\"Arial\" Size=\"2\"><table border=\"1\" BORDERCOLOR=\"#aaaaaa\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\">");
        // Code.Add("      </tr>");
    }

    public void TableEnd() => Code.Add("    </table>");

    #endregion
}