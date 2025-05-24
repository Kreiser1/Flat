namespace Flat
{
    public sealed class Shape
    {
        public Vector Position = 0, Size = 0;

        public Shape(Vector position, Vector size)
        {
            Position = position;
            Size = size;
        }

        public static Vector Intersect(Shape a, Shape b)
        {
            if ((a.Position + a.Size / 2).X >= (b.Position - b.Size / 2).X && (a.Position - a.Size / 2).X <= (b.Position + b.Size / 2).X && (a.Position + a.Size / 2).Y >= (b.Position - b.Size / 2).Y && (a.Position - a.Size / 2).Y <= (b.Position + b.Size / 2).Y)
                return a.Position + (b.Position - a.Position) / 2;
            else return float.NaN;
        }

        public static Shape Lerp(Shape a, Shape b, float deltaTime = 1) => new Shape(Vector.Lerp(a.Position, b.Position, deltaTime), Vector.Lerp(a.Size, b.Size, deltaTime));

        public static float Distance(Shape a, Shape b) => Vector.Distance(a.Position, b.Position);

        public override string ToString() => nameof(Shape) + $"(({Position.X}, {Position.Y}), ({Size.X}, {Size.Y}))";

        public static bool operator >(float a, Shape b) => b < a;
        public static bool operator <(float a, Shape b) => !(a > b);
        public static bool operator >(Shape a, float b) => a.Size.Length > b;
        public static bool operator <(Shape a, float b) => !(a > b);
        public static bool operator >(Shape a, Shape b) => a.Size.Length > b.Size.Length;
        public static bool operator <(Shape a, Shape b) => !(a > b);
        public static Shape operator +(Shape a, Shape b) => new Shape(a.Position + b.Position, a.Size + b.Size);
        public static Shape operator -(Shape a, Shape b) => a + b * -1;
        public static Shape operator *(Shape a, Shape b) => new Shape(a.Position * b.Position, a.Size * b.Size);
        public static Shape operator /(Shape a, Shape b) => new Shape(a.Position / b.Position, a.Size / b.Size);
        public static bool operator ==(Shape a, Shape b) => a.Position == b.Position && a.Size == b.Size;
        public static bool operator !=(Shape a, Shape b) => !(a == b);

        public static implicit operator Shape(float value) => new Shape(value, value);
        public static implicit operator Shape(Vector value) => new Shape(value, value);
    }
}