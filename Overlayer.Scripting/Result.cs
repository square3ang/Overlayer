using Jint;
using System;
using Esprima;
using System.IO;
using System.Text;

namespace Overlayer.Scripting
{
    public class Result : IDisposable
    {
        Engine engine;
        Esprima.Ast.Script scr;
        public Result(Engine engine, string expr)
        {
            this.engine = engine;
            scr = new Esprima.Ast.Script(new JavaScriptParser().ParseScript(RemoveImports(expr)).Body, false);
        }
        public object Eval() => engine.Evaluate(scr).ToObject();
        public void Exec() => engine.Execute(scr);
        public object GetValue(string key) => engine.GetValue(key).ToObject();
        public void SetValue(string key, object value) => engine.SetValue(key, value);
        public void Dispose() => Dispose(false);
        void Dispose(bool byFinalizer)
        {
            engine = null;
            scr = null;
            if (!byFinalizer)
                GC.SuppressFinalize(this);
        }
        ~Result() => Dispose(true);
        static string RemoveImports(string expr)
        {
            using (StringReader sr = new StringReader(expr))
            {
                StringBuilder sb = new StringBuilder();
                string line = null;
                while ((line = sr.ReadLine()) != null)
                    if (!line.StartsWith("import"))
                        sb.AppendLine(line);
                    else sb.AppendLine();
                return sb.ToString();
            }
        }
    }
}
