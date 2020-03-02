using System;
using System.ComponentModel;

namespace BlueControls
{
    [AttributeUsage(AttributeTargets.All)]
    public class PropertyAttributes : DescriptionAttribute
    {
        public PropertyAttributes(string description, bool fehlerWennLeer)
        {
            this.Description = description;
            this.FehlerWennLeer = fehlerWennLeer;
        }

        public override string Description { get; }
        public bool FehlerWennLeer { get; }
    }
}
