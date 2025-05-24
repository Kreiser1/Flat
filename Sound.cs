namespace Flat
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Media;

    public sealed class Sound
    {
        private List<string> _tags = new List<string>();

        internal bool _loaded = false;
        internal SoundPlayer _soundPlayer = null;

        public string FilePath = string.Empty;
        public bool Loop = false;
        public string[] Tags => _tags.ToArray();

        public bool Play()
        {
            if (!_loaded)
                return false;

            _soundPlayer?.Stop();

            _soundPlayer = new SoundPlayer(FilePath);

            if (Loop)
                _soundPlayer.PlayLooping();
            else
                _soundPlayer.Play();

            return true;
        }

        public bool Stop()
        {
            if (!_loaded || _soundPlayer == null)
                return false;

            _soundPlayer.Stop();

            return true;
        }

        public bool Tag(string tag)
        {
            if (!_tags.Contains(tag))
            {
                _tags.Add(Regex.Replace(tag, @"[^a-z0-9]", ""));
                return true;
            }

            return false;
        }

        public bool Tagged(string tag) => _tags.Contains(tag);

        public bool Untag(string tag)
        {
            if (_tags.Contains(tag))
            {
                _tags.Remove(tag);
                return true;
            }

            return false;
        }

        public override string ToString() => nameof(Sound);
    }
}