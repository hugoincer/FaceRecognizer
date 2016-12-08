using System;
using System.Drawing;


namespace FaceRecognizer
{
    public class CustomColor
    {
        private CustomColor()
        {

        }

        private int R { get; set; }

        private int G { get; set; }

        private int B { get; set; }

        public CustomColor(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public Color GetColor()
        {
            return Color.FromArgb(R <= 255 ? R : 255, G <= 255 ? G : 255, B <= 255 ? B : 255);
        }

        public static CustomColor operator +(CustomColor a, CustomColor b)
        {
            CustomColor c = new CustomColor();
            c.R = a.R + b.R;
            c.G = a.G + b.G;
            c.B = a.B + b.B;

            return c;
        }

        public static CustomColor operator -(CustomColor a, CustomColor b)
        {
            CustomColor c = new CustomColor();
            c.R = Math.Abs(a.R - b.R);
            c.G = Math.Abs(a.G - b.G);
            c.B = Math.Abs(a.B - b.B);

            return c;
        }

        public static CustomColor operator *(CustomColor a, CustomColor b)
        {
            CustomColor c = new CustomColor();
            c.R = a.R * b.R;
            c.G = a.G * b.G;
            c.B = a.B * b.B;

            return c;
        }

        public static CustomColor Sqr(CustomColor a, CustomColor b)
        {
            return a * b;
        }

        public static CustomColor Sqrt(CustomColor x)
        {
            if (x.R < 0 || x.G < 0 || x.B < 0)
            {
                throw new ArgumentException();
            } 

            CustomColor c = new CustomColor();
            c.R = Convert.ToInt32(Math.Sqrt(x.R));
            c.G = Convert.ToInt32(Math.Sqrt(x.G));
            c.B = Convert.ToInt32(Math.Sqrt(x.B));

            return c;
        }
    }
}
