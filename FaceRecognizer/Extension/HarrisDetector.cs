using System;
using System.Collections.Generic;
using System.Drawing;

namespace FaceRecognizer.Extension
{
    class HarrisDetector
    {
        public static Pixel ToGray(Pixel p)
        {
            var gray = (byte)(p.Blue * 0.2125f + p.Green * 0.7154f + p.Red * 0.0721f);
            p.Red = gray;
            p.Blue = gray;
            p.Green = gray;
            return p;
        }



        public static CustomImage Apply(CustomImage img)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    img[i, j] = ToGray(img[i, j]);
                }
            }
            return img;
        }

        public unsafe List<Point> ProcessImage(Bitmap image, float k, float threshold)
        {
            var bmp = new CustomImage(image);
            var grayImage = Apply(bmp).GetImage();
            // check image format

            // make sure we have grayscale image
            var srcimg = grayImage.LockBits(new Rectangle(0, 0, grayImage.Width, grayImage.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            // Get the address of the first line.

            IntPtr ptr = srcimg.Scan0;


            // create temporary grayscale image



            // get source image size
            int width = image.Width;
            int height = image.Height;
            int srcStride = srcimg.Stride;
            int srcOffset = srcStride - width;


            // 1. Calculate partial differences
            float[,] diffx = new float[height, width];
            float[,] diffy = new float[height, width];
            float[,] diffxy = new float[height, width];


            fixed (float* pdx = diffx, pdy = diffy, pdxy = diffxy)
            {
                byte* src = (byte*)ptr.ToPointer() + srcStride + 1;

                // Skip first row and first column
                float* dx = pdx + width + 1;
                float* dy = pdy + width + 1;
                float* dxy = pdxy + width + 1;

                // for each line
                for (int y = 1; y < height - 1; y++)
                {
                    // for each pixel
                    for (int x = 1; x < width - 1; x++, src++, dx++, dy++, dxy++)
                    {
                        // Convolution with horizontal differentiation kernel mask
                        float h = ((src[-srcStride + 1] + src[+1] + src[srcStride + 1]) -
                                   (src[-srcStride - 1] + src[-1] + src[srcStride - 1])) * 0.166666667f;

                        // Convolution vertical differentiation kernel mask
                        float v = ((src[+srcStride - 1] + src[+srcStride] + src[+srcStride + 1]) -
                                   (src[-srcStride - 1] + src[-srcStride] + src[-srcStride + 1])) * 0.166666667f;

                        // Store squared differences directly
                        *dx = h * h;
                        *dy = v * v;
                        *dxy = h * v;
                    }

                    // Skip last column
                    dx++; dy++; dxy++;
                    src += srcOffset + 1;
                }

                // Free some resources which wont be needed anymore

            }


            // 2. Smooth the diff images
            
                float[,] temp = new float[height, width];

                // Convolve with Gaussian kernel
            /*    convolve(diffx, temp, kernel);
                convolve(diffy, temp, kernel);
                convolve(diffxy, temp, kernel);
            */


            // 3. Compute Harris Corner Response Map
            float[,] map = new float[height, width];

            fixed (float* pdx = diffx, pdy = diffy, pdxy = diffxy, pmap = map)
            {
                float* dx = pdx;
                float* dy = pdy;
                float* dxy = pdxy;
                float* H = pmap;
                float M, A, B, C;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, dx++, dy++, dxy++, H++)
                    {
                        A = *dx;
                        B = *dy;
                        C = *dxy;


                        // Original Harris corner measure
                        M = (A * B - C * C) - (k * ((A + B) * (A + B)));


                        if (M > threshold)
                        {
                            *H = M; // insert value in the map
                        }
                    }
                }
            }


            // 4. Suppress non-maximum points
            List<Point> cornersList = new List<Point>();

            // for each row
            for (int y = 1, maxY = height - 1; y < maxY; y++)
            {
                // for each pixel
                for (int x = 1, maxX = width - 1; x < maxX; x++)
                {
                    float currentValue = map[y, x];

                    // for each windows' row
                    for (int i = -1; (currentValue != 0) && (i <= 1); i++)
                    {
                        // for each windows' pixel
                        for (int j = -1; j <= 1; j++)
                        {
                            if (map[y + i, x + j] > currentValue)
                            {
                                currentValue = 0;
                                break;
                            }
                        }
                    }

                    // check if this point is really interesting
                    if (currentValue != 0)
                    {
                        cornersList.Add(new Point(x, y));
                    }
                }
            }


            return cornersList;
        }

    }
}
