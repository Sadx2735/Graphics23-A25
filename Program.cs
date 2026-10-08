// ------------------------------------------------------------------------------------------------
// Training ~ A training program for new joinees at Metamation, Batch - July 2026.
// Copyright (c) TRUMPF Metamation India.
// ------------------------------------------------------------------------------------------------
// MyWindow.cs
// WPF window with a Gray8 WriteableBitmap for line drawing algorithm.
// ------------------------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;

namespace A25;

#region Class MyWindow ----------------------------------------------------------------------------
/// <summary>Window that renders pixel graphics into a WriteableBitmap</summary>
class MyWindow : Window {
   #region Constructors ---------------------------------------------
   /// <summary>Creates the window, the bitmap-backed image and hooks up mouse input</summary>
   public MyWindow () {
      Width = 800; Height = 800;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      Image image = new Image () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);
      mBmp = new WriteableBitmap ((int)Width, (int)Height,
         96, 96, PixelFormats.Gray8, null);
      mStride = mBmp.BackBufferStride;
      image.Source = mBmp;
      Content = image;
      this.MouseDown += CollectPoint;
   }
   #endregion

   #region Implementation -------------------------------------------
   // Collects mouse clicks, and draws a line once two points are available
   void CollectPoint (object sender, MouseButtonEventArgs e) {
      var pt = e.GetPosition (this);
      Points.Add (((int)pt.X, (int)pt.Y));
      if (Points.Count == 1) return;
      DrawLine ();
      Points.Clear ();
   }

   // Fills a 256 x 256 square with a horizontal gray gradient
   void DrawGraySquare () {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = 0; x <= 255; x++) {
            for (int y = 0; y <= 255; y++) {
               SetPixel (x, y, (byte)x);
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, 256, 256));
      } finally {
         mBmp.Unlock ();
      }
   }

   // Draws a line between the two collected points using Bresenham's algorithm
   // Reference : ../reference/Algorithm.png
   void DrawLine () {
      var (x0, y0) = Points[0];
      var (x1, y1) = Points[1];
      bool steep = Math.Abs (y1 - y0) > Math.Abs (x1 - x0);
      if (steep) (x0, y0, x1, y1) = (y0, x0, y1, x1);
      if (x1 < x0) (x0, y0, x1, y1) = (x1, y1, x0, y0);
      var (dx, dy) = (x1 - x0, Math.Abs (y1 - y0));
      int step = y0 < y1 ? 1 : -1;
      int p = 2 * dy - dx, y = y0;
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = x0; x <= x1; x++) {
            if (steep) SetPixel (y, x, 255); else SetPixel (x, y, 255);
            if (p >= 0) { y += step; p -= 2 * dx; }
            p += 2 * dy;
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, mBmp.PixelWidth, mBmp.PixelHeight));
      } finally {
         mBmp.Unlock ();
      }
   }

   // Draws a line between the two collected points by stepping along the longer axis
   void DrawLineNaive () {
      var (x0, y0) = Points[0];
      var (x1, y1) = Points[1];
      var (dx, dy) = (x1 - x0, y1 - y0);
      double n = Math.Max (Math.Abs (dx), Math.Abs (dy));
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         if (n == 0) SetPixel (x0, y0, 255);
         else {
            var (xs, ys) = (dx / (double)n, dy / (double)n);
            for (int i = 0; i <= n; i++)
               SetPixel ((int)(x0 + (xs * i)), (int)(y0 + (ys * i)), 255);
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, mBmp.PixelWidth, mBmp.PixelHeight));
      } finally {
         mBmp.Unlock ();
      }
   }

   // Renders the Mandelbrot set centered at (xc, yc) with the given zoom
   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int dx = mBmp.PixelWidth, dy = mBmp.PixelHeight;
         double step = 2.0 / dy / zoom;
         double x1 = xc - step * dx / 2, y1 = yc + step * dy / 2;
         for (int x = 0; x < dx; x++) {
            for (int y = 0; y < dy; y++) {
               Complex c = new Complex (x1 + x * step, y1 - y * step);
               SetPixel (x, y, Escape (c));
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dx, dy));
      } finally {
         mBmp.Unlock ();
      }
   }

   // Returns a gray level based on how quickly the point c escapes (0 if it never does)
   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 32; i++) {
         if (z.NormSq > 4) return (byte)(i * 8);
         z = z * z + c;
      }
      return 0;
   }

   // Paints a pixel under the mouse while the left button is pressed
   void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
         try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            var pt = e.GetPosition (this);
            int x = (int)pt.X, y = (int)pt.Y;
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         } finally {
            mBmp.Unlock ();
         }
      }
   }

   // Writes a gray value into the back buffer at (x, y)
   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }
   #endregion

   #region Fields ---------------------------------------------------
   List<(int, int)> Points = [];
   ConcurrentQueue<(int, int)> mQueue = [];
   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
   #endregion
}
#endregion

#region Class Program -----------------------------------------------------------------------------
/// <summary>Application entry point</summary>
internal class Program {
   [STAThread]
   static void Main (string[] args) {
      var w = new MyWindow ();
      w.Show ();
      Application app = new ();
      app.Run ();
   }
}
#endregion