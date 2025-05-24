namespace Flat
{
    using System.Text.RegularExpressions;

    public abstract class Script
    {
        public virtual void OnUpdate(Entity entity) { }
        public virtual void OnRender(Entity entity) { }
        public virtual void OnLoad(Entity entity) { }
        public virtual void OnUnload(Entity entity) { }
        public virtual void OnAttach(Entity entity) { }
        public virtual void OnDetach(Entity entity) { }
        public virtual void OnIntersection(Entity entity, Entity other, Vector position, Vector impulse) { }
        public override string ToString() => Regex.Replace(GetType().FullName.Replace('+', '.'), @"[^a-zA-Z0-9/.]", "");
    }
}
