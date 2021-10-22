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
            const double BRIGHTNESS_MAX = 0.65;
            const double BRIGHTNESS_MIN = 0.30;
            const double ERROR_DOUBLE = 0.0000001;
            const double ERROR_COLOR = 0.004;

            static byte ColorComponent(double component)
            {
                if (component - 1.0 > ERROR_DOUBLE)
                {
                    component = 1.0;
                }
                return (byte) (component * 255);
            }

            double r = random.NextDouble();
            double g = random.NextDouble();
            double b = random.NextDouble();

            if (r < ERROR_COLOR && g < ERROR_COLOR && b < ERROR_COLOR)
            {
                byte c = (byte) (System.Math.Sqrt(BRIGHTNESS_MIN * BRIGHTNESS_MIN / 3.0) * 255);
                return Color.FromArgb(255, c, c, c);
            }

            double brightness = CalculateBrightness(r, g, b);
            double adjustK = 1.0;

            if (brightness > BRIGHTNESS_MAX)
            {
                adjustK = BRIGHTNESS_MAX / brightness;
            }
            else if (brightness < BRIGHTNESS_MIN)
            {
                adjustK = BRIGHTNESS_MIN / brightness;
            }

            if (System.Math.Abs(1.0 - adjustK) > ERROR_DOUBLE)
            {
                r *= adjustK;
                g *= adjustK;
                b *= adjustK;
            }

            return Color.FromArgb(255, ColorComponent(r), ColorComponent(g), ColorComponent(b));
        }

        private static double CalculateBrightness(in double r, in double g, in double b)
        {
            return System.Math.Sqrt(r * r * 0.241 + g * g * 0.691 + b * b * 0.068);
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
