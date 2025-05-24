namespace Flat
{
    public sealed class Body
    {
        public Vector Velocity = 0;
        public bool Static = false;
        public bool Intangible = false;
        public float Bounciness = 0;
        public float Friction = 0;
        public bool Gravity = false;
        public bool Pushable = false;

        public override string ToString() => nameof(Body);
    }
}