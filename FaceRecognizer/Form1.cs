using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

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
            ofd.Filter = "(*.png)|*.png|(*.jpg)|*.jpg";
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
            if (_sourceImage == null)
            {
                MessageBox.Show("Source image is not set!");
                return;
            }

            if (radioButton1.Checked)
            {
                pictureBox2.Image = new Median(Convert.ToInt32(textBox1.Text)).filter(_sourceImage);
            }
            else if (radioButton2.Checked)
            {
                pictureBox2.Image = new Gaussian(Convert.ToInt32(textBox1.Text), Convert.ToDouble(textBox2.Text)).filter(_sourceImage);
            }
            else if (radioButton3.Checked)
            {
                pictureBox2.Image = new Roberts().filter(_sourceImage);
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

        
    }
}
