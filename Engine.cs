namespace Flat
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Threading;
    using System.Windows.Forms;
    using System;

    public static class Engine
    {
        public static volatile string License = """
BSD 3-Clause License

Copyright(c) 2025, Syrtsov Vadim

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
""";

        private sealed class Window : Form
        {
            public Window()
            {
                CheckForIllegalCrossThreadCalls = false;
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.ResizeRedraw, false);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.Opaque, true);
                SuspendLayout();
            }

            protected override void OnLoad(EventArgs e) => Visible = true;

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.InterpolationMode = InterpolationMode.Low;
                e.Graphics.SmoothingMode = SmoothingMode.None;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

                var transform = e.Graphics.Transform;

                transform.Scale(_windowScale.X, _windowScale.Y);
                transform.Translate(0.5f, 0.5f);
                transform.Rotate(Scene.CameraAngle);
                transform.Translate(-0.5f, -0.5f);
                transform.Scale(1 / Scene.Camera.Size.X, 1 / Scene.Camera.Size.Y);
                transform.Translate(-(Scene.Camera.Position - Scene.Camera.Size / 2).X, -(1 - (Scene.Camera.Position + Scene.Camera.Size / 2)).Y);

                e.Graphics.Clear(Color.Black);

                foreach (var entity in Scene.Entities)
                    if (entity.Sprite._rendered != null)
                    {
                        e.Graphics.Transform = transform.Clone();
                        e.Graphics.TranslateTransform(entity.Shape.Position.X, 1 - entity.Shape.Position.Y);
                        e.Graphics.RotateTransform(entity.Sprite.Mirror ? -entity.Sprite.Angle : entity.Sprite.Angle);
                        e.Graphics.TranslateTransform(-entity.Shape.Position.X, -(float)(1 - entity.Shape.Position.Y));
                        e.Graphics.DrawImage(entity.Sprite._rendered, (entity.Shape.Position - entity.Shape.Size / 2).X, 1 - (entity.Shape.Position + entity.Shape.Size / 2).Y, entity.Shape.Size.X, entity.Shape.Size.Y);
                    }
            }

            protected override void OnKeyDown(KeyEventArgs e) => _keys[(byte)e.KeyCode] = true;

            protected override void OnKeyUp(KeyEventArgs e) => _keys[(byte)e.KeyCode] = false;

            protected override void OnMouseMove(MouseEventArgs e)
            {
                _deltaCursor = _cursorPosition;
                _cursorPosition = new Vector(e.X, _windowScale.Y - e.Y) / _windowScale - 0.5f + Scene.Camera.Position;
                _deltaCursor = _cursorPosition - _deltaCursor;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                var button = (byte)((uint)e.Button / 0x100000);

                switch (button)
                {
                    case 4:
                        button = 3;
                        break;
                    case 8:
                        button = 4;
                        break;
                    case 10:
                        button = 5;
                        break;
                }

                _mouseButtons[button - 1] = true;
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                var button = (byte)((uint)e.Button / 0x100000);

                switch (button)
                {
                    case 4:
                        button = 3;
                        break;
                    case 8:
                        button = 4;
                        break;
                    case 10:
                        button = 5;
                        break;
                }

                _mouseButtons[button - 1] = false;
            }

            protected override void OnFormClosed(FormClosedEventArgs e) => Stop();
        }

        private static bool Render(Sprite sprite, bool force = false)
        {
            sprite._time -= _deltaTime;

            if (sprite._time > 0 && !force)
                return false;

            sprite._time = sprite.Delay;

            var resolution = (ushort)(sprite.Quality * _windowScale.Length);

            if (resolution > _windowScale.Length)
                resolution = (ushort)_windowScale.Length;
            else if (resolution == 0)
                resolution = 1;

            Image rendered = new Bitmap(resolution, resolution);
            var renderer = Graphics.FromImage(rendered);

            renderer.InterpolationMode = InterpolationMode.Low;
            renderer.SmoothingMode = SmoothingMode.None;
            renderer.PixelOffsetMode = PixelOffsetMode.None;
            renderer.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

            if (sprite.Image != null)
                renderer.DrawImage(sprite.Image, 0, 0, resolution, resolution);
            if (sprite.Brush != null)
                renderer.FillRectangle(sprite.Brush, 0, 0, resolution, resolution);
            if (sprite.Text.Length > 0 && sprite.TextBrush != null)
                renderer.DrawString(sprite.Text, new Font(sprite.Font, sprite.Text.Length == 1 ? resolution / (sprite.Text.Length + 1) : resolution / sprite.Text.Length, sprite.FontStyle), sprite.TextBrush, (resolution - (resolution / sprite.Text.Length)) / 4, (resolution - (resolution / sprite.Text.Length)) / 2);

            if (sprite.Mirror)
                rendered.RotateFlip(RotateFlipType.RotateNoneFlipX);

            sprite._rendered = rendered;
            return true;
        }

        private static readonly Window _window = new Window();

        private static readonly Thread _primaryThread = new Thread(new ThreadStart(() =>
        {
            var stopwatch = Stopwatch.StartNew();

            while (_running)
            {
                _deltaTime = (float)stopwatch.Elapsed.TotalSeconds * Scene.TimeScale;
                _time += _deltaTime;

                stopwatch.Restart();

                try
                {
                    foreach (var entity in _entities)
                    {
                        foreach (var script in entity.Scripts)
                            script.OnUpdate(entity);

                        if (!entity.Body.Static && entity.Body.Gravity)
                            entity.Body.Velocity += Scene.Gravity * _deltaTime;

                        if (!entity.Body.Static)
                            entity.Shape.Position.X += entity.Body.Velocity.X * _deltaTime;

                        foreach (var entity2 in _entities)
                        {
                            if (!Equals(entity, entity2) && !float.IsNaN(Shape.Intersect(entity.Shape, entity2.Shape).Length))
                            {
                                var position = Shape.Intersect(entity.Shape, entity2.Shape);

                                foreach (var script in entity.Scripts)
                                    script.OnIntersection(entity, entity2, position, float.NaN);

                                foreach (var script in entity2.Scripts)
                                    script.OnIntersection(entity2, entity, position, float.NaN);

                                if (!entity.Body.Intangible && !entity2.Body.Intangible && !(entity.Body.Static && entity2.Body.Static))
                                {
                                    var impulse = new Vector((entity.Body.Velocity.X + entity2.Body.Velocity.X) / 2, 0);

                                    foreach (var script in entity.Scripts)
                                        script.OnIntersection(entity, entity2, position, impulse);

                                    foreach (var script in entity2.Scripts)
                                        script.OnIntersection(entity2, entity, position, impulse);

                                    if (!entity.Body.Static)
                                        entity.Shape.Position.X -= entity.Body.Velocity.X * _deltaTime;

                                    if (entity2.Body.Pushable && !entity2.Body.Static)
                                        entity2.Body.Velocity.X = impulse.X;

                                    entity.Body.Velocity.X =
                                        -entity.Body.Velocity.X * entity.Body.Bounciness;
                                    entity.Body.Velocity.Y -=
                                        entity.Body.Velocity.Y * entity.Body.Friction;
                                }
                            }
                        }

                        if (!entity.Body.Static)
                            entity.Shape.Position.Y += entity.Body.Velocity.Y * _deltaTime;

                        foreach (var entity2 in _entities)
                        {
                            if (!Equals(entity, entity2) && !float.IsNaN(Shape.Intersect(entity.Shape, entity2.Shape).Length))
                            {
                                var position = Shape.Intersect(entity.Shape, entity2.Shape);

                                foreach (var script in entity.Scripts)
                                    script.OnIntersection(entity, entity2, position, float.NaN);

                                foreach (var script in entity2.Scripts)
                                    script.OnIntersection(entity2, entity, position, float.NaN);

                                if (!entity.Body.Intangible && !entity2.Body.Intangible && !(entity.Body.Static && entity2.Body.Static))
                                {
                                    var impulse = new Vector(0, (entity.Body.Velocity.Y + entity2.Body.Velocity.Y) / 2);

                                    foreach (var script in entity.Scripts)
                                        script.OnIntersection(entity, entity2, position, impulse);

                                    foreach (var script in entity2.Scripts)
                                        script.OnIntersection(entity2, entity, position, impulse);

                                    if (!entity.Body.Static)
                                        entity.Shape.Position.Y -= entity.Body.Velocity.Y * _deltaTime;

                                    if (entity2.Body.Pushable && !entity2.Body.Static)
                                        entity2.Body.Velocity.Y = impulse.Y;

                                    entity.Body.Velocity.Y =
                                        -entity.Body.Velocity.Y * entity.Body.Bounciness;
                                    entity.Body.Velocity.X -=
                                        entity.Body.Velocity.X * entity.Body.Friction;
                                }
                            }
                        }

                        if (Render(entity.Sprite))
                        {
                            foreach (var script in entity.Scripts)
                                script.OnRender(entity);
                        }
                    }
                }
                catch { }

                _window.Invalidate();
                _updates++;
            }

            _window.Close();
        }))
        {
            Priority = ThreadPriority.Highest
        };

        private static readonly Thread _secondaryThread = new Thread(new ThreadStart(() => Application.Run(_window)))
        {
            Priority = ThreadPriority.Lowest
        };

        private static Vector _windowScale => new Vector(_window.ClientRectangle.Width, _window.ClientRectangle.Height);
        private static Vector _screenScale => new Vector(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);

        private static bool _running = false;
        private static List<Entity> _entities = new List<Entity>();
        private static List<Sound> _sounds = new List<Sound>();
        private static bool[] _keys = new bool[256], _mouseButtons = new bool[5];
        private static Vector _cursorPosition = 0, _deltaCursor = 0;
        private static float _time = 0, _deltaTime = 0;
        private static ulong _updates = 0;

        public static class Scene
        {
            public static bool Running => _running;
            public static Entity[] Entities => _entities.ToArray();
            public static Sound[] Sounds => _sounds.ToArray();
            public static bool[] Keys => (bool[])_keys.Clone();
            public static bool[] MouseButtons => (bool[])_mouseButtons.Clone();
            public static Vector CursorPosition => _cursorPosition;
            public static Vector DeltaCursor => _deltaCursor;
            public static float Time => _time;
            public static float DeltaTime => _deltaTime;
            public static ulong Updates => _updates;

            public static Shape Shape
            {
                get => new Shape(new Vector(_window.Location.X - _window.Width / 2, _screenScale.Y - _window.Location.Y - _window.Height / 2), new Vector(_window.Width, _window.Height)) / _screenScale - new Shape(new Vector(0, 0.5f), 0);
                set
                {
                    var shape = value * _screenScale + new Shape(_screenScale / 2, 0);
                    _window.Location = new Point((int)(shape.Position.X - shape.Size.X / 2), (int)(_screenScale - shape.Position - shape.Size.Y / 2).Y);
                    _window.Size = new Size((int)shape.Size.X, (int)shape.Size.Y);
                }
            }

            public static Shape Camera = new Shape(0.5f, 1);

            public static float CameraAngle = 0;

            public static Vector Gravity = new Vector(0, -9.8f);

            public static float TimeScale = 1;

            public static byte State
            {
                get => (byte)_window.WindowState;
                set => _window.WindowState = (FormWindowState)((value + 1) & 0b11) - 1;
            }

            public static bool Controllable
            {
                get => _window.ControlBox;
                set
                {
                    _window.ControlBox = value;
                    Sizeable = value;
                }
            }

            public static bool Sizeable
            {
                get => _window.MaximizeBox;
                set
                {
                    if (!Controllable)
                        value = false;

                    _window.MaximizeBox = value;
                    _window.FormBorderStyle = value ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
                }
            }

            public static bool Visible
            {
                get => _window.Visible;
                set => _window.Visible = value;
            }

            public static string Title
            {
                get => _window.Text;
                set => _window.Text = value;
            }

            public static Sprite Icon
            {
                get => _window.ShowIcon ? new Sprite() { Image = _window.Icon.ToBitmap() } : null;
                set
                {
                    if (value == null)
                    {
                        _window.Icon = SystemIcons.Application;
                        return;
                    }
                    Render(value, true);
                    _window.Icon = System.Drawing.Icon.FromHandle((value._rendered as Bitmap).GetHicon());
                }
            }
        }

        public static bool LoadEntity(Entity entity)
        {
            if (_entities.Contains(entity))
                return false;

            _entities.Add(entity);

            foreach (var script in entity.Scripts)
                script.OnLoad(entity);

            return true;
        }

        public static bool UnloadEntity(Entity entity)
        {
            if (!_entities.Contains(entity))
                return false;

            _entities.Remove(entity);

            foreach (var script in entity.Scripts)
                script.OnUnload(entity);

            return true;
        }

        public static bool LoadSound(Sound sound)
        {
            if (_sounds.Contains(sound))
                return false;

            _sounds.Add(sound);

            sound._loaded = true;

            return true;
        }

        public static bool UnloadSound(Sound sound)
        {
            if (!_sounds.Contains(sound))
                return false;

            _sounds.Remove(sound);

            sound._loaded = false;

            return true;
        }

        public static void Focus() => _window.Activate();

        public static void Join() => _primaryThread.Join();

        public static void Stop() => _running = false;

        public static void Start()
        {
            _running = true;
            _entities.Clear();
            _keys = new bool[256];
            _mouseButtons = new bool[5];
            _cursorPosition = 0;
            _deltaCursor = 0;
            _time = 0;
            _deltaTime = 0;
            _updates = 0;

            _secondaryThread.Start();
            _primaryThread.Start();

            while (!Scene.Visible)
                ;

            Scene.Shape = new Shape(0, 0.5f);
            Scene.Gravity = new Vector(0, -9.8f);
            Scene.TimeScale = 1;
            Scene.Camera = new Shape(0, 1);
            Scene.CameraAngle = 0;
            Scene.State = 0;
            Scene.Controllable = true;
            Scene.Sizeable = true;
            Scene.Title = string.Empty;
            Scene.Icon = null;
            Scene.Visible = true;

            Focus();
        }
    }
}