using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TestSwissEphemeris
{
    class Program
    {
        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern int swe_calc_ut(double julianDayNumber, int planetNumber, int computationFlag, double[] latitudeAndLongitude, StringBuilder errorMessage);


        static void Main(string[] args)
        {
            StringBuilder errorMessage = new StringBuilder(256);
            StringBuilder errorMessag2e = new StringBuilder();
            var latitudeAndLongitude = Marshal.AllocHGlobal(6 * sizeof(double));

            var computationFlag = 0;
            var tret = new Double[6] { 0, 0, 0, 0, 0, 0 };
            var sweCalcUt = swe_calc_ut(2443436.659722222, 0, 0, tret, errorMessage);

            Marshal.Release(latitudeAndLongitude);
            Console.ReadKey();
        }
    }
}
