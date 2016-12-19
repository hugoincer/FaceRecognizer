using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace HarrisCornersDetector
{
    class HarrisCornersDetectionSerial : HarrisCornersDetectionBase
    {
        protected int[] gray;                

        protected int[] grad_x;               
        protected int[] grad_y;               


        protected int[] Mx;
        protected int[] My;
        protected int[] Mxy;


        public HarrisCornersDetectionSerial(Bitmap bmp, double k, int corner_treshold, int edge_treshold, int nm_supress_window_size)
            : base(bmp, k, corner_treshold, edge_treshold, nm_supress_window_size)
        {
            this.gray = new int[img_width * img_height];
            this.grad_x = new int[img_width * img_height];
            this.grad_y = new int[img_width * img_height];
            this.Mx = new int[img_width * img_height];
            this.My = new int[img_width * img_height];
            this.Mxy = new int[img_width * img_height];

        }

        
        public void run_algorithm()
        {
            RGB2GrayScale();            
            BuildGradient();            
            BuildOrder2MomentMatrix();  
            BuildHarrisResponse();      
            ApplyNONMaxSupression();    
        }
        
        private void RGB2GrayScale()
        {

            for (int i = 0; i < img_width * img_height; i++)
            {
                gray[i] = (30 * rgb[i * 3] + 59 * rgb[i * 3 + 1] + 11 * rgb[i * 3 + 2]) / 100;
            }
        }
        private void BuildGradient()
        {
            int line_coord = 0;

            for (int i = img_width + 1; i < (img_height * img_width) - img_width - 1; i++)
            {
                //avoid to exceed image left and right margins and loss of information
                line_coord = i % img_width;
                if (line_coord <= img_width - 1 && line_coord >= 1)
                {
                    grad_x[i] = GradSpatialConvolution(gray, i, Dx, 3);
                    grad_y[i] = GradSpatialConvolution(gray, i, Dy, 3);
                }

            }
        }
        private void BuildOrder2MomentMatrix()
        {
            int line_coord = 0;
            for (int i = (2 + 1) * img_width + 2; i < (img_height * img_width) - (2 + 1) * img_width - 2; i++)
            {
                line_coord = i % img_width;
                if (line_coord <= img_width - 2 - 1 && line_coord >= 2 + 1)
                {
                    Mx[i] = MSpatialConvolution(grad_x, grad_x, i, GaussianKernel, 5);
                    My[i] = MSpatialConvolution(grad_y, grad_y, i, GaussianKernel, 5);
                    Mxy[i] = MSpatialConvolution(grad_x, grad_y, i, GaussianKernel, 5);
                }
            }
        }

        private void BuildHarrisResponse()
        {
            for (int i = 0; i < img_width * img_height; i++)
            {
                corners[i] = (int)((double)(Mx[i] * My[i]) - (Mxy[i] * Mxy[i]) - (double)k * ((double)(Mx[i] + My[i]) * (Mx[i] + My[i])));

                if (corners[i] < corner_treshold)
                {
                    corners[i] = 0;

                }

            }
        }

        private void ApplyNONMaxSupression()
        {
            int line_coord = 0;

            for (int i = (nm_supress_window_size / 2 + 1) * img_width + (nm_supress_window_size / 2);
                     i < (img_height * img_width) - (nm_supress_window_size / 2 + 1) * img_width - (nm_supress_window_size / 2);
                     i++)
            {
                line_coord = i % img_width;
                if (line_coord <= img_width - (nm_supress_window_size / 2 + 1) && line_coord >= (nm_supress_window_size / 2 + 1))
                {

                    if (corners[i] != 0)
                    {
                        for (int kk = -(nm_supress_window_size / 2); kk <= (nm_supress_window_size / 2); kk++)
                        {
                            for (int ll = -(nm_supress_window_size / 2); ll <= (nm_supress_window_size / 2); ll++)
                            {
                                if (corners[i + kk * img_width + ll] > corners[i])
                                {
                                    corners[i] = 0;
                                    break;
                                }
                            }
                        }

                    }
                }
            }
        }



    }
}
