using System;
using System.Drawing;

namespace FaceRecognizer.Feature
{
    public static class FeatureFactory
    {
        public static IFeature GetFeature(string name, Rectangle frame)
        {
            if ("TwoHorizontalRectanglesFeature".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return new TwoHorizontalRectanglesFeature(frame);
            if ("TwoVerticalRectanglesFeature".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return new TwoVerticalRectanglesFeature(frame);
            if ("ThreeHorizontalRectanglesFeature".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return new ThreeHorizontalRectanglesFeature(frame);
            if ("ThreeVerticalRectanglesFeature".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return new ThreeVerticalRectanglesFeature(frame);
            if ("FourRectanglesFeature".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return new FourRectanglesFeature(frame);
            throw new ArgumentException("Unknown feature!");

        }

    }
}
