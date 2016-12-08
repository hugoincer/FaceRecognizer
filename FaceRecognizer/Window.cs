using FaceRecognizer.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FaceRecognizer
{
    public class Window
    {
        public static int WindowHeight { get; set; }
        public static int WindowWidth { get; set; }

        private static int _xStart = 0;
        private static int _yStart = 0;
        
        public Point TopLeft;

        public float SizeRatio;

        private CustomImage _image;

        public readonly int Deviation;

        private static float _windowScale = 1.5f;
        public static int WindowXstep = 5;
        public static int WindowYstep = 5;


        public int Width
        {
            get
            {
                return (int)(WindowWidth * SizeRatio);
            }
        }

        public int Height
        {
            get
            {
                return (int)(WindowHeight * SizeRatio);
            }
        }

        public Window(Point TopLeft, float SizeRatio, CustomImage Image, CustomImage SquaredImage)
        {
            this.TopLeft = TopLeft;
            this.SizeRatio = SizeRatio;
            this._image = Image;
            this.Deviation = 0;
            this.Deviation = this.GetDeviation(Image, SquaredImage);

        }
        private int GetDeviation(CustomImage Image, CustomImage SquaredImage)
        {

            var nPixs = this.Width * this.Height;
            // if (Width > Image.Width || Height > Image.Height) return 1;
            var aCoords = this.TopLeft;
            var bCoords = aCoords.Translate(this.Width, 0);
            var cCoords = aCoords.Translate(0, this.Height);
            var dCoords = cCoords.Translate(this.Width, 0);

            var a = Image.GetValue(aCoords);
            var b = Image.GetValue(bCoords);
            var c = Image.GetValue(cCoords);
            var d = Image.GetValue(dCoords);

            var squaredA = SquaredImage.GetValue(aCoords);
            var squaredB = SquaredImage.GetValue(bCoords);
            var squaredC = SquaredImage.GetValue(cCoords);
            var squaredD = SquaredImage.GetValue(dCoords);

            var sum = d - (b + c) + a;
            var squaredSum = squaredD - (squaredB + squaredC) + squaredA;

            var avg = sum / nPixs;

            var variance = squaredSum / nPixs - avg * avg;

            if (variance > 0)
                return (int)Math.Sqrt(variance);
            else
                return 1;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(this.TopLeft, new Size(this.Width, this.Height));
        }

        public static IEnumerable<Window> ListWindows(CustomImage Image, CustomImage SquaredImage)
        {
            var maxX = Image.Width - WindowWidth;
            var maxY = Image.Height - WindowHeight;

            for (var x = _xStart; x <= maxX; x += WindowXstep)
            {
                for (var y = _yStart; y <= maxY; y += WindowYstep)
                {
                    var maxWidth = Image.Width - x;
                    var maxHeight = Image.Height - y;
                    var width = WindowWidth;
                    var height = WindowHeight;
                    var ratio = 1f;
                    while (width <= maxWidth && height <= maxHeight)
                    {
                        yield return new Window(new Point(x, y), ratio, Image, SquaredImage);

                        ratio *= _windowScale;
                        width = (int)(WindowWidth * ratio);
                        height = (int)(WindowHeight * ratio);
                    }
                }
            }
        }

    }
}
