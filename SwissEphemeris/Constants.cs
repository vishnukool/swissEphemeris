namespace SwissEphemerisWrapperTest
{
    public static class Planets
    {
        public static int SUN = 0;
        public static int MOON = 1;
        public static int MERCURY = 2;
        public static int VENUS = 3;
        public static int MARS = 4;
        public static int JUPITER = 5;
        public static int SATURN = 6;
        public static int URANUS = 7;
        public static int NEPTUNE = 8;
        public static int PLUTO = 9;
        public static int MEAN_NODE = 10;

        public static int[] AllPlanets = new[]{0,1,2,3,4,5,6,10};
    }

    public static class EphemerisModes
    {
        public static int NoNutation = 64;
        public static int MoshierEphemeris = 4;
        public static int SwissEphemeris = 2;
        public static int Sidereal = 64*1024;
        public static int Topocentric = 32*1024;
    }

    public enum Plan
    {
        Sun = 0,
        Moon = 1
    }

    public static class SiderealFlag
    {
        public static int Lahiri = 1;
    }
}