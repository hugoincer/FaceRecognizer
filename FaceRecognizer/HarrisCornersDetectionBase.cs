using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HarrisCornersDetector
{
    class HarrisCornersDetectionBase
    {
        
        protected byte[] rgb;                 

        protected int img_width;              
        protected int img_height;             



        protected int[] corners;                        
        protected double k;                             
        protected int corner_treshold;
        protected int nm_supress_window_size;

        
        protected int dxdy_kernel_window_size = 3; 
        protected int[] Dx = new int[9] { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
        protected int[] Dy = new int[9] { -1, -2, -1, 0, 0, 0, 1, 2, 1 };

        protected int gaussian_kernel_window_size = 5; 
        protected double[] GaussianKernel = new double[25] {
            ((double)1 / (double)84), ((double)2 / (double)84), ((double)3 / (double)84), ((double)2 / (double)84), ((double)1 / (double)84),
            ((double)2 / (double)84), ((double)5 / (double)84), ((double)6 / (double)84), ((double)5 / (double)84), ((double)2 / (double)84),
            ((double)3 / (double)84), ((double)6 / (double)84), ((double)8 / (double)84), ((double)6 / (double)84), ((double)3 / (double)84),
            ((double)2 / (double)84), ((double)5 / (double)84), ((double)6 / (double)84), ((double)5 / (double)84), ((double)2 / (double)84),
            ((double)1 / (double)84), ((double)2 / (double)84), ((double)3 / (double)84), ((double)2 / (double)84), ((double)1 / (double)84)
        };




        
        public HarrisCornersDetectionBase(Bitmap bmp, double k, int corner_treshold, double edge_treshold, int nm_supress_window_size)
        {
            this.rgb = getRGB(bmp);  //get rgb bits;
            this.img_width = bmp.Width;
            this.img_height = bmp.Height;
            this.corners = new int[img_width * img_height];
            this.k = k;
            this.corner_treshold = corner_treshold;
            this.nm_supress_window_size = nm_supress_window_size;
        }


        public int[] GetCorners()
        {
            return corners;
        }

        
        protected byte[] getRGB(Bitmap bmp)
        {

            
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            //image matrix is structured as an array in which are concatenated all image ROWS; 
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);
            return rgbValues;

        }

        public Bitmap WriteResultImage(Bitmap original, Point startPoint)
        {
            var bmp = new Bitmap(original);
            var g = Graphics.FromImage(bmp);
            var pen = new Pen(Color.Red);
            int t = 0;
            
            for (int i = 2 * img_width + 2; i < img_width * img_height - 2 * img_width - 2; i++)
            {   
                if ((byte)corners[i] != 0)
                {
                    t++;
                    int x = i % img_width;
                    int y = i / img_width;
                    g.DrawRectangle(pen, startPoint.X + x - 1, startPoint.Y + y - 1, 3, 3);
            

                }
            }

            
            return bmp;

        }


        
        protected int MSpatialConvolution(int[] data1, int[] data2, int k, double[] kernel_filter, int kernelSize)
        {
            int output = 0;
            int l = 0;
            for (int i = -(kernelSize / 2); i <= (kernelSize / 2); i++)
            {
                for (int j = -(kernelSize / 2); j <= (kernelSize / 2); j++)
                {
                    output += (int)(data1[k + i * img_width + j] * data2[k + i * img_width + j] * kernel_filter[l]);
                    l++;
                }
            }

            return output;

        }

        //--------------------------------------------------------------------------------------------
        //Methood: int GradSpatialConvolution(int[] data, int k, int[] kernel_filter, int kernelSize)
        //
        //Description:
        //  - spatial convolution for 1 matrix element for gradient construction 
        //  - build spatial convolution of data[x]
        //-------------------------------------------------------------------------------------------
        protected int GradSpatialConvolution(int[] data, int k, int[] kernel_filter, int kernelSize)
        {
            int output = 0;
            int l = 0;
            for (int i = -(kernelSize / 2); i <= (kernelSize / 2); i++)
            {
                for (int j = -(kernelSize / 2); j <= (kernelSize / 2); j++)
                {
                    output += data[k + i * img_width + j] * kernel_filter[l];
                    l++;
                }
            }

            return output;

        }





    }
}
