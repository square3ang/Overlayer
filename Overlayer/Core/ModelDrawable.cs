using Overlayer.Core.Interfaces;

namespace Overlayer.Core
{
    public abstract class ModelDrawable<T> : IDrawable where T : IModel
    {
        public T model;
        public string Name { get; protected set; }
        public ModelDrawable(T model)
        {
            this.model = model;
        }
        public abstract void Draw();
    }
}
