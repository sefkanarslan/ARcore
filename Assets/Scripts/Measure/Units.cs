using System.Globalization;

namespace ArSpacePlanner.Measure
{
    /// <summary>
    /// Unit conversion and human-readable formatting. All internal values are in
    /// metres (lengths) and square metres (areas); this converts to the unit the
    /// user selected only at display time.
    /// </summary>
    public static class Units
    {
        private const float MetersToCentimeters = 100f;
        private const float MetersToInches = 39.3700787f;
        private const float MetersToFeet = 3.2808399f;

        public static MeasureUnit Next(MeasureUnit unit)
        {
            switch (unit)
            {
                case MeasureUnit.Meters: return MeasureUnit.Centimeters;
                case MeasureUnit.Centimeters: return MeasureUnit.Feet;
                case MeasureUnit.Feet: return MeasureUnit.Inches;
                default: return MeasureUnit.Meters;
            }
        }

        /// <summary>Formats a length given in metres, e.g. "1.24 m" or "124 cm".</summary>
        public static string FormatLength(float meters, MeasureUnit unit)
        {
            switch (unit)
            {
                case MeasureUnit.Centimeters:
                    return Round(meters * MetersToCentimeters, 0) + " cm";
                case MeasureUnit.Inches:
                    return Round(meters * MetersToInches, 1) + " in";
                case MeasureUnit.Feet:
                    return Round(meters * MetersToFeet, 2) + " ft";
                default:
                    return Round(meters, 2) + " m";
            }
        }

        /// <summary>Formats an area given in square metres.</summary>
        public static string FormatArea(float squareMeters, MeasureUnit unit)
        {
            switch (unit)
            {
                case MeasureUnit.Centimeters:
                    return Round(squareMeters * MetersToCentimeters * MetersToCentimeters, 0) + " cm\u00B2";
                case MeasureUnit.Inches:
                    return Round(squareMeters * MetersToInches * MetersToInches, 0) + " in\u00B2";
                case MeasureUnit.Feet:
                    return Round(squareMeters * MetersToFeet * MetersToFeet, 2) + " ft\u00B2";
                default:
                    return Round(squareMeters, 2) + " m\u00B2";
            }
        }

        private static string Round(float value, int digits)
        {
            double rounded = System.Math.Round(value, digits);
            return rounded.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
