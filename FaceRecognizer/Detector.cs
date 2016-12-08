using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer
{
    public class Detector
    {
        public readonly CustomImage CustomImage;
        public readonly Classifier Classifier;
        public readonly CustomImage SquaredImage;
        public Detector(CustomImage CustomImage, CustomImage SquaredImage, Classifier Classifier)
        {
            this.CustomImage = CustomImage;
            this.Classifier = Classifier;
            this.SquaredImage = SquaredImage;
        }
        public Detector(CustomImage CustomImage, Classifier Classifier) : this(new CustomImage(CustomImage), new CustomImage(CustomImage, (pix) => (long)pix * pix), Classifier)
        {
        }
        public IEnumerable<Window> Detect()
        {
            Func<Window, bool> check = (win) =>
                this.Classifier.Check(win, this.CustomImage);
            foreach (var item in Window.ListWindows(this.CustomImage, this.SquaredImage))
            {
                if (check(item)) yield return item;
            }
        }
    }
}
