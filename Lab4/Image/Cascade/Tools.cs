using System;
using System.Drawing;

namespace Lab4.Image.Cascade
{
    public static class Tools
    {

        private const double SQRT2 = 1.4142135623730951;

        public static bool IsEqual(this Rectangle objA, Rectangle objB, int threshold)
        {
            return (Math.Abs(objA.X - objB.X) < threshold) &&
                   (Math.Abs(objA.Y - objB.Y) < threshold) &&
                   (Math.Abs(objA.Width - objB.Width) < threshold) &&
                   (Math.Abs(objA.Height - objB.Height) < threshold);
        }

    }
}
