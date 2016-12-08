using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer
{
    public class Pixel
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public Pixel (byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public Pixel() { }
    }
}
