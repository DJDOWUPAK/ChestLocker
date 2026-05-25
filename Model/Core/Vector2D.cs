using System;

namespace ChestLocker.Model.Core
{
    public struct Vector2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D Zero => new Vector2D(0, 0);

        public double Length => Math.Sqrt(X * X + Y * Y);

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
        public static Vector2D operator *(Vector2D v, double scalar) => new Vector2D(v.X * scalar, v.Y * scalar);

        public Vector2D Normalize()
        {
            double len = Length;
            if (len == 0) return Zero;
            return new Vector2D(X / len, Y / len);
        }
    }
}