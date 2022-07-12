using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PAENN2
{
    public static class Helper
    {
        /// <summary>
        /// Extension method for doubles, checks whether two floating-point values are close enough
        /// to be considered equal, given some tolerance. 
        /// </summary>
        /// <param name="value1">The first floating-point value.</param>
        /// <param name="value2">The second floating-point value.</param>
        /// <param name="tol">The maximum acceptable distance between the values.</param>
        /// <returns>Whether the values are close enough or not.</returns>
        public static bool CloseEnough(this double value1, double value2, double tol = 1e-5)
        {
            return Math.Abs(value1 - value2) < tol;
        }



        /// <summary>
        /// Extension method for strings, parses a given string into a floating-point value.
        /// </summary>
        /// <param name="str">The string to be parsed.</param>
        /// <param name="CanBeNegative">Whether to accept negative values or to throw an exception.</param>
        /// <param name="CanBeZero">Whether to accept zero as a valid value or to throw an exception.</param>
        /// <returns>The parsed double.</returns>
        public static double StringToDouble(this string str, bool CanBeNegative = true, bool CanBeZero = true)
        {
            double result = 0;
            if (str.Trim() != "")
            {
                result = double.Parse(str);
            }

            return !CanBeNegative && result < 0
                ? throw new ArgumentOutOfRangeException("Negative values not allowed for this variable.")
                : !CanBeZero && result.CloseEnough(0, 1e-8)
                ? throw new ArgumentException("Zero is not a valid value for this variable.")
                : result;
        }



        /// <summary>
        /// Extension method for points, calculates the angle between the line connecting the point and 
        /// another given point, and the positive X-axis.
        /// </summary>
        /// <param name="P1">The line's starting point.</param>
        /// <param name="P2">The line's ending point.</param>
        /// <returns>The line's slope angle, in degrees.</returns>
        public static double AngleBetween(this Point P1, Point P2)
        {
            if (CloseEnough(P1.X, P2.X))
            {
                return P2.Y >= P1.Y ? 90 : 270;
            }

            double atan = Math.Atan((P2.Y - P1.Y) / (P2.X - P1.X)) * 180 / Math.PI;

            return P2.X > P1.X ? P2.Y >= P1.Y ? atan : 360 + atan : 180 + atan;
        }



        /// <summary>
        /// Extension method for points, rotates the point around a given center point for a given angle
        /// (positive counter-clockwise).
        /// </summary>
        /// <param name="point">The point to be rotated.</param>
        /// <param name="center">The rotation's center-point.</param>
        /// <param name="angle">The rotation angle</param>
        /// <returns>The rotated point.</returns>
        public static Point Rotate(this Point point, Point center, double angle)
        {
            angle *= Math.PI / 180;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double dx = point.X - center.X;
            double dy = point.Y - center.Y;

            return new Point((cos * dx) - (sin * dy) + center.X, (sin * dx) + (cos * dy) + center.Y);
        }



        /// <summary>
        /// Extension method for Textblocks, calculates the screen space needed for a given string to be displayed in the Textbox.
        /// Original code by RandomEngy at https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters
        /// </summary>
        /// <param name="textblock">The textblock.</param>
        /// <param name="candidate">The string whose size is to be measured.</param>
        /// <returns></returns>
        public static Size MeasureString(this TextBlock textblock, string candidate)
        {
            FormattedText formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textblock.FontFamily, textblock.FontStyle, textblock.FontWeight, textblock.FontStretch),
                textblock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }



        /// <summary>
        /// Finds the intersection between two lines (if it exists), each defined by two points.
        /// </summary>
        /// <param name="P1">The first line's first point.</param>
        /// <param name="P2">The first line's second point.</param>
        /// <param name="Q1">The second line's first point.</param>
        /// <param name="Q2">The second line's second point.</param>
        /// <returns>The intersection point if the lines cross, null if they are parallel.</returns>
        public static Point? FindIntersection(Point P1, Point P2, Point Q1, Point Q2)
        {
            double a1;
            double b1;
            double a2;
            double b2;

            if (Q2.X.CloseEnough(Q1.X))
            {
                if (P2.X.CloseEnough(P1.X))
                {
                    return null;
                }

                a1 = (P2.Y - P1.Y) / (P2.X - P1.X);
                b1 = P1.Y - (a1 * P1.X);

                return (Q2.Y.CloseEnough(Q1.Y) && ((a1 * Q2.X) + b1 != Q2.Y))? null : (Point?)new Point(Q2.X, (a1 * Q2.X) + b1);
            }

            if (P2.X.CloseEnough(P1.X))
            {
                a2 = (Q2.Y - Q2.Y) / (Q2.X - Q1.X);
                b2 = Q1.Y - (a2 * Q1.X);

                return (P2.Y.CloseEnough(P1.Y) && ((a2 * P2.X) + b2 != Q2.Y))? null : (Point?)new Point(P2.X, (a2 * P2.X) + b2);
            }


            a1 = (P2.Y - P1.Y) / (P2.X - P1.X);
            b1 = P1.Y - (a1 * P1.X);

            a2 = (Q2.Y - Q2.Y) / (Q2.X - Q1.X);
            b2 = Q1.Y - (a2 * Q1.X);

            if (a1.CloseEnough(a2))
            {
                return null;
            }

            double x = (b1 - b2) / (a2 - a1);
            double y = ((b1 * a2) - (b2 * a1)) / (a2 - a1);

            return new Point(x, y);
        }



        /// <summary>
        /// Checks whether two line segments (defined by their endpoints) intersect or not.
        /// </summary>
        /// <param name="P1">The first segment's starting point.</param>
        /// <param name="P2">The first segment's ending point.</param>
        /// <param name="Q1">The second segment's starting point.</param>
        /// <param name="Q2">The second segment's ending point.</param>
        /// <returns>True if the segments intersect, false otherwise.</returns>
        public static bool DoTheyIntersect(Point P1, Point P2, Point Q1, Point Q2)
        {
            Point? P = FindIntersection(P1, P2, Q1, Q2);

            if (!P.HasValue)
            {
                return false;
            }

            return P.Value.X < Math.Min(P1.X, P2.X) || P.Value.X > Math.Max(P1.X, P2.X) ||
                P.Value.Y < Math.Min(P1.Y, P2.Y) || P.Value.Y > Math.Max(P1.Y, P2.Y)
                ? false
                : P.Value.X >= Math.Min(Q1.X, Q2.X) && P.Value.X <= Math.Max(Q1.X, Q2.X) &&
                P.Value.Y >= Math.Min(Q1.Y, Q2.Y) && P.Value.Y <= Math.Max(Q1.Y, Q2.Y);
        }


        public static void AssignToChildren(this TransformGroup tg, double S_ScaleX, double S_ScaleY, double S_CenterX,
                                            double S_CenterY, double R_Angle, double R_CenterX, double R_CenterY)
        {
            foreach (Transform t in tg.Children)
            {
                if (t.GetType() == typeof(ScaleTransform))
                {
                    ScaleTransform ch = (ScaleTransform)t;
                    ch.ScaleX = S_ScaleX;
                    ch.ScaleY = S_ScaleY;
                    ch.CenterX = S_CenterX;
                    ch.CenterY = S_CenterY;
                }
                else
                {
                    RotateTransform ch = (RotateTransform)t;
                    ch.Angle = R_Angle;
                    ch.CenterX = R_CenterX;
                    ch.CenterY = R_CenterY;
                }
            }
        }


        public static double LinInterp(double X0, double Y0, double X1, double Y1, double X)
        {
            if (X1.CloseEnough(X0))
            {
                throw new ArgumentException("Cannot interpolate between two points on a vertical line.");
            }

            double m = (Y1 - Y0) / (X1 - X0);
            double Y = Y0 + (m * (X - X0));
            return Y;
        }


        public static void AddToCanvas(this Canvas cnv, params UIElement[] controls)
        {
            foreach (UIElement elem in controls)
            {
                if (!cnv.Children.Contains(elem))
                {
                    _ = cnv.Children.Add(elem);
                }
            }
        }

        public static void RemoveFromCanvas(this Canvas cnv, params UIElement[] controls)
        {
            foreach (UIElement elem in controls)
            {
                if (cnv.Children.Contains(elem))
                {
                    cnv.Children.Remove(elem);
                }
            }
        }
    }

    /// <summary>
    /// Static class that holds any imported native Win32 methods needed.
    /// </summary>
    public static class Native
    {
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public static void SetCursor(int x, int y)
        {
            // Left boundary
            int xL = (int)Application.Current.MainWindow.Left;
            // Top boundary
            int yT = (int)Application.Current.MainWindow.Top;

            _ = SetCursorPos(x + xL, y + yT);
        }
    }

    /// <summary>
    /// Converts between Visible/Collapsed and boolean values.
    /// </summary>
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Boolean && (bool)value ? Visibility.Visible : (object)Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible ? true : (object)false;
        }

    }


    /// <summary>
    /// Converts between Visible/Collapsed and boolean values.
    /// </summary>
    public class NotBoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Boolean && (bool)value ? Visibility.Collapsed : (object)Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible ? false : (object)true;
        }

    }


    /// <summary>
    /// Returns the inverse boolean (aka the "No" operator).
    /// </summary>
    public class InverseBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Boolean && (bool)value ? false : (object)true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Boolean && (bool)value ? false : (object)true;
        }

    }

    /// <summary>
    /// Converts the Radiobutton selection to an integer value
    /// Originally by Jonathan1 on https://stackoverflow.com/questions/1317891/simple-wpf-radiobutton-binding
    /// </summary>
    public class RadioBoolToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int integer = (int)value;
            if (integer == int.Parse(parameter.ToString()))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }

}
