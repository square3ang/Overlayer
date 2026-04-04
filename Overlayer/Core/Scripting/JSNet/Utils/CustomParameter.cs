using System;
using System.Reflection;

namespace Overlayer.Core.Scripting.JSNet.Utils;

public class CustomParameter : ParameterInfo {
    public CustomParameter(Type type, string name) {
        ClassImpl = type;
        NameImpl = name;
    }
}
