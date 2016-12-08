using FaceRecognizer.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer.Feature
{
    public abstract class BasicHaarFeature : IFeature
    {
        public abstract Rectangle Frame { get; set; }

        public abstract int ComputeValue(CustomImage Image);

        public abstract int ComputeValue(Point WinTopLeft, float SizeRatio, CustomImage Image);

        public ImageCoords? GetImageCoords(Rectangle frame, CustomImage img, Point WinTopLeft)
        {
            var topLeft = frame.TopLeft().NestedPoint(WinTopLeft);
            if (topLeft.X + frame.Width > img.Width || topLeft.Y + frame.Height > img.Height)
            {
                return null;
            }

            var rectsWidth = frame.Width / 3;
            var rectsHeight = frame.Height;

            var aCoords = topLeft;
            var bCoords = aCoords.Translate(rectsWidth, 0);
            var cCoords = bCoords.Translate(rectsWidth, 0);
            var dCoords = cCoords.Translate(rectsWidth, 0);

            var eCoords = aCoords.Translate(0, rectsHeight);
            var fCoords = eCoords.Translate(rectsWidth, 0);
            var gCoords = fCoords.Translate(rectsWidth, 0);
            var hCoords = gCoords.Translate(rectsWidth, 0);

            ImageCoords result = new ImageCoords();

            result.A = img.GetValue(aCoords);
            result.B = img.GetValue(bCoords);
            result.C = img.GetValue(cCoords);
            result.D = img.GetValue(dCoords);
            result.E = img.GetValue(eCoords);
            result.F = img.GetValue(fCoords);
            result.G = img.GetValue(gCoords);
            result.H = img.GetValue(hCoords);

            return result;
        }
    }
}
