using System;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace SwissEphemerisWrapperTest
{

    [TestFixture]
    public class SwissEphemerisWrapperTests
    {
        private const string IndiaStandardTimeZoneId = "India Standard Time";
        private readonly int computationFlag = EphemerisModes.Sidereal | EphemerisModes.SwissEphemeris
                                                                            | EphemerisModes.Topocentric;

        [DllImport(@"../../../lib/swedll32.dll")]
        private static extern void swe_close();
        
        [DllImport(@"../../../lib/swedll32.dll")]
        static extern void swe_set_ephe_path([MarshalAs(UnmanagedType.LPTStr)] StringBuilder path);

        [Test]
        public void ShouldSetEphemerisPath()
        {
            var ephemerisTablesPath = new StringBuilder(@"C:\Users\Administrator\Desktop\sweph\ephe");

            swe_set_ephe_path(ephemerisTablesPath);
            swe_close();
        }

        [DllImport(@"../../../lib/swedll32.dll")]
        private static extern StringBuilder swe_get_planet_name(int ipl, StringBuilder spname);

        [Test]
        public void ShouldGetPlanetNames()
        {
            var planetName = new StringBuilder(40); // the 40 may be useful, not sure

            Assert.That(swe_get_planet_name(0, planetName).ToString(),Is.EqualTo("Sun"));
            Assert.That(swe_get_planet_name(1, planetName).ToString(),Is.EqualTo("Moon"));
            Assert.That(swe_get_planet_name(2, planetName).ToString(),Is.EqualTo("Mercury"));
            Assert.That(swe_get_planet_name(3, planetName).ToString(),Is.EqualTo("Venus"));
            Assert.That(swe_get_planet_name(4, planetName).ToString(),Is.EqualTo("Mars"));
            Assert.That(swe_get_planet_name(5, planetName).ToString(),Is.EqualTo("Jupiter"));
            Assert.That(swe_get_planet_name(6, planetName).ToString(),Is.EqualTo("Saturn"));
            Assert.That(swe_get_planet_name(10, planetName).ToString(),Is.EqualTo("mean Node"));
            swe_close();
        }

        [DllImport(@"../../../lib/swedll32.dll")]
        private static extern int swe_utc_to_jd(int year, int month, int day, int hour, int minute, double second, int gregflag, 
            double[] julianDayNumbersInEtAndUt, StringBuilder errorMessage);

        [Test]
        public void ShouldComputeJulianDayNumberFromUtc()
        {
            var julianDayNumbersInEtAndUt = new double[2];
            var errorMessage = new StringBuilder();
            var errorCode = swe_utc_to_jd(2003, 5, 5, 4, 0, 0, 1, julianDayNumbersInEtAndUt, errorMessage);
            if (errorCode == 1)
            {
                Assert.Fail("Error ho gaya ree baaabaaa !!!");
            }
            Console.WriteLine("Julian Day Number in ET: "+ julianDayNumbersInEtAndUt[0]);
            Console.WriteLine("Julian Day Number in UT: "+ julianDayNumbersInEtAndUt[1]);
        }

        [DllImport(@"../../../lib/swedll32.dll")]
        private static extern double swe_julday(int year, int month, int day, double hour, int gregflag);

        [Test]
        public void ShouldComputeJulianDayNumber()
        {
            var timeSpan = new TimeSpan(23, 1, 34);
            double totalHours = timeSpan.TotalHours;
            Console.WriteLine(swe_julday(2001, 1, 1, totalHours, 1));
            Console.WriteLine(swe_julday(2001, 1, 1, 1.1, 1));
            Console.WriteLine(swe_julday(2001, 1, 1, 1.11, 1));
            Console.WriteLine(swe_julday(2001, 1, 1, 25, 1));
            Console.WriteLine(swe_julday(2001, 1, 32, 25, 1));
        }
        
        [DllImport(@"../../../lib/swedll32.dll")]
        private static extern int swe_date_conversion(int year, int month, int day, double hour, int gregflag, IntPtr julianDayPointer);

        [Test]
        public void ShouldReturnNegativeOneOnPassingIncorrectDates()
        {
            IntPtr julianDayPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (double)));
            Assert.That(swe_date_conversion(2001, 1, 1, 23.99999999, 1, julianDayPointer),Is.EqualTo(0),"A Valid date seems to return -1(ERR) flag");
            Console.WriteLine(julianDayPointer.ToString());
            Assert.That(swe_date_conversion(2001, 1, 1, 25, 1, julianDayPointer),Is.EqualTo(-1),"An Invalid date doesn't seem to be failing");
            Console.WriteLine(julianDayPointer.ToString());
            Assert.That(swe_date_conversion(2001, 1, 32, 22, 1, julianDayPointer), Is.EqualTo(-1), "An Invalid date doesn't seem to be failing");
            Console.WriteLine(julianDayPointer.ToString());
            Marshal.FreeHGlobal(julianDayPointer);
            swe_close();
        }

        [Test, Ignore("some bug in swe_date_conversion, seems to add 13 days more")]
        public void Swe_date_conversionShouldReturnTheSameValueAsSwe_julday()
        {
            IntPtr julianDayPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (double)));
            swe_date_conversion(2012, 1, 1, 12, 1, julianDayPointer);
            var expectedJulianDayWithValidation = (double) Marshal.PtrToStructure(julianDayPointer, typeof (double));
            Assert.That(swe_julday(2012, 1, 1, 12, 1), Is.EqualTo(expectedJulianDayWithValidation));
            swe_close();
        }

        //use try finally on all Releasable memory structures in actual code
        [Test]
        public void ShouldCreateIntPointerAllocateMemoryCopySomeDoubleDataAndReadBack()
        {
            var doublePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)));
            try
            {
                var sourceDouble = new[] { 123.123 };
                var destinationDouble = new double[1];

                Console.WriteLine("Source double value is:" + sourceDouble[0]);
                Marshal.Copy(sourceDouble, 0, doublePointer, 1);
                Console.WriteLine("Pointer value is:" + doublePointer);
                Marshal.Copy(doublePointer, destinationDouble, 0, 1);
                Console.WriteLine("Actual Data value copied back from pointer:" + destinationDouble[0]);
            
            }
            finally
            {
                Marshal.FreeHGlobal(doublePointer);    
            }
            
        } 
        
        [Test]
        public void ShouldCreateIntPointerAllocateMemoryMarshalSomeDoubleDataToThePointerAndReadItBack()
        {
            var doublePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (double)));
            var sourceDouble = new[]{123.123};

            Console.WriteLine("Source double value is:" + sourceDouble[0]);
            Marshal.StructureToPtr(sourceDouble[0],doublePointer,true);
            Console.WriteLine("Pointer value is:" + doublePointer);
            var destinationDouble = (double)Marshal.PtrToStructure(doublePointer, typeof(double));
            Console.WriteLine("Actual Data value copied back from pointer:" + destinationDouble);

            Marshal.Release(doublePointer);
        }

        [Test]
        public void ShouldCreateDoubleWithin24FromHoursMinutesAndSeconds()
        {
            var timeSpan = new TimeSpan(23, 1, 34);
            double totalHours = timeSpan.TotalHours;
            Console.WriteLine(totalHours);
        }

        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern int swe_calc_ut(double julianDayNumber, int planetNumber, Int32 computationFlag, double[] longitudeAndLatitude, StringBuilder errorMessage);
        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern int swe_set_sid_mode(Int32 siderealMode, double referenceDate, double initialAyanamsha);
        
        [Test]
        public void ShouldGetLatitudeAndLongitudeOfAPlanet()
        {
            StringBuilder errorMessage = new StringBuilder(256);
            double someJulianDayNumber = swe_julday(2001, 1, 1, 1, 1);

            int computationFlag = EphemerisModes.Sidereal | EphemerisModes.MoshierEphemeris | EphemerisModes.NoNutation;
            var longitudeAndLatitude = new Double[] { 0, 0, 0, 0, 0, 0 };
            Console.WriteLine("Computation Flag b4: " + computationFlag);
            var sweCalcUt = swe_calc_ut(someJulianDayNumber, 0, computationFlag, longitudeAndLatitude, errorMessage);
            Console.WriteLine("Computation Flag after: " + sweCalcUt);
            Console.WriteLine(errorMessage);
            Console.WriteLine("Before setting Lahiri Ayanamsha Longitude: "+ longitudeAndLatitude[0]);
            Console.WriteLine("Before setting Lahiri Ayanamsha Latitude: " + longitudeAndLatitude[1]);
            swe_set_sid_mode(SiderealFlag.Lahiri, 0, 0);
            Console.WriteLine("Computation Flag b4: " + computationFlag);
            var calcUt = swe_calc_ut(someJulianDayNumber, 0, computationFlag, longitudeAndLatitude, errorMessage);
            Console.WriteLine("Computation Flag after: " + calcUt);
            Console.WriteLine("After setting Lahiri Ayanamsha Longitude: "+longitudeAndLatitude[0]);
            Console.WriteLine("After setting Lahiri Ayanamsha Latitude: " + longitudeAndLatitude[1]);
            swe_close();
            var calcUt2 = swe_calc_ut(someJulianDayNumber, 0, computationFlag, longitudeAndLatitude, errorMessage);
            Console.WriteLine("After setting Lahiri Ayanamsha and closing and repoen Longitude: " + longitudeAndLatitude[0]);
            Console.WriteLine("After setting Lahiri Ayanamsha and closing and repoen Latitude: " + longitudeAndLatitude[1]);
        }

        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern int swe_set_topo(double longitude, double latitude, double altitudeAboveSeaLevel);
        [Test]
        public void ShouldGetLatitudeAndLongitudeOfAPlanetAfterSettingTopoLocation()
        {
            var errorMessage = new StringBuilder(256);
            double someJulianDayNumber = swe_julday(2001, 1, 1, 1, 1);

            int computationFlag = EphemerisModes.Sidereal | EphemerisModes.MoshierEphemeris | EphemerisModes.NoNutation | EphemerisModes.Topocentric;
            double[] longitudeAndLatitude = new Double[] { 0, 0, 0, 0, 0, 0 };
            Console.WriteLine("Computation Flag b4: " + computationFlag);
            var sweCalcUt = swe_calc_ut(someJulianDayNumber, 6, EphemerisModes.Sidereal | EphemerisModes.MoshierEphemeris | EphemerisModes.NoNutation, longitudeAndLatitude, errorMessage);
            Console.WriteLine("Computation Flag after: " + sweCalcUt);
            Console.WriteLine(errorMessage);
            Console.WriteLine("Before setting Topo Longitude: " + longitudeAndLatitude[0]);
            Console.WriteLine("Before setting Topo Latitude: " + longitudeAndLatitude[1]);
            swe_set_topo(-90, 0, 0);

            Console.WriteLine("Computation Flag b4: " + computationFlag);
            var calcUt = swe_calc_ut(someJulianDayNumber, 6, computationFlag, longitudeAndLatitude, errorMessage);
            Console.WriteLine("Computation Flag after: " + calcUt);
            Console.WriteLine("After setting Topo Longitude: " + longitudeAndLatitude[0]);
            Console.WriteLine("After setting Topo Latitude: " + longitudeAndLatitude[1]);
            swe_close();
            var calcUt2 = swe_calc_ut(someJulianDayNumber, 6, computationFlag, longitudeAndLatitude, errorMessage);
            Console.WriteLine("After setting Topo and closing and repoen Longitude: " + longitudeAndLatitude[0]);
            Console.WriteLine("After setting Topo and closing and repoen Latitude: " + longitudeAndLatitude[1]);
        }

        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern StringBuilder swe_version(StringBuilder versionNUmber);

        [Test]
        public void ShouldGetSwissDllVersionNumber()
        {
            var versionNUmber = new StringBuilder(512);
            Console.WriteLine(swe_version(versionNUmber));
        }

        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern double swe_sidtime(double julianDayNumber);
        
        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern StringBuilder swe_house_name(int houseMethodAsciiCode);
        
        [Test]
        public void ShouldFindWhatTheFuckTheseHouseFuckingNamesAre()
        {
            Console.WriteLine("P: " + swe_house_name('P'));
            Console.WriteLine("K: " + swe_house_name('K'));
            Console.WriteLine("O: " + swe_house_name('O'));
            Console.WriteLine("R: " + swe_house_name('R'));
            Console.WriteLine("C: " + swe_house_name('C'));
            Console.WriteLine("A: " + swe_house_name('A'));
            Console.WriteLine("E: " + swe_house_name('E'));
            Console.WriteLine("V: " + swe_house_name('V'));
            Console.WriteLine("X: " + swe_house_name('X'));
            Console.WriteLine("H: " + swe_house_name('H'));
            Console.WriteLine("T: " + swe_house_name('T'));
            Console.WriteLine("B: " + swe_house_name('B'));
            Console.WriteLine("G: " + swe_house_name('G'));
        }

        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern double swe_get_ayanamsa_ut(double tjd_ut);

        [Test]
        public void ShouldGetAyanamshaForYear285Ad()
        {
            swe_set_sid_mode(SiderealFlag.Lahiri, 0, 0);
            double someJulianDayNumber = swe_julday(2000, 1, 1, 0, 1);
            var sweGetAyanamsaUt = swe_get_ayanamsa_ut(someJulianDayNumber);
            Console.WriteLine(sweGetAyanamsaUt);
        }

        [DllImport(@"../../../lib/swedll32.dll", CharSet = CharSet.Ansi)]
        private static extern int swe_houses_ex(double julianDayNumber, int sideralAndRadiansFlag, double latitude, double longitude,
            int houseMethodAsciiCode, double[] cusps, double[] ascendantAndSomeOtherCrap);

        [Test]
        public void ShouldFindTheAscendant()
        {
            double someJulianDayNumber = swe_julday(2001, 1, 1, 1, 1);
            var cusps = new double[13];
            var ascendantAndOthers = new double[10];

            swe_set_sid_mode(SiderealFlag.Lahiri, 0, 0);
            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'E', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);

            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'P', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);


            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'K', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);


            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'O', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);


            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'R', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);


            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'C', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);

            
            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'V', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);

            
            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'X', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);

            
            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'H', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);

            
            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'T', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);

            
            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'B', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);


