using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace PAENN2
{
    public static class Units
    {
        public static string Length = "cm";
        public static string Force = "kN";
        public static string Moment = "kN.m";
        public static string Load = "kN/m";

        public static string Angle = "°";
        public static string Spring = "kN/m";

        public static string Displacement = "cm";
        public static string Rotation = "rad";

        public static Dictionary<string, string> Formats = new Dictionary<string, string> {
                { "Length", "F2" }, { "Force", "F2" }, { "Moment", "F2" },
                { "Load", "F2" }, { "Angle", "F1" },  { "Spring", "F1" },
                { "Displacement", "F3" }, { "Rotation", "F3" },
        };

        public static double ConvertLength(double value, string unit, bool ConvertFromDefault)
        {
            if (ConvertFromDefault)
            {
                switch (unit)
                {
                    case "cm": return value;
                    case "m": return value / 100;
                    case "mm": return value * 10;
                    case "in": return value / 2.54;
                    case "ft": return value / 30.48;
                    default: throw new ArgumentException("Invalid length unit passed as argument.");
                }
            }
            else
            {
                switch (unit)
                {
                    case "cm": return value;
                    case "m": return value * 100;
                    case "mm": return value / 10;
                    case "in": return value * 2.54;
                    case "ft": return value * 30.48;
                    default: throw new ArgumentException("Invalid length unit passed as argument.");
                }
            }
        }

        public static double ConvertForce(double value, string unit, bool ConvertFromDefault)
        {
            if (ConvertFromDefault)
            {
                switch (unit)
                {
                    case "kN": return value;
                    case "N": return value * 1000;
                    case "kgf": return value * 101.972;
                    case "tf": return value * 0.101972;
                    case "ft": return value * 224.809;
                    case "kip": return value * 0.224809;
                    default: throw new ArgumentException("Invalid force unit passed as argument.");
                }
            }
            else
            {
                switch (unit)
                {
                    case "kN": return value;
                    case "N": return value / 1000;
                    case "kgf": return value / 101.972;
                    case "tf": return value / 0.101972;
                    case "ft": return value / 224.809;
                    case "kip": return value / 0.224809;
                    default: throw new ArgumentException("Invalid force unit passed as argument.");
                }
            }
        }

        public static double ConvertAngle(double value, string unit, bool ConvertFromDefault)
        {
            if (ConvertFromDefault)
            {
                switch (unit)
                {
                    case "°": return value;
                    case "rad": return value * Math.PI / 180;
                    default: throw new ArgumentException("Invalid angle unit passed as argument.");
                }
            }
            else
            {
                switch (unit)
                {
                    case "°": return value;
                    case "rad": return value * Math.PI / 180;
                    default: throw new ArgumentException("Invalid angle unit passed as argument.");
                }
            }
        }

        public static double ConvertMoment(double value, string unit, bool ConvertFromDefault)
        {
            string[] units = unit.Split('.');

            double factor = ConvertForce(1, units[0], ConvertFromDefault);
            factor *= ConvertLength(1, units[1], ConvertFromDefault);

            return factor * value;
        }

        public static double ConvertLoad(double value, string unit, bool ConvertFromDefault)
        {
            string[] units = unit.Split('/');

            double factor = ConvertForce(1, units[0], ConvertFromDefault);
            factor /= ConvertLength(1, units[1], ConvertFromDefault);

            return factor * value;
        }
    }
}
