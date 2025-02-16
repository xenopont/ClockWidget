using System.Windows.Media;

namespace ClockWidget
{
    internal class ColorEngine
    {
        private enum Pattern
        {
            Random,
        }

        private static Pattern ActivePattern => Pattern.Random;

        public static Color NextColor => GenerateNextColor();

        private static Color GenerateNextColor()
        {
            return ActivePattern switch
            {
                Pattern.Random => NextRandom(),
                _ => NextRandom(),
            };
        }

        private static readonly Random RandomNumberGenerator = new();

        private static Color NextRandom()
        {
            const double brightnessMax = 0.65;
            const double brightnessMin = 0.30;
            const double errorDouble = 0.0000001;
            const double errorColor = 0.004;

            var r = RandomNumberGenerator.NextDouble();
            var g = RandomNumberGenerator.NextDouble();
            var b = RandomNumberGenerator.NextDouble();

            if (r < errorColor && g < errorColor && b < errorColor)
            {
                var c = (byte) (Math.Sqrt(brightnessMin * brightnessMin / 3.0) * 255);
                return Color.FromArgb(255, c, c, c);
            }

            var brightness = CalculateBrightness(r, g, b);

            var adjustK = brightness switch
            {
                > brightnessMax => brightnessMax / brightness,
                < brightnessMin => brightnessMin / brightness,
                _ => 1.0
            };

            if (Math.Abs(1.0 - adjustK) < errorDouble)
                return Color.FromArgb(255, ColorComponent(r), ColorComponent(g), ColorComponent(b));
            r *= adjustK;
            g *= adjustK;
            b *= adjustK;

            return Color.FromArgb(255, ColorComponent(r), ColorComponent(g), ColorComponent(b));

            static byte ColorComponent(double component)
            {
                if (component - 1.0 > errorDouble)
                {
                    component = 1.0;
                }
                return (byte) (component * 255);
            }
        }

        private static double CalculateBrightness(in double r, in double g, in double b)
        {
            return Math.Sqrt(r * r * 0.241 + g * g * 0.691 + b * b * 0.068);
        }

        /*private static Color FromHSV(double hue, double saturation, double brightness)
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
        }*/
    }
}
