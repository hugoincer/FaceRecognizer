using Lab4.Image.Cascade;
using System.Drawing;

namespace Lab4.Image
{
    public class ImageModifier
    {
        HaarObjectDetector detector;

        public ImageModifier()
        {
            HaarCascade cascade = new FaceHaarCascade();
            detector = new HaarObjectDetector(cascade, 30);
        }

        public Rectangle[] DetectFaces(Bitmap image)
        {
            detector.SearchMode = ObjectDetectorSearchMode.Default;
            detector.ScalingMode = ObjectDetectorScalingMode.GreaterToSmaller;
            detector.ScalingFactor = 1.5f;
            detector.UseParallelProcessing = false;

            // Process frame to detect objects
            Rectangle[] objects = detector.ProcessFrame(image);

            return objects;
        }
    }
}
