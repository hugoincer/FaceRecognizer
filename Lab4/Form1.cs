using Lab4.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace Lab4
{
    public partial class Form1 : Form
    {
        Hopfield network = null;
        OpenFileDialog ofd;

        public Form1()
        {
            InitializeComponent();
            ofd = new OpenFileDialog();
            ofd.Filter = "(*.jpg)|*.jpg|(*.png)|*.png";
            //ofd.InitialDirectory = @"C:\";
            ofd.RestoreDirectory = true;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private Bitmap ModifyImage(Bitmap image)
        {
            var modifier = new ImageModifier();
            var windows = modifier.DetectFaces(image);
            if (windows.Length < 1)
            {
                MessageBox.Show("Can't detect a face on the image");
                return null;
            }
            var wins = new List<Rectangle>();

            long pictureSize = image.Height * image.Width;

            Graphics g1 = Graphics.FromImage(image);
            foreach (var r in windows)
            {
                if (r.Width * r.Height < 0.01 * pictureSize)
                    continue;
                wins.Add(r);
            }
            //pictureBox1.Image = image;

            if (wins.Count < 1)
            {
                MessageBox.Show("Can't detect a face on the image");
                return null;
            }

            var face = wins.OrderByDescending(x => x.Height * x.Width).First();
            
            var result = image.Clone(face, image.PixelFormat);
            var scaled = ResizeImage(result, 50, 50);
            return scaled;

            //var tmp =
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "(*.jpg)|*.jpg|(*.png)|*.png";
            ofd.InitialDirectory = @"C:\";
            ofd.RestoreDirectory = true;
            string path = string.Empty;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            path = ofd.FileName;

            var image = new Bitmap(path);

            var output = ModifyImage(image);
            if (output == null)
                return;
            //return;

            if (network == null)
            {
                network = new Hopfield(output.Height * output.Width);
            }

            int[] inputVector = network.ConvertImageInInputVector(output);
            network.AddImage(inputVector, textBox1.Text);
            network.LearningOfNetwork(inputVector);

            pictureBox1.Image = image;
            MessageBox.Show("Обучение завершено");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path = string.Empty;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            label3.Text = "";

            path = ofd.FileName;

            var image = new Bitmap(path);

            var output = ModifyImage(image);

            if (output == null)
                return;
            int[] inputVector = network.ConvertImageInInputVector(output);
            string className = network.ClassifedInputVector(inputVector);

            label3.Text = (className == null) ? "Неопределено" : "Класс: " + className;

            pictureBox2.Image = image;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            network = null;
            MessageBox.Show("Сеть удалена");
        }
    }
}
