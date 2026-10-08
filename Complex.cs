// ------------------------------------------------------------------------------------------------
// Training ~ A training program for new joinees at Metamation, Batch - July 2026.
// Copyright (c) TRUMPF Metamation India.
// ------------------------------------------------------------------------------------------------
// Complex.cs
// Implements a complex number struct with basic arithmetic.
// ------------------------------------------------------------------------------------------------

namespace A25;

#region Struct Complex ----------------------------------------------------------------------------
/// <summary>Represents a complex number with real (X) and imaginary (Y) parts</summary>
public struct Complex {
   #region Constructors ---------------------------------------------
   /// <summary>Creates a complex number from the given real and imaginary parts</summary>
   public Complex (double x, double y) => (X, Y) = (x, y);
   #endregion

   #region Properties -----------------------------------------------
   /// <summary>Returns the magnitude (modulus) of this complex number</summary>
   public double Norm => Math.Sqrt (X * X + Y * Y);

   /// <summary>Returns the square of the magnitude of this complex number</summary>
   public double NormSq => X * X + Y * Y;
   #endregion

   #region Methods --------------------------------------------------
   /// <summary>Returns the string form of this number, as X + iY</summary>
   public override string ToString () => $"{X} + i{Y}";
   #endregion

   #region Operators ------------------------------------------------
   /// <summary>Returns the sum of two complex numbers</summary>
   public static Complex operator + (Complex a, Complex b)
      => new (a.X + b.X, a.Y + b.Y);

   /// <summary>Returns the product of two complex numbers</summary>
   public static Complex operator * (Complex a, Complex b)
      => new (a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);
   #endregion

   #region Fields ---------------------------------------------------
   /// <summary>The complex number 0 + i0</summary>
   public readonly static Complex Zero = new (0, 0);

   /// <summary>The real (X) and imaginary (Y) parts</summary>
   public readonly double X, Y;
   #endregion
}
#endregion