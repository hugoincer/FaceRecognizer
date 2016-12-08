using FaceRecognizer.Feature;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FaceRecognizer
{
    public class Classifier
    {
        public Checker[] Checkers;

        public double GlobalAlpha;
        public Classifier(params Checker[] Checkers)
        {
            this.Checkers = Checkers;

            Func<double, Checker, double> sum = (acc, classifier) =>
                acc + classifier.Alpha;

            this.GlobalAlpha = this.Checkers.Aggregate(0.0, sum);
        }

        public static Classifier LoadFromFile(string Path)
        {
            Func<string, Checker> RestoreClassifier = (str) => {
                string[] vals = str.Split(';');

                var alpha = double.Parse(vals[0]);
                var threshold = int.Parse(vals[1]);
                var parity = sbyte.Parse(vals[2]);
                var featureName = vals[3];
                var featureX = int.Parse(vals[4]);
                var featureY = int.Parse(vals[5]);
                var featureWidth = int.Parse(vals[6]);
                var featureHeight = int.Parse(vals[7]);
                var featureFrame = new Rectangle(new Point(featureX, featureY), new Size(featureWidth, featureHeight));

                var feature = FeatureFactory.GetFeature(vals[3], featureFrame);

                return new Checker(alpha, threshold, parity, feature);
            };

            var classifiers = File.ReadAllLines(Path).Select(RestoreClassifier)
                                                     .ToArray();

            return new Classifier(classifiers);
        }
        public bool Check(Window Win, CustomImage Image)
        {
            double sumValues = 0.0;
            foreach (var weakClassifier in this.Checkers)
                sumValues += weakClassifier.GetValue(Win, Image);

            return sumValues >= this.GlobalAlpha / 2;
        }
    }
}

