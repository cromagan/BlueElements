using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueScript
{
    public class Script
    {

        string _script = string.Empty;


        public string ScriptText
        {
            get
            {
                return _script;
            }
            set
            {
                _script = value;
            }
        }


    }
}
