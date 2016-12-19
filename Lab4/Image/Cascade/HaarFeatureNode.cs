using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lab4.Image.Cascade
{
    [Serializable]
    public class HaarFeatureNode : ICloneable
    {
        private int rightNodeIndex = -1;
        private int leftNodeIndex = -1;

        [XmlElement("threshold")]
        public double Threshold { get; set; }

        [XmlElement("left_val")]
        public double LeftValue { get; set; }

        [XmlElement("right_val")]
        public double RightValue { get; set; }

        [XmlElement("left_node")]
        public int LeftNodeIndex
        {
            get { return leftNodeIndex; }
            set { leftNodeIndex = value; }
        }

        [XmlElement("right_node")]
        public int RightNodeIndex
        {
            get { return rightNodeIndex; }
            set { rightNodeIndex = value; }
        }

        [XmlElement("feature", IsNullable = false)]
        public HaarFeature Feature { get; set; }

        public HaarFeatureNode()
        {
        }

        public HaarFeatureNode(double threshold, double leftValue, double rightValue, params int[][] rectangles)
            : this(threshold, leftValue, rightValue, false, rectangles)
        {
        }

        public HaarFeatureNode(double threshold, double leftValue, double rightValue, bool tilted, params int[][] rectangles)
        {
            this.Feature = new HaarFeature(tilted, rectangles);
            this.Threshold = threshold;
            this.LeftValue = leftValue;
            this.RightValue = rightValue;
        }

        public object Clone()
        {
            HaarFeatureNode r = new HaarFeatureNode();

            r.Feature = (HaarFeature)Feature.Clone();
            r.Threshold = Threshold;

            r.RightValue = RightValue;
            r.LeftValue = LeftValue;

            r.LeftNodeIndex = leftNodeIndex;
            r.RightNodeIndex = rightNodeIndex;

            return r;
        }
    }
}
