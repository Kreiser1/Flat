namespace Flat
{
    using System;

    public sealed class Vector
    {
        public float X = 0, Y = 0;

        public Vector Unit => this / Length;
        public Vector UnitX => new Vector(X, 0);
        public Vector UnitY => new Vector(0, Y);
        public float Angle => (float)Math.Atan2(Y, X) * (180 / (float)Math.PI);
        public float Length => (float)Math.Sqrt(Y * Y + X * X);

        public Vector(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector(float d) : this(d, d) { }

        public static Vector Lerp(Vector a, Vector b, float deltaTime = 1) => new Vector(a.X * (1 - deltaTime) + b.X * deltaTime, a.Y * (1 - deltaTime) + b.Y * deltaTime) - a;

        public static float Distance(Vector a, Vector b) => (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));

        public override string ToString() => nameof(Vector) + $"({X}, {Y})";

        public static bool operator >(float a, Vector b) => b < a;
        public static bool operator <(float a, Vector b) => !(a > b);
        public static bool operator >(Vector a, float b) => a.Length > b;
        public static bool operator <(Vector a, float b) => !(a > b);
        public static bool operator >(Vector a, Vector b) => a.Length > b.Length;
        public static bool operator <(Vector a, Vector b) => !(a > b);
        public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, Vector b) => a + b * -1;
        public static Vector operator -(Vector a) => new Vector(-a.X, -a.Y);
        public static Vector operator *(Vector a, Vector b) => new Vector(a.X * b.X, a.Y * b.Y);
        public static Vector operator /(Vector a, Vector b) => new Vector(a.X / b.X, a.Y / b.Y);
        public static Vector operator %(Vector a, double b) => new Vector(a.X * (float)Math.Cos(b / (180 * (float)Math.PI)) - a.Y * (float)Math.Sin(b / (180 * (float)Math.PI)), a.X * (float)Math.Sin(b / (180 * (float)Math.PI)) + a.Y * (float)Math.Cos(b / (180 * (float)Math.PI)));
        public static bool operator ==(Vector a, Vector b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vector a, Vector b) => !(a == b);

        public static implicit operator Vector(float value) => new Vector(value, value);
    }
}
