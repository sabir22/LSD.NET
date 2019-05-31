# LSD.NET
Line segment detector wrapper for .net (c#)
This project is based on: "LSD: a Line Segment Detector" by Rafael Grompone von Gioi,
Jeremie Jakubowicz, Jean-Michel Morel, and Gregory Randall,
Image Processing On Line, 2012. DOI:10.5201/ipol.2012.gjmr-lsd
http://dx.doi.org/10.5201/ipol.2012.gjmr-lsd
Copyright (c) 2007-2011 rafael grompone von gioi <grompone@gmail.com>

Note that C++ project builds from different versions of Visual Studio may cause a problem with compatibility with some Windows versions. You need to download proper MVC libraries to make it work properly.

Solution have an example that work with emgu cv, so please check their license before using with your solution: http://www.emgu.com/wiki/index.php/Licensing:
Otherwise you can use lsd.net.bitmap to work with system.drawing objects and LineSegment2D analog (LSDLine).

![Screenshot](example%20lines.png)
