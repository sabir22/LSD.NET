using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
/*LSD.DLL is part of this solution, that contains edited code of "LSD: a Line Segment Detector" by Rafael Grompone von Gioi,
Jeremie Jakubowicz, Jean-Michel Morel, and Gregory Randall,
Image Processing On Line, 2012. DOI:10.5201/ipol.2012.gjmr-lsd
http://dx.doi.org/10.5201/ipol.2012.gjmr-lsd

Copyright (c) 2018 Sabich Gregory <sabir22@mail.ru>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.*/

namespace LSD.net.bitmap
{
    public class LineSegmentDetector : IDisposable
    {
        Bitmap _grayImage;
        List<LSDLine> _lines;
        double[] _lsdImage;
        int _X, _Y;
        public LSDLine[] Lines
        {
            get
            {
                return _lines.ToArray();
            }
        }
        public double[] lsdImage
        {
            get
            {
                return _lsdImage;
            }
        }
        IntPtr ptr;
        public LineSegmentDetector(Bitmap bmp)
        {
            _grayImage = MakeGrayscaleBitmap(bmp);
            _lines = new List<LSDLine>();
            _X = _grayImage.Width;
            _Y = _grayImage.Height;
            _lsdImage = new double[_X * _Y];
            for (int i = 0; i < _X; i++)
            {
                for (int j = 0; j < _Y; j++)
                {
                    _lsdImage[i + j * _X] = (double)_grayImage.GetPixel(i, j).R;
                }
            }

            var myPath = new Uri(typeof(LineSegmentDetector).Assembly.CodeBase).LocalPath;
            var myFolder = Path.GetDirectoryName(myPath);

            var is64 = IntPtr.Size == 8;
            var subfolder = is64 ? "\\x64\\" : "\\x86\\";
            if (!File.Exists(myFolder + subfolder + "\\LSD.dll"))
            {
                if (!File.Exists(myFolder + "\\" + "LSD.dll"))
                {
                    throw new DllNotFoundException("LSD.dll was not found in current directory!");
                }
                else
                {
                    subfolder = "\\";
                }
            }
            ptr = LoadLibrary(myFolder + subfolder + "LSD.dll");
            //SetDllDirectory(myFolder + subfolder);
        }

        private static Bitmap MakeGrayscaleBitmap(Bitmap original)
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

        public void SetNewImage(Bitmap bmp)
        {
            _lines.Clear();
            _grayImage = MakeGrayscaleBitmap(bmp);
            _X = _grayImage.Width;
            _Y = _grayImage.Height;
            _lsdImage = new double[_X * _Y];
            for (int i = 0; i < _X; i++)
            {
                for (int j = 0; j < _Y; j++)
                {
                    _lsdImage[i + j * _X] = (double)_grayImage.GetPixel(i, j).R;
                }
            }
        }

        //----- standart LSD function with 0.8 scale -----
        public void FindLines()
        {
            _lines.Clear();
            int n = 0;
            IntPtr result = lsd(ref n, _lsdImage, _lsdImage.Length, _X, _Y); //LSD function
            if (n > 0)
            {
                int n_out = 7 * n;
                double[] outLSD = new double[n_out];
                Marshal.Copy(result, outLSD, 0, n_out);
                //The seven values are:
                // - x1,y1,x2,y2,width,p,-log10(NFA)            
                for (int i = 0; i < n; i++)
                {
                    int pX1, pX2, pY1, pY2;
                    pX1 = (int)outLSD[7 * i + 0];
                    pY1 = (int)outLSD[7 * i + 1];
                    pX2 = (int)outLSD[7 * i + 2];
                    pY2 = (int)outLSD[7 * i + 3];
                    Point pX = new Point(pX1, pY1);
                    Point pY = new Point(pX2, pY2);
                    LSDLine line = new LSDLine(pX, pY);
                    _lines.Add(line);
                }
            }
        }

        //----- LSD function with user scale -----
        public void FindLines(double scale)
        {
            _lines.Clear();
            int n = 0;
            IntPtr result = lsd_scale(ref n, _lsdImage, _lsdImage.Length, _X, _Y, scale); //LSD function
            if (n > 0)
            {
                int n_out = 7 * n;
                double[] outLSD = new double[n_out];
                Marshal.Copy(result, outLSD, 0, n_out);
                //The seven values are:
                // - x1,y1,x2,y2,width,p,-log10(NFA)            
                for (int i = 0; i < n; i++)
                {
                    int pX1, pX2, pY1, pY2;
                    pX1 = (int)outLSD[7 * i + 0];
                    pY1 = (int)outLSD[7 * i + 1];
                    pX2 = (int)outLSD[7 * i + 2];
                    pY2 = (int)outLSD[7 * i + 3];
                    Point pX = new Point(pX1, pY1);
                    Point pY = new Point(pX2, pY2);
                    LSDLine line = new LSDLine(pX, pY);
                    _lines.Add(line);
                }
            }
        }

        public void Dispose()
        {
            _grayImage.Dispose();
            _lines.Clear();
            _lsdImage = null;
            FreeLibrary(ptr);
            System.GC.Collect();
        }
        [DllImport("LSD.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        private static extern IntPtr lsd(ref int n_out, double[] img, int imgSize, int X, int Y);

        [DllImport("LSD.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        private static extern IntPtr lsd_scale(ref int n_out, double[] img, int imgSize, int X, int Y, double scale);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);


    }
}
