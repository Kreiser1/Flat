namespace Flat
{
    using System.Drawing;

    public sealed class Sprite
    {
        internal float _time = 0;
        internal Image _rendered = null;

        public float Quality = 1, Delay = 1, Angle = 0;
        public Image Image = null;
        public Brush Brush = null, TextBrush = null;
        public string Text = string.Empty, Font = "System";
        public FontStyle FontStyle = FontStyle.Regular;
        public bool Mirror = false;

        public override string ToString() => nameof(Sprite);
    }
}
