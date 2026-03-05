using Overlayer.Core.Interfaces;
using System;

namespace Overlayer.Core;

public class MethodDrawable : IDrawable {
    public string Name { get; set; }
    public Action drawerMethod { get; set; }
    public Action onceMethod { get; set; }

    public void Draw() => drawerMethod?.Invoke();
    public void OnceCall() => onceMethod?.Invoke();

    public MethodDrawable(Action drawer, string name, Action once = null) {
        drawerMethod = drawer;
        Name = name;
        onceMethod = once;
    }
}
