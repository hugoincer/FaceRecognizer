using FaceRecognizer.Extension;
using System.Drawing;

namespace FaceRecognizer.Feature
{
    public class ThreeHorizontalRectanglesFeature :BasicHaarFeature,IFeature
    {
        public const int minWidth = 3;
        public const int minHeight = 1;

        public override Rectangle Frame { get; set; }

        public ThreeHorizontalRectanglesFeature(Rectangle Frame)
        {
            this.Frame = Frame;
        }

        public override int ComputeValue(Point WinTopLeft, float SizeRatio, CustomImage Image)
        {

            var scaledFrame = Frame.Scale(SizeRatio);

            var coords = GetImageCoords(scaledFrame, Image, WinTopLeft);
            if (!coords.HasValue)
                return 0;
            var coordsValue = coords.Value;
            var sumR1 = coordsValue.F - (coordsValue.B + coordsValue.E) + coordsValue.A;
            var sumR2 = coordsValue.G - (coordsValue.C + coordsValue.F) + coordsValue.B;
            var sumR3 = coordsValue.H - (coordsValue.D + coordsValue.G) + coordsValue.C;

            return (int)(sumR1 - sumR2 + sumR3);
        }

        public override int ComputeValue(CustomImage Image)
        {
            var topLeft = new Point(0, 0);
            return this.ComputeValue(topLeft, 1f, Image);
        }
    }
}
