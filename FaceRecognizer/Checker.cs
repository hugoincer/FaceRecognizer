using FaceRecognizer.Feature;

namespace FaceRecognizer
{

    public class Checker
    {
        public double Alpha;

        public int Threshold;
        public sbyte Parity;

        public IFeature Feature;
        public Checker(double Alpha, int Threshold, sbyte Parity, IFeature Feature)
        {
            this.Alpha = Alpha;
            this.Threshold = Threshold;
            this.Parity = Parity;
            this.Feature = Feature;
        }
        public bool Check(Window Win, CustomImage Image)
        {
            var featureValue = this.Feature.ComputeValue(Win.TopLeft, Win.SizeRatio, Image);
            var sizedValue = (int)(featureValue / (Win.SizeRatio * Win.SizeRatio));
            var normalizedValue = NormalizeFeature(sizedValue, Win.Deviation);

            return this.Parity * normalizedValue < this.Parity * this.Threshold;
        }

        public static int NormalizeFeature(int FeatureValue, int Derivation)
        {
            return (FeatureValue * 40) / Derivation;
        }

        public double GetValue(Window Win, CustomImage Image)
        {
            if (this.Check(Win, Image))
                return this.Alpha;
            else
                return 0;
        }
    }
}
