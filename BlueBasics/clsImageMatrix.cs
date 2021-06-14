﻿namespace BlueBasics {

    public static class clsImageMatrix {

        public static double[,] Mean3x3 => new double[,]
               { { 1, 1, 1, },
                  { 1, 1, 1, },
                  { 1, 1, 1, }, };

        public static double[,] Mean5x5 => new double[,]
               { { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 }, };

        public static double[,] Mean7x7 => new double[,]
                { { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 }, };

        public static double[,] Mean9x9 => new double[,]
                { { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, };

        public static double[,] GaussianBlur3x3 => new double[,]
                { { 1, 2, 1, },
                  { 2, 4, 2, },
                  { 1, 2, 1, }, };

        public static double[,] GaussianBlur5x5 => new double[,]
                { { 2, 04, 05, 04, 2 },
                  { 4, 09, 12, 09, 4 },
                  { 5, 12, 15, 12, 5 },
                  { 4, 09, 12, 09, 4 },
                  { 2, 04, 05, 04, 2 }, };

        public static double[,] MotionBlur5x5 => new double[,]
                { { 1, 0, 0, 0, 1 },
                  { 0, 1, 0, 1, 0 },
                  { 0, 0, 1, 0, 0 },
                  { 0, 1, 0, 1, 0 },
                  { 1, 0, 0, 0, 1 }, };

        public static double[,] MotionBlur5x5At45Degrees => new double[,]
                { { 0, 0, 0, 0, 1 },
                  { 0, 0, 0, 1, 0 },
                  { 0, 0, 1, 0, 0 },
                  { 0, 1, 0, 0, 0 },
                  { 1, 0, 0, 0, 0 }, };

        public static double[,] MotionBlur5x5At135Degrees => new double[,]
                { { 1, 0, 0, 0, 0 },
                  { 0, 1, 0, 0, 0 },
                  { 0, 0, 1, 0, 0 },
                  { 0, 0, 0, 1, 0 },
                  { 0, 0, 0, 0, 1 }, };

        public static double[,] MotionBlur7x7 => new double[,]
                { { 1, 0, 0, 0, 0, 0, 1 },
                  { 0, 1, 0, 0, 0, 1, 0 },
                  { 0, 0, 1, 0, 1, 0, 0 },
                  { 0, 0, 0, 1, 0, 0, 0 },
                  { 0, 0, 1, 0, 1, 0, 0 },
                  { 0, 1, 0, 0, 0, 1, 0 },
                  { 1, 0, 0, 0, 0, 0, 1 }, };

        public static double[,] MotionBlur7x7At45Degrees => new double[,]
                { { 0, 0, 0, 0, 0, 0, 1 },
                  { 0, 0, 0, 0, 0, 1, 0 },
                  { 0, 0, 0, 0, 1, 0, 0 },
                  { 0, 0, 0, 1, 0, 0, 0 },
                  { 0, 0, 1, 0, 0, 0, 0 },
                  { 0, 1, 0, 0, 0, 0, 0 },
                  { 1, 0, 0, 0, 0, 0, 0 }, };

        public static double[,] MotionBlur7x7At135Degrees => new double[,]
                { { 1, 0, 0, 0, 0, 0, 0 },
                  { 0, 1, 0, 0, 0, 0, 0 },
                  { 0, 0, 1, 0, 0, 0, 0 },
                  { 0, 0, 0, 1, 0, 0, 0 },
                  { 0, 0, 0, 0, 1, 0, 0 },
                  { 0, 0, 0, 0, 0, 1, 0 },
                  { 0, 0, 0, 0, 0, 0, 1 }, };

        public static double[,] MotionBlur9x9 => new double[,]
                { { 1, 0, 0, 0, 0, 0, 0, 0, 1, },
                  { 0, 1, 0, 0, 0, 0, 0, 1, 0, },
                  { 0, 0, 1, 0, 0, 0, 1, 0, 0, },
                  { 0, 0, 0, 1, 0, 1, 0, 0, 0, },
                  { 0, 0, 0, 0, 1, 0, 0, 0, 0, },
                  { 0, 0, 0, 1, 0, 1, 0, 0, 0, },
                  { 0, 0, 1, 0, 0, 0, 1, 0, 0, },
                  { 0, 1, 0, 0, 0, 0, 0, 1, 0, },
                  { 1, 0, 0, 0, 0, 0, 0, 0, 1, }, };

        public static double[,] MotionBlur9x9At45Degrees => new double[,]
                { { 0, 0, 0, 0, 0, 0, 0, 0, 1, },
                  { 0, 0, 0, 0, 0, 0, 0, 1, 0, },
                  { 0, 0, 0, 0, 0, 0, 1, 0, 0, },
                  { 0, 0, 0, 0, 0, 1, 0, 0, 0, },
                  { 0, 0, 0, 0, 1, 0, 0, 0, 0, },
                  { 0, 0, 0, 1, 0, 0, 0, 0, 0, },
                  { 0, 0, 1, 0, 0, 0, 0, 0, 0, },
                  { 0, 1, 0, 0, 0, 0, 0, 0, 0, },
                  { 1, 0, 0, 0, 0, 0, 0, 0, 0, }, };

        public static double[,] MotionBlur9x9At135Degrees => new double[,]
                { { 1, 0, 0, 0, 0, 0, 0, 0, 0, },
                  { 0, 1, 0, 0, 0, 0, 0, 0, 0, },
                  { 0, 0, 1, 0, 0, 0, 0, 0, 0, },
                  { 0, 0, 0, 1, 0, 0, 0, 0, 0, },
                  { 0, 0, 0, 0, 1, 0, 0, 0, 0, },
                  { 0, 0, 0, 0, 0, 1, 0, 0, 0, },
                  { 0, 0, 0, 0, 0, 0, 1, 0, 0, },
                  { 0, 0, 0, 0, 0, 0, 0, 1, 0, },
                  { 0, 0, 0, 0, 0, 0, 0, 0, 1, }, };
    }
}