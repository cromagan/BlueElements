using System.Drawing;

namespace BlueBasics
{
    public static partial class Extensions
    {
        public static SizeF SizeFParse(string Code)
        {
            Code = Code.RemoveChars("{}WidthHeg= ");
            var w = Code.Split(',');
            return new SizeF(float.Parse(w[0]), float.Parse(w[1]));
        }

    }
}