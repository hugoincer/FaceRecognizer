using System.Drawing;

namespace FaceRecognizer.Feature
{
    public interface IFeature
    {
        Rectangle Frame { get; }
        int ComputeValue(Point WinTopLeft, float SizeRatio, CustomImage Image);
        int ComputeValue(CustomImage Image);
    }
}
