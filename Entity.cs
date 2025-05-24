namespace Flat
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public sealed class Entity
    {
        private List<Script> _scripts = new List<Script>();
        private List<string> _tags = new List<string>();

        public Shape Shape = 0;
        public Body Body = new Body();
        public Sprite Sprite = new Sprite { Quality = 0 };
        public Script[] Scripts => _scripts.ToArray();
        public string[] Tags => _tags.ToArray();

        public Entity(Script script = null)
        {
            if (script != null)
                AttachScript(script);
        }

        public bool AttachScript<T>(T script) where T : Script
        {
            foreach (var script2 in Scripts)
            {
                if (script2.GetType().IsSubclassOf(typeof(T)) || script2.GetType() == typeof(T))
                    return false;
            }

            _scripts.Add(script);
            script.OnAttach(this);
            return true;
        }

        public T GetScript<T>() where T : Script
        {
            foreach (var script in Scripts)
            {
                if (script.GetType().IsSubclassOf(typeof(T)) || script.GetType() == typeof(T))
                    return script as T;
            }

            return null;
        }

        public bool DetachScript<T>() where T : Script
        {
            foreach (var script in Scripts)
            {
                if (script.GetType().IsSubclassOf(typeof(T)) || script.GetType() == typeof(T))
                {
                    _scripts.Remove(script);
                    script.OnDetach(this);
                    return true;
                }
            }

            return false;
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
    }
}
