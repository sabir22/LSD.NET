using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;

using LSD.net.bitmap;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
            if (File.Exists(textBox1.Text))
            {
                pictureBox1.Image = new Bitmap(textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBox1.Text))
            {
                Bitmap bmp = new Bitmap(textBox1.Text);
                //Bitmap bmpT = (Bitmap)bmp.GetThumbnailImage(500, (int)((float)bmp.Height / ((float)bmp.Width / 500)), null, IntPtr.Zero); //use this image if tou want to autorotate by text lines
                using (LineSegmentDetector lsd = new LineSegmentDetector(MakeGrayscaleBitmap(bmp)))
                {    
                    lsd.FindLines(0.6);
                    //lsd.FindLines(0.5); //specified scale for image
                    using (Graphics gr = Graphics.FromImage(bmp))
                    {
                        gr.SmoothingMode = SmoothingMode.AntiAlias;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        if (lsd.Lines.Length>0)
                        {
                            foreach (LSDLine ll in lsd.Lines)
                            {
                                LSDLine l = new LSDLine(ll.P1, ll.P2);
                                using (Pen thick_pen = new Pen(Color.Red, 2))
                                {
                                    gr.DrawLine(thick_pen, ll.P1, ll.P2);
                                }
                            }
                        }

                    }
                    
                    LSDLine hl = new LSDLine(new Point(0, 0), new Point(1, 0));
                    var lgroup = lsd.Lines.GroupBy(x=>(int)x.GetExteriorAngleDegree(hl),new simAngleLines());
                    var lgroupG = lgroup.GroupBy(m => m.Key);
                    var ang = (from i in lgroupG orderby i.Sum(x=>x.Count()) descending select i.Key).ElementAtOrDefault(0);//avg angle of lines
                    foreach (var u in lgroup)
                    {
                        Debug.WriteLine(u.Count() + " : " + u.Key );
                    }
                    this.Text = "Lines count = " + lsd.Lines.Length.ToString() + ", avg angle=" + ang.ToString();
                    pictureBox1.Image = bmp; //=RotateImage(bmp,ang); //use this to rotate image by avg lines angle
                }                
            }
        }
        public static Bitmap RotateImage(Bitmap image, float angle)
        {
            // center of the image
            float rotateAtX = image.Width / 2;
            float rotateAtY = image.Height / 2;
            bool bNoClip = false;
            return RotateImage(image, rotateAtX, rotateAtY, angle, bNoClip);
        }

        public static Bitmap RotateImage(Bitmap image, float angle, bool bNoClip)
        {
            // center of the image
            float rotateAtX = image.Width / 2;
            float rotateAtY = image.Height / 2;
            return RotateImage(image, rotateAtX, rotateAtY, angle, bNoClip);
        }

        public static Bitmap RotateImage(Bitmap image, float rotateAtX, float rotateAtY, float angle, bool bNoClip)
        {
            int W, H, X, Y;
            if (bNoClip)
            {
                double dW = (double)image.Width;
                double dH = (double)image.Height;

                double degrees = Math.Abs(angle);
                if (degrees <= 90)
                {
                    double radians = 0.0174532925 * degrees;
                    double dSin = Math.Sin(radians);
                    double dCos = Math.Cos(radians);
                    W = (int)(dH * dSin + dW * dCos);
                    H = (int)(dW * dSin + dH * dCos);
                    X = (W - image.Width) / 2;
                    Y = (H - image.Height) / 2;
                }
                else
                {
                    degrees -= 90;
                    double radians = 0.0174532925 * degrees;
                    double dSin = Math.Sin(radians);
                    double dCos = Math.Cos(radians);
                    W = (int)(dW * dSin + dH * dCos);
                    H = (int)(dH * dSin + dW * dCos);
                    X = (W - image.Width) / 2;
                    Y = (H - image.Height) / 2;
                }
            }
            else
            {
                W = image.Width;
                H = image.Height;
                X = 0;
                Y = 0;
            }

            //create a new empty bitmap to hold rotated image
            Bitmap bmpRet = new Bitmap(W, H);
            bmpRet.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(bmpRet);

            //Put the rotation point in the "center" of the image
            g.TranslateTransform(rotateAtX + X, rotateAtY + Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-rotateAtX - X, -rotateAtY - Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0 + X, 0 + Y));

            return bmpRet;
        }

        public static Bitmap MakeGrayscaleBitmap(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();
            
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            /*Bitmap bmp = new Bitmap(textBox1.Text);
            bmp = MakeGrayscaleBitmap(bmp);
            Color c1 = bmp.GetPixel(e.X, e.Y);
            this.Text = c1.R.ToString() + " " + c1.G.ToString() + " " + c1.B.ToString();*/
        }
    }
    public class simAngleLines: IEqualityComparer<int>
    {
        LSDLine hl = new LSDLine(new Point(0, 0), new Point(1, 0));
        public bool Equals(int x, int y)
        {
            return xy(x,y);
        }
        int a;
        bool xy(int x, int y)
        {
            a = x;
            return (Math.Abs(x - y) < 5);
        }

        public int GetHashCode(int i)
        {
            return a.GetHashCode();
        }

    }
}
