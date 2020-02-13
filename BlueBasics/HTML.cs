using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueBasics
{
  public  class HTML
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
            Code.Add("  <Font face=\"Arial\" Size=\"7\">" + _Caption + "</h1><br>");
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
            Code.Add("      </tr>");
        }


    }
}
