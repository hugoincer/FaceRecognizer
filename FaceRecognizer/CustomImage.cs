using FaceRecognizer.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FaceRecognizer
{
    public class CustomImage
    {
        public long[,] integralTable;
        public byte[,] grayPixels;

        private const byte _defaultRvalue = 255;
        private const byte _defaultGvalue = 255;
        private const byte _defaultBvalue = 255;

        public Func<long, long> Type;
        public int Width { get; set; }
        public int Height { get; set; }


        private List<Pixel> _pixels = new List<Pixel>();
        private List<Pixel> _orignalPixels = new List<Pixel>();
        public List<Pixel> Pixels
        {
            get { return _pixels; }
            private set { _pixels = value; }
        }

        public Pixel this[int x, int y]
        {
            get { return Pixels[x + Width * y]; }
            set { Pixels[x + Width * y] = value; }
        }

        public int PixelCounts => Pixels.Count;

        public CustomImage(string path)
        {
            var CustomImage = new Bitmap(path);
            Type = (pix) => pix;
            integralTable = new long[CustomImage.Width, CustomImage.Height];
            grayPixels = new byte[CustomImage.Width, CustomImage.Height];
            LoadPixels(CustomImage);
            ComputeIntegralImage();
        }


        public CustomImage(CustomImage src) : this(src, (pix) => pix)
        {
        }

        public CustomImage(Bitmap src)
        {
            Type = (pix) => pix;
            integralTable = new long[src.Width, src.Height];
            grayPixels = new byte[src.Width, src.Height];
            LoadPixels(src);
            ComputeIntegralImage();
        }


        public CustomImage(CustomImage src, Func<long, long> Type)
        {
            Pixels = new List<Pixel>(src.Pixels);
            integralTable = new long[src.Width, src.Height];
            
            grayPixels = src.grayPixels;
            Width = src.Width;
            Height = src.Height;
            this.Type = Type;
            ComputeIntegralImage();
        }
        
        public Bitmap GetImage()
        {
            var bitmap = Pixels.GetRange(0, Width * Height).SelectMany(p => new[] { p.Blue, p.Green, p.Red }).ToArray();
            var bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            
            var data = bmp.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            var stride = data.Stride;
            Marshal.Copy(bitmap, 0, data.Scan0, bitmap.Length);

            bmp.UnlockBits(data);

            return bmp;
        }

        private void LoadPixels(Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            Height = bmp.Height;
            Width = bmp.Width;

            byte[] rgbValues = new byte[bmpData.Stride * bmp.Height];
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, rgbValues.Length);
            for (int counter = 0; counter <= rgbValues.Length - 3; counter += 3)
            {
                Pixels.Add(new Pixel
                {
                    Blue = rgbValues[counter],
                    Green = rgbValues[counter + 1],
                    Red = rgbValues[counter + 2]
                });
            }
            
            for (int i = 0; i < Width; i++)
            {
                Pixels.Add(new Pixel(255, 255, 25));
            }
            bmp.UnlockBits(bmpData);
        }

        public IEnumerator<Pixel> GetEnumerator()
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    yield return this[j, i];
                }
            }
        }

        public long GetValue(Point Coords)
        {
            if (Coords.X == 0 || Coords.Y == 0)
                return 0L;
            else
                return this.integralTable[Coords.X - 1, Coords.Y - 1];
        }
        public void DrawRectangle(Rectangle r)
        {
            for (int i = r.X; i < r.X + r.Width; i++)
            {
                this[i, r.Y] = new Pixel(_defaultRvalue, _defaultGvalue, _defaultBvalue);
                this[i, r.Y + r.Height] = new Pixel(_defaultRvalue, _defaultGvalue, _defaultBvalue);
            }
            for (int i = r.Y; i < r.Y + r.Height; i++)
            {
                this[r.X, i] = new Pixel(_defaultRvalue, _defaultGvalue, _defaultBvalue);
                this[r.X + r.Width, i] = new Pixel(_defaultRvalue, _defaultGvalue, _defaultBvalue);
            }

        }
        public void ComputeIntegralImage()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    this.grayPixels[i, j] = this[i, j].GetGray();
                    this.integralTable[i, j] = this.grayPixels[i, j];
                }
            }

            for (int i = 1; i < Width; i++)
            {
                this.integralTable[i, 0] += this.Type(this.grayPixels[i - 1, 0]);
            }

            for (int i = 1; i < Height; i++)
            {
                this.integralTable[0, i] += this.Type(this.grayPixels[0, i - 1]);
            }

            for (int i = 1; i < Width - 1; i++)
            {
                for (int j = 1; j < Height - 1; j++)
                {
                    this.integralTable[i, j] =
                        this.Type(this.grayPixels[i, j]) +
                        this.integralTable[i - 1, j] +
                        this.integralTable[i, j - 1] -
                        this.integralTable[i - 1, j - 1];
                }
            }
        }

    }
}
