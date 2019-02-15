using System.Text;

namespace BlueBasics
{
    public static partial class Extensions
    {
        public static string ToStringConvert(this byte[] b)
        {
            return Encoding.Default.GetString(b);
        }
    }
}