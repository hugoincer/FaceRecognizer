using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace FaceRecognizer
{
    public abstract class AFilter
    {

        public abstract Bitmap filter(Bitmap _sourceImage);

        public CustomColor GetPixel(Bitmap image, int x, int y)
        {
            return new CustomColor(image.GetPixel(x, y));
        }

        public class ColorWithGrey
        {
            public Color Color;
            public int grey;

            public ColorWithGrey(Color color)
            {
                this.Color = color;
                var R = color.R;
                var G = color.G;
                var B = color.B;
                var rgb = 0.3 * R + 0.59 * G + 0.11 * B;
                var rgb_out = AroundPixel((int)rgb);
                this.grey = rgb_out;
            }
            private int AroundPixel(int value)
            {
                if (value > 255)
                {
                    return 255;
                }
                if (value < 0)
                {
                    return 0;
                }
                return value;
            }

        }
    }

    public class Roberts : AFilter
    {

        public override Bitmap filter(Bitmap _sourceImage)
        {

            var newImage = new Bitmap(_sourceImage);
            for (int i = 0; i < _sourceImage.Width; i++)
            {
                for (int j = 0; j < _sourceImage.Height; j++)
                {
                    CustomColor output;
                    //int revertJ
                    if ((i == _sourceImage.Width - 1) || (j == _sourceImage.Height - 1))
                    {
                        output = GetPixel(newImage, i, j);
                    }
                    else
                    {

                        CustomColor newX = GetPixel(newImage, i + 1, j + 1) - GetPixel(newImage, i, j);
                        var newY = GetPixel(newImage, i + 1, j) - GetPixel(newImage, i, j + 1);
                        output = CustomColor.Sqrt(newX * newX + newY * newY);
                        //output = (int)Math.Sqrt(newX * newX + newY * newY);
                        newImage.SetPixel(i, j, output.GetColor());
                    }
                }
            }
            return newImage;
        }
    }

    class Median : AFilter
    {
        private int mask;

        public Median(int mask)
        {
            this.mask = mask;
        }

        public override Bitmap filter(Bitmap image)
        {

            var outBitmap = new Bitmap(image);
            var width = image.Height;
            var heigth = image.Width;
            var medianInd = (int)mask * mask / 2;


            for (int x = mask; x < heigth - mask; x++)
            {
                for (int y = mask; y < width - mask; y++)
                {
                    var dict = new Dictionary<ColorWithGrey, int>();
                    for (int i = -(mask - 2); i < mask - 1; i++)
                    {
                        if (i != 0)
                        {
                            // 1,1 -1,-1
                            var colorWithGrey = new ColorWithGrey(image.GetPixel(x + i, y + i));
                            dict.Add(colorWithGrey, colorWithGrey.grey);

                            // 0,1 //  0,-1
                            colorWithGrey = new ColorWithGrey(image.GetPixel(x, y + i));
                            dict.Add(colorWithGrey, colorWithGrey.grey);

                            // 1,0  // -1,0
                            colorWithGrey = new ColorWithGrey(image.GetPixel(x + i, y));
                            dict.Add(colorWithGrey, colorWithGrey.grey);

                            // -1,1  // 1,-1
                            colorWithGrey = new ColorWithGrey(image.GetPixel(x + i, y - i));
                            dict.Add(colorWithGrey, colorWithGrey.grey);

                        }
                        else
                        {
                            var colorWithGrey = new ColorWithGrey(image.GetPixel(x + i, y + i));
                            dict.Add(colorWithGrey, colorWithGrey.grey);
                        }
                    }
                    //  int[] terms = list.ToArray();
                    dict = dict.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                    var val = dict.ToList()[medianInd];
                    var color = val.Key.Color;
                    //var color = val.Value.;
                    outBitmap.SetPixel(x, y, color);
                }
            }
            return outBitmap;
        }
    }



    public class Gaussian : AFilter
    {
        private int length;
        private double weight;

        public Gaussian(int length, double weight)
        {
            this.length = length;
            this.weight = weight;
        }

        public override Bitmap filter(Bitmap image)
        {
            return ConvolutionFilter(image, Calculate(length, weight));
        }

        public static Bitmap ConvolutionFilter(Bitmap sourceBitmap,
                                             double[,] filterMatrix,
                                                  double factor = 1,
                                                       int bias = 0)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                    sourceBitmap.Width, sourceBitmap.Height),
                                                      ImageLockMode.ReadOnly,
                                                PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;

            int filterWidth = filterMatrix.GetLength(1);
            int filterHeight = filterMatrix.GetLength(0);

            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;

            int byteOffset = 0;


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;

                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            blue += (double)(pixelBuffer[calcOffset]) *
                                    filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            green += (double)(pixelBuffer[calcOffset + 1]) *
                                     filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            red += (double)(pixelBuffer[calcOffset + 2]) *
                                   filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }


                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;


                    blue = (blue > 255 ? 255 : (blue < 0 ? 0 : blue));
                    green = (green > 255 ? 255 : (green < 0 ? 0 : green));
                    red = (red > 255 ? 255 : (red < 0 ? 0 : red));

                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                                                      ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        public static double[,] Calculate(int length, double weight)
        {
            double[,] Kernel = new double[length, length];
            double sumTotal = 0;


            int kernelRadius = length / 2;
            double distance = 0;
            double weigthSrt2 = 2 * weight * weight;

            double calculatedEuler = 1.0 / (Math.PI * weigthSrt2);


            for (int filterY = -kernelRadius; filterY <= kernelRadius; filterY++)
            {
                for (int filterX = -kernelRadius; filterX <= kernelRadius; filterX++)
                {
                    distance = ((filterX * filterX) + (filterY * filterY)) / weigthSrt2;

                    Kernel[filterY + kernelRadius, filterX + kernelRadius] = calculatedEuler * Math.Exp(-distance);
                    sumTotal += Kernel[filterY + kernelRadius, filterX + kernelRadius];
                }
            }


            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Kernel[y, x] = Kernel[y, x] * (1.0 / sumTotal);
                }
            }


            return Kernel;
        }
    }

}
