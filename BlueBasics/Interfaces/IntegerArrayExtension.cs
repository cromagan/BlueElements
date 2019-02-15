namespace BlueBasics.Interfaces
{
    public static class IntegerArrayExtension
    {

        public static string[] ToStringArray(this int[] Ar)
        {

            var sar = new string[Ar.GetUpperBound(0) + 1];

            for (var z = 0 ; z <= Ar.GetUpperBound(0) ; z++)
            {
                sar[z] = Ar[z].ToString().Trim();
            }


            return sar;

        }



    }
}