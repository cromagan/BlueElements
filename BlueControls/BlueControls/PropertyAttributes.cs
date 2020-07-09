using System;
using System.ComponentModel;

namespace BlueControls
{
    [AttributeUsage(AttributeTargets.All)]
    public class PropertyAttributes : DescriptionAttribute
    {
        public PropertyAttributes(string description, bool fehlerWennLeer)
        {
            Description = description;
            FehlerWennLeer = fehlerWennLeer;
        }

        public override string Description { get; }
        public bool FehlerWennLeer { get; }
    }
}
