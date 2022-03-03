﻿namespace BlueBasics {

    public static class clsImageMatrix {

        #region Properties

        public static double[,] GaussianBlur3X3 => new double[,]
                { { 1, 2, 1},
                  { 2, 4, 2},
                  { 1, 2, 1}};

        public static double[,] GaussianBlur5X5 => new double[,]
                { { 2, 04, 05, 04, 2 },
                  { 4, 09, 12, 09, 4 },
                  { 5, 12, 15, 12, 5 },
                  { 4, 09, 12, 09, 4 },
                  { 2, 04, 05, 04, 2 }};

        public static double[,] Mean3X3 => new double[,]
                               { { 1, 1, 1},
                  { 1, 1, 1},
                  { 1, 1, 1}};

        public static double[,] Mean5X5 => new double[,]
               { { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1 }};

        public static double[,] Mean7X7 => new double[,]
                { { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1 }};

        public static double[,] Mean9X9 => new double[,]
                { { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                  { 1, 1, 1, 1, 1, 1, 1, 1, 1 }};

        public static double[,] MotionBlur5X5 => new double[,]
                { { 1, 0, 0, 0, 1 },
                  { 0, 1, 0, 1, 0 },
                  { 0, 0, 1, 0, 0 },
                  { 0, 1, 0, 1, 0 },
                  { 1, 0, 0, 0, 1 }};

        public static double[,] MotionBlur5X5At135Degrees => new double[,]
                { { 1, 0, 0, 0, 0 },
                  { 0, 1, 0, 0, 0 },
                  { 0, 0, 1, 0, 0 },
                  { 0, 0, 0, 1, 0 },
                  { 0, 0, 0, 0, 1 }};

        public static double[,] MotionBlur5X5At45Degrees => new double[,]
                        { { 0, 0, 0, 0, 1 },
                  { 0, 0, 0, 1, 0 },
                  { 0, 0, 1, 0, 0 },
                  { 0, 1, 0, 0, 0 },
                  { 1, 0, 0, 0, 0 }};

        public static double[,] MotionBlur7X7 => new double[,]
                { { 1, 0, 0, 0, 0, 0, 1 },
                  { 0, 1, 0, 0, 0, 1, 0 },
                  { 0, 0, 1, 0, 1, 0, 0 },
                  { 0, 0, 0, 1, 0, 0, 0 },
                  { 0, 0, 1, 0, 1, 0, 0 },
                  { 0, 1, 0, 0, 0, 1, 0 },
                  { 1, 0, 0, 0, 0, 0, 1 }};

        public static double[,] MotionBlur7X7At135Degrees => new double[,]
                { { 1, 0, 0, 0, 0, 0, 0 },
                  { 0, 1, 0, 0, 0, 0, 0 },
                  { 0, 0, 1, 0, 0, 0, 0 },
                  { 0, 0, 0, 1, 0, 0, 0 },
                  { 0, 0, 0, 0, 1, 0, 0 },
                  { 0, 0, 0, 0, 0, 1, 0 },
                  { 0, 0, 0, 0, 0, 0, 1 }};

        public static double[,] MotionBlur7X7At45Degrees => new double[,]
                        { { 0, 0, 0, 0, 0, 0, 1 },
                  { 0, 0, 0, 0, 0, 1, 0 },
                  { 0, 0, 0, 0, 1, 0, 0 },
                  { 0, 0, 0, 1, 0, 0, 0 },
                  { 0, 0, 1, 0, 0, 0, 0 },
                  { 0, 1, 0, 0, 0, 0, 0 },
                  { 1, 0, 0, 0, 0, 0, 0 }};

        public static double[,] MotionBlur9X9 => new double[,]
                { { 1, 0, 0, 0, 0, 0, 0, 0, 1},
                  { 0, 1, 0, 0, 0, 0, 0, 1, 0},
                  { 0, 0, 1, 0, 0, 0, 1, 0, 0},
                  { 0, 0, 0, 1, 0, 1, 0, 0, 0},
                  { 0, 0, 0, 0, 1, 0, 0, 0, 0},
                  { 0, 0, 0, 1, 0, 1, 0, 0, 0},
                  { 0, 0, 1, 0, 0, 0, 1, 0, 0},
                  { 0, 1, 0, 0, 0, 0, 0, 1, 0},
                  { 1, 0, 0, 0, 0, 0, 0, 0, 1}};

        public static double[,] MotionBlur9X9At135Degrees => new double[,]
                { { 1, 0, 0, 0, 0, 0, 0, 0, 0},
                  { 0, 1, 0, 0, 0, 0, 0, 0, 0},
                  { 0, 0, 1, 0, 0, 0, 0, 0, 0},
                  { 0, 0, 0, 1, 0, 0, 0, 0, 0},
                  { 0, 0, 0, 0, 1, 0, 0, 0, 0},
                  { 0, 0, 0, 0, 0, 1, 0, 0, 0},
                  { 0, 0, 0, 0, 0, 0, 1, 0, 0},
                  { 0, 0, 0, 0, 0, 0, 0, 1, 0},
                  { 0, 0, 0, 0, 0, 0, 0, 0, 1}};

        public static double[,] MotionBlur9X9At45Degrees => new double[,]
                        { { 0, 0, 0, 0, 0, 0, 0, 0, 1},
                  { 0, 0, 0, 0, 0, 0, 0, 1, 0},
                  { 0, 0, 0, 0, 0, 0, 1, 0, 0},
                  { 0, 0, 0, 0, 0, 1, 0, 0, 0},
                  { 0, 0, 0, 0, 1, 0, 0, 0, 0},
                  { 0, 0, 0, 1, 0, 0, 0, 0, 0},
                  { 0, 0, 1, 0, 0, 0, 0, 0, 0},
                  { 0, 1, 0, 0, 0, 0, 0, 0, 0},
                  { 1, 0, 0, 0, 0, 0, 0, 0, 0}};

        #endregion
    }
}