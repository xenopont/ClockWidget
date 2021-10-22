using System.Windows.Media;

namespace ClockWidget
{
    internal class ColorEngine
    {
        public enum Pattern
        {
            Random,
        }

        public Pattern ActivePattern { get; set; } = Pattern.Random;

        public Color NextColor { get => GenerateNextColor(); }

        private Color GenerateNextColor()
        {
            return ActivePattern switch
            {
                Pattern.Random => NextRandom(),
                _ => NextRandom(),
            };
        }

        private static readonly System.Random random = new();

        private static Color NextRandom()
        {
            double hue = random.NextDouble() * 360;
            double saturation = random.NextDouble();
            double brightness = random.NextDouble();

            return FromHSV(hue, saturation, brightness);
        }

        private static Color FromHSV(double hue, double saturation, double brightness)
        {
            static byte ColorComponent(double component, double m)
            {
                return (byte)((component + m) * 255);
            }

            double C = brightness * saturation;
            double X = C * (1 - System.Math.Abs((hue % 2) - 1));
            double m = brightness - C;
            double[] rgb = hue switch
            {
                (>= 000) and (< 060) => new double[] { C, X, 0 },
                (>= 060) and (< 120) => new double[] { X, C, 0 },
                (>= 120) and (< 180) => new double[] { 0, C, X },
                (>= 180) and (< 240) => new double[] { 0, X, C },
                (>= 240) and (< 300) => new double[] { X, 0, C },
                // (>= 300) and (< 360)
                _ => new double[] { C, 0, X },
            };

            return Color.FromArgb(255,
                ColorComponent(rgb[0], m),
                ColorComponent(rgb[1], m),
                ColorComponent(rgb[2], m)
                );
        }
    }
}
