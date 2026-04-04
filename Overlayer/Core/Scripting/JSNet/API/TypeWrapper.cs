using System;

namespace Overlayer.Core.Scripting.JSNet.API;

public class TypeWrapper(Type type) {
    private readonly Type _type = type;

    public object New(params object[] args) => Activator.CreateInstance(_type, args);
}