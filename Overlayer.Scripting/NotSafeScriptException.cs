using System;

namespace Overlayer.Scripting
{
    public class NotSafeScriptException : Exception
    {
        public NotSafeScriptException() : base() { }
        public NotSafeScriptException(string message) : base(message) { }
    }
}
