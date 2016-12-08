using System;
using System.Drawing;
using System.Linq;

namespace FaceRecognizer
{
    public static class HarrisDetector
    {
        private const int AreaMaxIndex = 5;
        private const int AreaMinIndex = AreaMaxIndex - 1;

        private const double Sigma = 1.4f;
        private static double k1 = 1.0 / (Math.Sqrt(2 * Math.PI) * Sigma);
        private static double k2 = -1.0 / (2 * Sigma * Sigma);

        public static Image GetMarks(Image img)
        {
            var bmp = new Bitmap(img);
            var xDiff = GetDiff(bmp, x => x + 1, y => y);
            var yDiff = GetDiff(bmp, x => x, y => y + 1);
            var harris = GetMarks(xDiff, yDiff);
            var filtered = Filter(harris);
            return FilteredImage(filtered, bmp);
        }

        private static int[,] GetDiff(Bitmap image, Func<int, int> xchange, Func<int, int> ychange)
        {
            var result = new int[image.Height - 1, image.Width - 1];
            for (int i = 0; i < image.Height - 1; i++)
            {
                for (int j = 0; j < image.Width - 1; j++)
                {
                    result[i, j] = Math.Abs(GetPixelColor(image, xchange(j), ychange(i)) - GetPixelColor(image, j, i));
                }
            }
            return result;
        }

        private static int GetPixelColor(Bitmap img, int x, int y)
        {
            Color color = img.GetPixel(x, y);
            return (color.R + color.G + color.B) / 3;
        }


        public static double[,] GetMarks(int[,] xDiff, int[,] yDiff, double k = 0.05)
        {
            var result = new double[xDiff.GetLength(0), xDiff.GetLength(1)];

            for (int i = AreaMinIndex; i < result.GetLength(0) - AreaMinIndex; i++)
            {
                for (int j = AreaMinIndex; j < result.GetLength(1) - AreaMinIndex; j++)
                {
                    result[i, j] = GetSquareSum(xDiff, i, j) * GetSquareSum(yDiff, i, j) -
                                    GetSquarePareSum(xDiff, yDiff, i, j) -
                                    k * (GetSquareSum(xDiff, i, j) + GetSquareSum(yDiff, i, j));

                    // result[i, j] = result[i, j] > 0 ? result[i, j] : 0;
                }
            }
            return result;
        }

        private static double GetSquareSum(int[,] xDiff, int i, int j)
        {
            double result = 0;
            for (int k = -AreaMinIndex; k < AreaMaxIndex; k++)
            {
                for (int l = -AreaMinIndex; l < AreaMaxIndex; l++)
                {
                    result += xDiff[i + k, j + l] * xDiff[i + k, j + l] * k1 * Math.Exp((k * k + l * l) * k2);
                }
            }
            return result;
        }


        private static int GetSquarePareSum(int[,] xDiff, int[,] yDiff, int i, int j)
        {
            int result = 0;
            for (int k = -AreaMinIndex; k < AreaMaxIndex; k++)
            {
                for (int l = -AreaMinIndex; l < AreaMaxIndex; l++)
                {
                    result += xDiff[i + k, j + l] * xDiff[i + k, j + l] * yDiff[i + k, j + l] * yDiff[i + k, j + l];
                }
            }
            return result;
        }

        public static bool[,] Filter(double[,] data)
        {
            var result = new bool[data.GetLength(0), data.GetLength(1)];
            var maxValue = data.Cast<double>().Max();
            var minValue = data.Cast<double>().Min();
            var step = maxValue / 1000;

            for (int i = AreaMinIndex; i < data.GetLength(0) - AreaMinIndex; i += AreaMinIndex * 2 + 1)
            {
                for (int j = AreaMinIndex; j < data.GetLength(1) - AreaMinIndex; j += AreaMinIndex * 2 + 1)
                {
                    var minimum = GetAreaMinimum(data, i, j);
                    if (minimum.Item3 > step)
                    {
                        result[i + minimum.Item1, j + minimum.Item2] = true;
                    }
                }
            }
            return result;
        }

        private static Tuple<int, int, double> GetAreaMinimum(double[,] data, int i, int j)
        {
            int resultK = -AreaMinIndex;
            int resultL = -AreaMinIndex;
            double minimum = data[i + resultK, j + resultL];

            for (int k = -AreaMinIndex; k < AreaMaxIndex; k++)
            {
                for (int l = -AreaMinIndex; l < AreaMaxIndex; l++)
                {
                    if (data[i + k, j + l] > minimum)
                    {
                        minimum = data[i + k, j + l];
                        resultK = k;
                        resultL = l;
                    }
                }
            }
            return new Tuple<int, int, double>(resultK, resultL, minimum);
        }

        public static Bitmap FilteredImage(bool[,] data, Bitmap origin)
        {
            var result = origin.Clone(new Rectangle(0, 0, origin.Width, origin.Height), origin.PixelFormat);
            var gr = Graphics.FromImage(result);
            var solidBrush = new SolidBrush(Color.Red);

            for (int i = AreaMinIndex; i < data.GetLength(0) - AreaMinIndex; i++)
            {
                for (int j = AreaMinIndex; j < data.GetLength(1) - AreaMinIndex; j++)
                {
                    if (data[i, j])
                    {

                        gr.FillEllipse(solidBrush, j - 3, i - 3, 7, 7);
                    }
                }
            }
            gr.Dispose();
            solidBrush.Dispose();
            return result;
        }
    }
}
