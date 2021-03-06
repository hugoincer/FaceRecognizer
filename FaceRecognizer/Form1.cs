﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace FaceRecognizer
{
    public partial class Form1 : Form
    {
        Bitmap _sourceImage = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void SetImage(string path)
        {
            try
            {
                _sourceImage = new Bitmap(path);
                pictureBox1.Image = _sourceImage;
                pictureBox2.Image = new Bitmap(_sourceImage.Width, _sourceImage.Height);
            }
            catch (Exception e)
            {
                _sourceImage = null;
                MessageBox.Show("Unexpected error occured while opening source image( " + e.Message + " )");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "(*.jpg)|*.jpg|(*.png)|*.png";
            ofd.InitialDirectory = @"C:\";
            ofd.RestoreDirectory = true;
            string path = string.Empty;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                path = ofd.FileName;
            }

            if (!string.IsNullOrEmpty(path))
            {
                SetImage(path);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog svDialog = new SaveFileDialog();

            svDialog.Filter = "(*.png)|*.png|(*.jpg)|*.jpg";
            svDialog.FilterIndex = 2;
            svDialog.RestoreDirectory = true;

            if (svDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(svDialog.FileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Source image is not set!");
                return;
            }

            if (radioButton1.Checked)
            {
                pictureBox2.Image = new Median(Convert.ToInt32(textBox1.Text)).filter(new Bitmap(pictureBox1.Image));
            }
            else if (radioButton2.Checked)
            {
                pictureBox2.Image = new Gaussian(Convert.ToInt32(textBox1.Text), Convert.ToDouble(textBox2.Text)).filter(new Bitmap(pictureBox1.Image));
            }
            else if (radioButton3.Checked)
            {
                pictureBox2.Image = new Roberts().filter(new Bitmap(pictureBox1.Image));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.Height = 200;
            pictureBox1.Width = 200;
            pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
            if (radioButton1.Checked)
            {
                label1.Visible = true;
                textBox1.Visible = true;
                label2.Visible = false;
                textBox2.Visible = false;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = true;
            textBox1.Visible = true;
            label2.Visible = false;
            textBox2.Visible = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = true;
            textBox1.Visible = true;
            label2.Visible = true;
            textBox2.Visible = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = false;
            textBox1.Visible = false;
            label2.Visible = false;
            textBox2.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pictureBox2.Image);
        }

        private List<Rectangle> GetPossibleFaces()
        {
            var result = new List<Rectangle>();            
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var img = new Gaussian(11, 5).filter(new Bitmap(pictureBox1.Image));
                img = new Median(5).filter(img);
                var image = new CustomImage(img);
                Window.WindowXstep = image.Width / 100;
                Window.WindowYstep = image.Height / 100;
                Window.WindowHeight = image.Height / 25;
                Window.WindowWidth = image.Width / 25;

                var classifier = Classifier.LoadFromFile(ofd.FileName);
                var detector = new Detector(image, classifier);
                foreach (var item in detector.Detect())
                {
                    if (item.Width > 5 && item.Height > 5)
                    {
                        result.Add(item.ToRectangle());
                    }
                }                
            }

            return result;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //var img = new Gaussian(Convert.ToInt32(textBox1.Text), Convert.ToDouble(textBox2.Text)).filter(new Bitmap(pictureBox1.Image));
            //var img = new Median(Convert.ToInt32(textBox1.Text)).filter(new Bitmap(pictureBox1.Image));
            //img = new Roberts().filter(img);

            // var img = new Bitmap(pictureBox1.Image);

            //var image = new CustomImage(new Bitmap(pictureBox1.Image));
            var r = GetPossibleFaces();
            if (r.Count == 0)
            {
                MessageBox.Show("Couldn't find face");
                return;
            }

            var normalImage = new Bitmap(pictureBox1.Image);
            var g = Graphics.FromImage(normalImage);
            var p = new Pen(Color.Red);
            foreach (var rect in r)
                g.DrawRectangle(p, rect);
            pictureBox2.Image = normalImage;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //pictureBox2.Image = HarrisDetector.GetMarks(pictureBox1.Image);
            /*var h = new Extension.HarrisDetector();
            var m = h.ProcessImage(new Bitmap(pictureBox1.Image), 0.05f, 1);
            MessageBox.Show(m.Count.ToString());*/
            var r = GetPossibleFaces();
            if (r.Count == 0)
            {
                MessageBox.Show("Couldn't find face");
                return;
            }

            var biggest = r.OrderByDescending(x => x.Width * x.Height).First();

            //var img = new Bitmap(_sourceImage, biggest);
            var img = _sourceImage.Clone(biggest, _sourceImage.PixelFormat);
            //pictureBox2.Image = img;

            //return;

            var constant_k = 0.05;
            var treshold = 4500000;
            var harris = new HarrisCornersDetector.HarrisCornersDetectionSerial
                (img, constant_k, treshold, 0, 11);
            harris.run_algorithm();
            pictureBox2.Image = harris.WriteResultImage(_sourceImage, new Point(biggest.X,biggest.Y));

        }
    }
}
