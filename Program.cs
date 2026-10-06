using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace A25;

class MyWindow : Window {
   List<(int, int)> Points = [];
   public MyWindow () {
      Width = 800; Height = 600;
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
      //DrawMandelbrot (-0.5, 0, 1);
   }

   void CollectPoint (object sender, MouseButtonEventArgs e) {
      var pt = e.GetPosition (this);
      Points.Add (((int)pt.X, (int)pt.Y));
      if (Points.Count == 1) return;
      DrawLine ();
      Points.Clear ();
   }

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
      int dxw = mBmp.PixelWidth, dyh = mBmp.PixelHeight;
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = x0; x <= x1; x++) {
            if (steep) SetPixel (y, x, 255); else SetPixel (x, y, 255);
            if (p >= 0) { y += step; p -= 2 * dx; }
            p += 2 * dy;
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dxw, dyh));
      } finally {
         mBmp.Unlock ();
      }
   }

   void DrawLineNaive () {
      var (x0, y0) = Points[0];
      var (x1, y1) = Points[1];
      var (dx, dy) = (x1 - x0, y1 - y0);
      double n = Math.Max (Math.Abs (dx), Math.Abs (dy));
      int dxw = mBmp.PixelWidth, dyh = mBmp.PixelHeight;
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         if (n == 0) SetPixel (x0, y0, 255);
         else {
            var (xs, ys) = (dx / (double)n, dy / (double)n);
            for (int i = 0; i <= n; i++)
               SetPixel ((int)(x0 + (xs * i)), (int)(y0 + (ys * i)), 255);
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dxw, dyh));
      } finally {
         mBmp.Unlock ();
      }
   }

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

   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 32; i++) {
         if (z.NormSq > 4) return (byte)(i * 8);
         z = z * z + c;
      }
      return 0;
   }

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

   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }

   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}