//          Watch out for the House Flag G if you ever use it, kuch to bakar kar raha he ye mkc
//          swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'G', cusps, ascendantAndOthers);
          Console.WriteLine(ascendantAndOthers[0]);
            
            swe_houses_ex(someJulianDayNumber, EphemerisModes.Sidereal, 20, 78, 'A', cusps, ascendantAndOthers);
            Console.WriteLine(ascendantAndOthers[0]);
//            Console.WriteLine(cusps[1]);
//            Console.WriteLine(cusps[2]);
        }

        [Test]
        public void ShouldGetAllPlanetValues()
        {
            var errorMessage = new StringBuilder(1000);
            var longitudeAndLatitude = new Double[6];
            var cusps = new Double[13];
            var ascentantAndMore = new Double[10];
            var julianDayNumbersInEtAndUt = new double[2];
            TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(IndiaStandardTimeZoneId);
            var ephemerisTablesPath = new StringBuilder(@"../../../EphemerisFiles/");

            swe_set_ephe_path(ephemerisTablesPath);
            var birthTimeInIndianZone = new DateTime(1989, 8, 21, 10, 29, 0);
            DateTime birthTimeInUtc = TimeZoneInfo.ConvertTimeToUtc(birthTimeInIndianZone, indiaTimeZone);

            int errorCOde = swe_utc_to_jd(birthTimeInUtc.Year, birthTimeInUtc.Month, birthTimeInUtc.Day,
                                          birthTimeInUtc.Hour, birthTimeInUtc.Minute, birthTimeInUtc.Second, 1,
                                          julianDayNumbersInEtAndUt, errorMessage);
            if (errorCOde != 0)
            {
                Assert.Fail("Error ho gaya julian date conversion par !!");
            }
            Console.WriteLine("Julian Day Number from UTC: " + julianDayNumbersInEtAndUt[1]);
            Console.WriteLine(swe_sidtime(julianDayNumbersInEtAndUt[1]));
            var computationFlag = EphemerisModes.Sidereal | EphemerisModes.SwissEphemeris;
//                                                                            | EphemerisModes.Topocentric;
//            swe_set_topo(12.9667, 77.5667, 0);
//            swe_set_topo(12.9667, 77.5667, 0);
            swe_set_sid_mode(SiderealFlag.Lahiri, 0, 0);
            foreach (var planet in Planets.AllPlanets)
            {
                int sweCalcUt = swe_calc_ut(julianDayNumbersInEtAndUt[1], planet, computationFlag, longitudeAndLatitude, errorMessage);
                Console.WriteLine("Planet Number " + planet + ": " + GetDegreesMinutesSeconds(longitudeAndLatitude[0]));
            }
            swe_houses_ex(julianDayNumbersInEtAndUt[1], EphemerisModes.Sidereal, 12.9667, 77.5667, 'A', cusps, ascentantAndMore);
            Console.WriteLine("Ascendant: " + GetDegreesMinutesSeconds(ascentantAndMore[0]));
        }

        [Test]
        public void ShouldConvertIndianTimeZoneToUtc()
        {
            TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(IndiaStandardTimeZoneId);
            var birthTimeInIndianZone = new DateTime(1989, 8, 21, 10, 29, 0);
            DateTime birthTimeInUtc = TimeZoneInfo.ConvertTimeToUtc(birthTimeInIndianZone, indiaTimeZone);
            Console.WriteLine(birthTimeInIndianZone);
            Console.WriteLine(birthTimeInUtc);
        }

        public double ConvertDegreeAngleToDouble(double degrees, double minutes, double seconds)
        {
            return degrees + (minutes / 60) + (seconds / 3600);
        }

        public string GetDegreesMinutesSeconds(double decimalDegrees)
        {
            double degrees = Math.Floor(decimalDegrees);
            double minutes = (decimalDegrees - Math.Floor(decimalDegrees)) * 60.0;
            double seconds = (minutes - Math.Floor(minutes)) * 60.0;
            return degrees + ":" + Math.Floor(minutes) + ":" + Math.Round(seconds);
        }

    }
}
