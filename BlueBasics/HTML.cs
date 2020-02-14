using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueBasics
{
    public class HTML
    {


        private List<string> Code { get; set; }


        // https://www.w3schools.com/html/html_tables.asp


        public HTML(string Title)
        {
            Code = new List<string>();

            // DebugPrint_Disposed(disposedValue)
            Code.Add("<!DOctypex HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"");
            Code.Add("\"http://www.w3.org/TR/html4/strict.dtd\">");
            Code.Add("<html>");
            Code.Add("  <head>");
            Code.Add("    <title>" + Title + "</title>");

            Code.Add("    <style type=\"text/css\">");

            Code.Add("      table {");
            //Code.Add("              border-spacing: 5px;");// If the table has collapsed borders, border-spacing has no effect.
            Code.Add("              border: 1px solid gray;");
            Code.Add("              border-collapse: collapse;");
            Code.Add("            }");

            Code.Add("      td {");
            Code.Add("          padding: 3px;");
            Code.Add("          border: 1px solid black;");
            Code.Add("          border-collapse: collapse;");
            Code.Add("          }");

            Code.Add("      th {");
            Code.Add("          padding: 3px;");
            Code.Add("          border: 1px solid black;");
            Code.Add("          border-collapse: collapse;");
            Code.Add("          text-align: left;");
            Code.Add("          text-valign: middle;");
            Code.Add("          font-family: Arial, Helvetica, sans-serif;");
            Code.Add("          font-size: 12px;");
            Code.Add("          }");


            Code.Add("      p {");
            Code.Add("          font-family: Arial, Helvetica, sans-serif;");
            Code.Add("          font-size: 12px;");
            Code.Add("          }");

            Code.Add("    </style>");

            Code.Add("  </head>");




            Code.Add("<body>");

        }


        public void AddFoot()
        {

            Code.Add("  </body>");
            Code.Add("</html>");
        }



        public void AddCaption(string _Caption)
        {
            AddCaption(_Caption, 1);
        }

        public void Add(string what)
        {
            Code.Add(what);
        }


        public void RowBeginn()
        {
            Code.Add("      <tr>");
        }

        public void RowEnd()
        {
            Code.Add("      </tr>");
        }

        public void Save(string filename, bool executeafter)
        {
            Code.Save(filename, executeafter);
        }


        public void TableBeginn()
        {

            //da.Add("  <Font face=\"Arial\" Size=\"2\"><table border=\"1\" BORDERCOLOR=\"#aaaaaa\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\">");

            Code.Add("<Font face=\"Arial\" Size=\"2\">");

            Code.Add("  <table>");


            //da.Add("  <Font face=\"Arial\" Size=\"2\"><table border=\"1\" BORDERCOLOR=\"#aaaaaa\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\">");


            //Code.Add("      </tr>");
        }

        public void TableEnd()
        {
            Code.Add("    </table>");
        }

        public void CellAdd(string content)
        {
            Code.Add("              <th>" + content + "</th>");
        }

        public void CellAdd(string content, Color c)
        {
            Code.Add("        <th  bgcolor=\"#" + c.ToHTMLCode() + "\">" + content + "</th>");
        }

        public void AddCaption(string _caption, int size)
        {

            switch (size)
            {
                case 1:
                    Code.Add("  <h1>" + _caption + "</h1><br>");
                    break;

                case 2:
                    Code.Add("  <h2>" + _caption + "</h2><br>");
                    break;
                case 3:
                    Code.Add("  <h3>" + _caption + "</h3><br>");
                    break;

                default:
                    Develop.DebugPrint("Size nicht definert");
                    break;
            }

        }

        public void ListAdd(List<string> items)
        {
            Code.Add("<ul>");

            foreach (var thisitem in items)
            {
                Code.Add("  <li>" + thisitem + "</li>");

            }


            Code.Add("</ul>");
        }
    }
}
