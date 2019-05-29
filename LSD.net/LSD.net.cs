using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
/*LSD.DLL is part of this solution, that contains edited code of "LSD: a Line Segment Detector" by Rafael Grompone von Gioi,
Jeremie Jakubowicz, Jean-Michel Morel, and Gregory Randall,
Image Processing On Line, 2012. DOI:10.5201/ipol.2012.gjmr-lsd
http://dx.doi.org/10.5201/ipol.2012.gjmr-lsd

Emgu CV is a cross platform .Net wrapper to the OpenCV image processing library http://www.emgu.com (C)
EMGU is an avid supporter of open source software. This is the appropriate option if you are creating an open source application with a license compatible with the GNU GPL license v3. https://www.gnu.org/licenses/gpl-3.0.txt
To get latest version of software and add to references http://www.emgu.com/wiki/index.php/Download_And_Installation
Licensing information: http://www.emgu.com/wiki/index.php/Licensing:

Copyright (c) 2018 Sabich Gregory <sabir22@mail.ru>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.*/

namespace LSD.net
{
    public class LineSegmentDetector : IDisposable
    {
        Image<Gray, Byte> _grayImage;
        List<LineSegment2D> _lines;
        double[] _lsdImage;
        int _X, _Y;
        public LineSegment2D[] Lines
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
        public LineSegmentDetector(Image<Gray, Byte> grayImage)
        {
            _lines = new List<LineSegment2D>();
            _grayImage = grayImage;
            _X = _grayImage.Width;
            _Y = _grayImage.Height;
            _lsdImage = new double[_X * _Y];
            for (int i = 0; i < _X; i++)
            {
                for (int j = 0; j < _Y; j++)
                {
                    _lsdImage[i + j * _X] = _grayImage[j, i].Intensity;
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

        public void SetNewImage(Image<Gray, Byte> grayImage)
        {
            _lines.Clear();
            _grayImage = grayImage;
            _X = _grayImage.Width;
            _Y = _grayImage.Height;
            _lsdImage = new double[_X * _Y];
            for (int i = 0; i < _X; i++)
            {
                for (int j = 0; j < _Y; j++)
                {
                    _lsdImage[i + j * _X] = _grayImage[j, i].Intensity;
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
                    LineSegment2D line = new LineSegment2D(pX, pY);
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
                    LineSegment2D line = new LineSegment2D(pX, pY);
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
