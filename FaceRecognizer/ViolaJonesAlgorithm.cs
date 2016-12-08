using System.Drawing;

namespace FaceRecognizer
{
    public static class ViolaJonesAlgorithm
    {
        public static double[,] GetIntegralRepresentation(Bitmap sourceImage)
        {
            var matrix = new double[sourceImage.Height, sourceImage.Width];
            for (int i = 0; i < sourceImage.Height; i++)
                for (int j = 0; j < sourceImage.Width; j++)
                {
                    matrix[i, j] = GetBrightness(sourceImage, j, i);
                    if (i > 0)
                    {
                        matrix[i, j] = matrix[i, j] + matrix[i - 1, j];

                        if (j > 0)
                        {
                            matrix[i, j] = matrix[i, j] - matrix[i - 1, j - 1];
                        }
                    }
                    if (j > 0)
                    {
                        matrix[i, j] = matrix[i, j] + matrix[i, j - 1];
                    }
                }
            return matrix;
        }

        public static double GetBrightness(Bitmap image, int x, int y)
        {
            return image.GetPixel(x, y).GetBrightness();
        }
    }
}
