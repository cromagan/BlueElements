namespace BlueBasics
{
    public static partial class Extensions
    {

        public static System.Windows.Forms.Padding PaddingParse(string Code)
        {


            Code = Code.RemoveChars("{}LeftTopRightBm= ");

            var w = Code.Split(',');

            var P = new System.Windows.Forms.Padding();


            P.Left = int.Parse(w[0]);
            P.Top = int.Parse(w[1]);
            P.Right = int.Parse(w[2]);
            P.Bottom = int.Parse(w[3]);

            return P;
        }

    }
}