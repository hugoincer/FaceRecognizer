using System;
using System.Drawing;

namespace Lab4.Image.Cascade
{
    [Serializable]
    public class HaarClassifier
    {

        private HaarCascade cascade;

        private float invArea;
        private float scale;


        public HaarClassifier(HaarCascade cascade)
        {
            this.cascade = cascade;
        }
 
        public HaarClassifier(int baseWidth, int baseHeight, HaarCascadeStage[] stages)
            : this(new HaarCascade(baseWidth, baseHeight, stages))
        {
        }


        public HaarCascade Cascade
        {
            get { return cascade; }
        }
 
        public float Scale
        {
            get { return this.scale; }
            set
            {
                if (this.scale == value)
                    return;

                this.scale = value;
                this.invArea = 1f / (cascade.Width * cascade.Height * scale * scale);

                // For each stage in the cascade 
                foreach (HaarCascadeStage stage in cascade.Stages)
                {
                    // For each tree in the cascade
                    foreach (HaarFeatureNode[] tree in stage.Trees)
                    {
                        // For each feature node in the tree
                        foreach (HaarFeatureNode node in tree)
                        {
                            // Set the scale and weight for the node feature
                            node.Feature.SetScaleAndWeight(value, invArea);
                        }
                    }
                }
            }
        }


        public bool Compute(IntegralImage image, Rectangle rectangle)
        {
            int x = rectangle.X;
            int y = rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            double mean = image.GetSum(x, y, w, h) * invArea;
            double factor = image.GetSum2(x, y, w, h) * invArea - (mean * mean);

            factor = (factor >= 0) ? Math.Sqrt(factor) : 1;


            // For each classification stage in the cascade
            foreach (HaarCascadeStage stage in cascade.Stages)
            {
                // Check if the stage has rejected the image
                if (stage.Classify(image, x, y, factor) == false)
                {
                    return false; // The image has been rejected.
                }
            }

            // If the object has gone all stages and has not
            //  been rejected, the object has been detected.

            return true; // The image has been detected.
        }


    }
}
