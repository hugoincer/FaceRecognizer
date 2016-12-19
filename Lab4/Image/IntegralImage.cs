using Lab4.Image.Cascade;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lab4.Image
{
    public unsafe class IntegralImage
    {
        private int[,] nSumImage; // normal  integral image
        private int[,] sSumImage; // squared integral image
        private int[,] tSumImage; // tilted  integral image

        private int* nSum; // normal  integral image
        private int* sSum; // squared integral image
        private int* tSum; // tilted  integral image

        private GCHandle nSumHandle;
        private GCHandle sSumHandle;
        private GCHandle tSumHandle;

        private int width;
        private int height;

        private int nWidth;
        private int nHeight;

        private int tWidth;
        private int tHeight;


        /// <summary>
        ///   Gets the image's width.
        /// </summary>
        /// 
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        ///   Gets the image's height.
        /// </summary>
        /// 
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        ///   Gets the Integral Image for values' sum.
        /// </summary>
        /// 
        public int[,] Image
        {
            get { return nSumImage; }
        }

        /// <summary>
        ///   Gets the Integral Image for values' squared sum.
        /// </summary>
        /// 
        public int[,] Squared
        {
            get { return sSumImage; }
        }

        /// <summary>
        ///   Gets the Integral Image for tilted values' sum.
        /// </summary>
        /// 
        public int[,] Rotated
        {
            get { return tSumImage; }
        }

        /// <summary>
        ///   Constructs a new Integral image of the given size.
        /// </summary>
        /// 
        protected IntegralImage(int width, int height)
        {
            this.width = width;
            this.height = height;

            this.nWidth = width + 1;
            this.nHeight = height + 1;

            this.tWidth = width + 2;
            this.tHeight = height + 2;

            this.nSumImage = new int[nHeight, nWidth];
            this.nSumHandle = GCHandle.Alloc(nSumImage, GCHandleType.Pinned);
            this.nSum = (int*)nSumHandle.AddrOfPinnedObject().ToPointer();

            this.sSumImage = new int[nHeight, nWidth];
            this.sSumHandle = GCHandle.Alloc(sSumImage, GCHandleType.Pinned);
            this.sSum = (int*)sSumHandle.AddrOfPinnedObject().ToPointer();            
        }

        public static IntegralImage FromBitmap(Bitmap image, int channel, bool computeTilted)
        {
            // lock source image
            BitmapData imageData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            // process the image
            IntegralImage im = FromBitmap(imageData, channel, computeTilted);

            // unlock image
            image.UnlockBits(imageData);

            return im;
        }

        public static IntegralImage FromBitmap(BitmapData imageData, int channel, bool computeTilted)
        {
            return FromBitmap(new UnmanagedImage(imageData), channel, computeTilted);
        }

        public static IntegralImage FromBitmap(Bitmap image, int channel)
        {
            // check image format
            // lock source image
            BitmapData imageData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            // process the image
            IntegralImage im = FromBitmap(imageData, channel);

            // unlock image
            image.UnlockBits(imageData);

            return im;
        }

        public static IntegralImage FromBitmap(BitmapData imageData, int channel)
        {
            return FromBitmap(new UnmanagedImage(imageData), channel);
        }

        public static IntegralImage FromBitmap(UnmanagedImage image, int channel)
        {
            int pixelSize = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;

            // get source image size
            int width = image.Width;
            int height = image.Height;
            int stride = image.Stride;
            int offset = stride - width * pixelSize;

            // create integral image
            IntegralImage im = new IntegralImage(width, height);
            int* nSum = im.nSum, sSum = im.sSum, tSum = im.tSum;

            int nWidth = im.nWidth, nHeight = im.nHeight;
            int tWidth = im.tWidth, tHeight = im.tHeight;

            if (image.PixelFormat == PixelFormat.Format8bppIndexed && channel != 0)
                throw new ArgumentException("Only the first channel is available for 8 bpp images.", "channel");

            byte* srcStart = (byte*)image.ImageData.ToPointer() + channel;

            // do the job
            byte* src = srcStart;

            // for each line
            for (int y = 1; y <= height; y++)
            {
                int yy = nWidth * (y);
                int y1 = nWidth * (y - 1);

                // for each pixel
                for (int x = 1; x <= width; x++, src += pixelSize)
                {
                    int p1 = *src;
                    int p2 = p1 * p1;

                    int r = yy + (x);
                    int a = yy + (x - 1);
                    int b = y1 + (x);
                    int c = y1 + (x - 1);

                    nSum[r] = p1 + nSum[a] + nSum[b] - nSum[c];
                    sSum[r] = p2 + sSum[a] + sSum[b] - sSum[c];
                }
                src += offset;
            }

            return im;
        }

        public int GetSum(int x, int y, int width, int height)
        {
            int a = nWidth * (y) + (x);
            int b = nWidth * (y + height) + (x + width);
            int c = nWidth * (y + height) + (x);
            int d = nWidth * (y) + (x + width);

            return nSum[a] + nSum[b] - nSum[c] - nSum[d];
        }

        public int GetSum2(int x, int y, int width, int height)
        {
            int a = nWidth * (y) + (x);
            int b = nWidth * (y + height) + (x + width);
            int c = nWidth * (y + height) + (x);
            int d = nWidth * (y) + (x + width);

            return sSum[a] + sSum[b] - sSum[c] - sSum[d];
        }

        public int GetSumT(int x, int y, int width, int height)
        {
            int a = tWidth * (y + width) + (x + width + 1);
            int b = tWidth * (y + height) + (x - height + 1);
            int c = tWidth * (y) + (x + 1);
            int d = tWidth * (y + width + height) + (x + width - height + 1);

            return tSum[a] + tSum[b] - tSum[c] - tSum[d];
        }

        protected IntegralImage(int width, int height, bool computeTilted)
        {
            this.width = width;
            this.height = height;

            this.nWidth = width + 1;
            this.nHeight = height + 1;

            this.tWidth = width + 2;
            this.tHeight = height + 2;

            this.nSumImage = new int[nHeight, nWidth];
            this.nSumHandle = GCHandle.Alloc(nSumImage, GCHandleType.Pinned);
            this.nSum = (int*)nSumHandle.AddrOfPinnedObject().ToPointer();

            this.sSumImage = new int[nHeight, nWidth];
            this.sSumHandle = GCHandle.Alloc(sSumImage, GCHandleType.Pinned);
            this.sSum = (int*)sSumHandle.AddrOfPinnedObject().ToPointer();

            if (computeTilted)
            {
                this.tSumImage = new int[tHeight, tWidth];
                this.tSumHandle = GCHandle.Alloc(tSumImage, GCHandleType.Pinned);
                this.tSum = (int*)tSumHandle.AddrOfPinnedObject().ToPointer();
            }
        }

        public static IntegralImage FromBitmap(UnmanagedImage image, int channel, bool computeTilted
            /*, TODO: Rectangle roi*/)
        {


            int pixelSize = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;

            // get source image size
            int width = image.Width;
            int height = image.Height;
            int stride = image.Stride;
            int offset = stride - width * pixelSize;

            // create integral image
            IntegralImage im = new IntegralImage(width, height, computeTilted);
            int* nSum = im.nSum, sSum = im.sSum, tSum = im.tSum;

            int nWidth = im.nWidth, nHeight = im.nHeight;
            int tWidth = im.tWidth, tHeight = im.tHeight;

            if (image.PixelFormat == PixelFormat.Format8bppIndexed && channel != 0)
                throw new ArgumentException("Only the first channel is available for 8 bpp images.", "channel");

            byte* srcStart = (byte*)image.ImageData.ToPointer() + channel;

            // do the job
            byte* src = srcStart;

            // for each line
            for (int y = 1; y <= height; y++)
            {
                int yy = nWidth * (y);
                int y1 = nWidth * (y - 1);

                // for each pixel
                for (int x = 1; x <= width; x++, src += pixelSize)
                {
                    int p1 = *src;
                    int p2 = p1 * p1;

                    int r = yy + (x);
                    int a = yy + (x - 1);
                    int b = y1 + (x);
                    int c = y1 + (x - 1);

                    nSum[r] = p1 + nSum[a] + nSum[b] - nSum[c];
                    sSum[r] = p2 + sSum[a] + sSum[b] - sSum[c];
                }
                src += offset;
            }


            if (computeTilted)
            {
                src = srcStart;

                // Left-to-right, top-to-bottom pass
                for (int y = 1; y <= height; y++, src += offset)
                {
                    int yy = tWidth * (y);
                    int y1 = tWidth * (y - 1);

                    for (int x = 2; x < width + 2; x++, src += pixelSize)
                    {
                        int a = y1 + (x - 1);
                        int b = yy + (x - 1);
                        int c = y1 + (x - 2);
                        int r = yy + (x);

                        tSum[r] = *src + tSum[a] + tSum[b] - tSum[c];
                    }
                }

                {
                    int yy = tWidth * (height);
                    int y1 = tWidth * (height + 1);

                    for (int x = 2; x < width + 2; x++, src += pixelSize)
                    {
                        int a = yy + (x - 1);
                        int c = yy + (x - 2);
                        int b = y1 + (x - 1);
                        int r = y1 + (x);

                        tSum[r] = tSum[a] + tSum[b] - tSum[c];
                    }
                }


                // Right-to-left, bottom-to-top pass
                for (int y = height; y >= 0; y--)
                {
                    int yy = tWidth * (y);
                    int y1 = tWidth * (y + 1);

                    for (int x = width + 1; x >= 1; x--)
                    {
                        int r = yy + (x);
                        int b = y1 + (x - 1);

                        tSum[r] += tSum[b];
                    }
                }

                for (int y = height + 1; y >= 0; y--)
                {
                    int yy = tWidth * (y);

                    for (int x = width + 1; x >= 2; x--)
                    {
                        int r = yy + (x);
                        int b = yy + (x - 2);

                        tSum[r] -= tSum[b];
                    }
                }
            }


            return im;
        }
    }
}
