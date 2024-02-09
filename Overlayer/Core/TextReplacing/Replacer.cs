using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Overlayer.Core.TextReplacing
{
    public class Replacer
    {
        public List<Tag> Tags { get; }
        public List<Tag> References { get; }
        private string source;
        private bool compiled;
        private Func<string> compiledMethod;
        public Replacer(List<Tag> tags = null)
        {
            source = string.Empty;
            compiled = false;
            Tags = tags ?? new List<Tag>();
            References = new List<Tag>();
        }
        public Replacer(string source, List<Tag> tags = null) : this(tags) => Source = source;
        public Replacer(IEnumerable<Tag> tags = null) : this(tags.ToList()) { }
        public Replacer(string source, IEnumerable<Tag> tags = null) : this(source, tags.ToList()) { }
        public string Source
        {
            get => source;
            set
            {
                source = value;
                compiled = false;
            }
        }
        public string Replace()
        {
            if (!compiled)
                if (!Compile())
                    return null;
            return compiledMethod();
        }
        public bool Compile()
        {
            if (compiled) return true;
            try
            {
                DynamicMethod dm = new DynamicMethod(string.Empty, typeof(string), Type.EmptyTypes, typeof(Replacer), true);
                ILGenerator il = dm.GetILGenerator();
                References.ForEach(t => t.ReferencedCount--);
                References.Clear();
                il.Emit(OpCodes.Newobj, StrBuilder_Ctor);
                foreach (var parsed in Parse(Lex(source), Tags))
                {
                    if (parsed is ParsedTag pt)
                    {
                        pt.tag.ReferencedCount++;
                        References.Add(pt.tag);
                        parsed.Emit(il);
                    }
                    else parsed.Emit(il);
                    il.Emit(OpCodes.Call, StrBuilder_Append);
                }
                il.Emit(OpCodes.Call, StrBuilder_ToString);
                il.Emit(OpCodes.Ret);
                compiledMethod = (Func<string>)dm.CreateDelegate(typeof(Func<string>));
                return compiled = true;
            }
            catch { return compiled = false; }
        }
        public void UpdateTags(IEnumerable<Tag> tags)
        {
            Tags.Clear();
            Tags.AddRange(tags);
        }
        enum TokenType
        {
            Identifier,
            TagStart,
            TagEnd,
            Colon,
            ArgStart,
            ArgEnd,
            Comma,
        }
        class Token
        {
            public TokenType type;
            public string value;
            public bool afterInvalid;
            public bool IsEmptyToken { get; }
            public Token(TokenType type, string value)
            {
                this.type = type;
                this.value = value;
                IsEmptyToken = string.IsNullOrEmpty(value);
            }
        }
        static List<Token> Lex(string source)
        {
            var lex = Lex_(source).ToList();
            var invalid = lex.FindIndex(t => t.afterInvalid);
            if (invalid >= 0)
            {
                StringBuilder values = new StringBuilder();
                for (int i = invalid; i < lex.Count; i++)
                    values.Append(lex[i].value);
                lex.RemoveRange(invalid, lex.Count - invalid);
                lex.Add(new Token(TokenType.Identifier, values.ToString()));
            }
            return lex;
        }
        static IEnumerable<Token> Lex_(string source)
        {
            StringBuilder sb = new StringBuilder();
            Stack<Token> lastTagToken = new Stack<Token>();
            bool tagStarted = false, escaping = false, colonActivated = false;
            int argDepth = 0;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (escaping)
                {
                    escaping = false;
                    sb.Append(c);
                    continue;
                }
                switch (c)
                {
                    case '{':
                        if (tagStarted || colonActivated) goto default;
                        if (sb.Length > 0)
                            yield return new Token(TokenType.Identifier, sb.ToString());
                        sb.Clear();
                        if (i + 1 < source.Length && source[i + 1] == '{')
                        {
                            sb.Append(c);
                            continue;
                        }
                        var tok = new Token(TokenType.TagStart, c.ToString());
                        tok.afterInvalid = true;
                        lastTagToken.Push(tok);
                        yield return tok;
                        tagStarted = true;
                        break;
                    case '}':
                        if (!tagStarted) goto default;
                        if (sb.Length > 0)
                            yield return new Token(TokenType.Identifier, sb.ToString());
                        sb.Clear();
                        yield return new Token(TokenType.TagEnd, c.ToString());
                        lastTagToken.Pop().afterInvalid = false;
                        tagStarted = colonActivated = false;
                        argDepth = 0;
                        break;
                    case '(':
                        if (!tagStarted || argDepth++ > 0 || colonActivated) goto default;
                        if (sb.Length > 0)
                            yield return new Token(TokenType.Identifier, sb.ToString());
                        sb.Clear();
                        yield return new Token(TokenType.ArgStart, c.ToString());
                        break;
                    case ')':
                        if (!tagStarted || --argDepth > 0 || colonActivated) goto default;
                        if (sb.Length > 0)
                            yield return new Token(TokenType.Identifier, sb.ToString());
                        sb.Clear();
                        yield return new Token(TokenType.ArgEnd, c.ToString());
                        break;
                    case ':':
                        if (!tagStarted || argDepth > 0 || colonActivated) goto default;
                        if (sb.Length > 0)
                            yield return new Token(TokenType.Identifier, sb.ToString());
                        sb.Clear();
                        yield return new Token(TokenType.Colon, c.ToString());
                        colonActivated = true;
                        break;
                    case ',':
                        if (!tagStarted || colonActivated) goto default;
                        if (sb.Length > 0)
                            yield return new Token(TokenType.Identifier, sb.ToString());
                        sb.Clear();
                        if (argDepth > 0)
                        {
                            yield return new Token(TokenType.Comma, c.ToString());
                            break;
                        }
                        else goto default;
                    case '\\':
                        escaping = true;
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            if (sb.Length > 0)
                yield return new Token(TokenType.Identifier, sb.ToString());
        }
        interface IParsed
        {
            void Emit(ILGenerator il);
        }
        class ParsedString : IParsed
        {
            public string str;
            public ParsedString(string str)
            {
                this.str = str;
            }
            void IParsed.Emit(ILGenerator il)
            {
                il.Emit(OpCodes.Ldstr, str);
            }
        }
        class ParsedTag : IParsed
        {
            public Tag tag;
            public List<string> args;
            public ParsedTag(Tag tag, List<string> args)
            {
                this.tag = tag;
                this.args = args;
            }
            void IParsed.Emit(ILGenerator il)
            {
                for (int i = 0; i < tag.ArgumentCount; i++)
                {
                    if (args.Count - 1 < i)
                        il.Emit(OpCodes.Ldstr, tag.GetterOriginal.GetParameters()[i].DefaultValue?.ToString() ?? string.Empty);
                    else il.Emit(OpCodes.Ldstr, args[i]);
                }
                il.Emit(OpCodes.Call, tag.Getter);
            }
        }
        static IEnumerable<IParsed> Parse(IEnumerable<Token> tokens, List<Tag> tags)
        {
            Queue<Token> queue = new Queue<Token>(tokens);
            while (queue.Count > 0)
            {
                Token t = queue.Dequeue();
                if (t.type == TokenType.TagStart)
                {
                    if (queue.Peek().type == TokenType.TagEnd)
                    {
                        queue.Dequeue();
                        yield return new ParsedString("{}");
                        continue;
                    }
                    Tag found = null;
                    StringBuilder sb = new StringBuilder();
                    sb.Append('{');
                    bool tagNotFound = false;
                    List<string> arguments = new List<string>();
                    while (queue.Count > 0 && t.type != TokenType.TagEnd)
                    {
                        t = queue.Dequeue();
                        if (tagNotFound)
                        {
                            sb.Append(t.value);
                            continue;
                        }
                        if (t.type == TokenType.Identifier)
                        {
                            found = tags.FirstOrDefault(tag => tag.Name == t.value);
                            if (tagNotFound = found == null)
                            {
                                sb.Append(t.value);
                                continue;
                            }
                        }
                        if (t.type == TokenType.ArgStart || t.type == TokenType.Colon)
                        {
                            while (queue.Count > 0 && t.type != TokenType.ArgEnd && t.type != TokenType.TagEnd)
                            {
                                t = queue.Dequeue();
                                if (t.type == TokenType.Identifier)
                                    arguments.Add(t.value);
                            }
                        }
                    }
                    if (tagNotFound)
                        yield return new ParsedString(sb.ToString());
                    else yield return new ParsedTag(found, arguments);
                }
                else yield return new ParsedString(t.value);
            }
        }
        public static readonly ConstructorInfo StrBuilder_Ctor = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
        public static readonly MethodInfo StrBuilder_Append = typeof(StringBuilder).GetMethod("Append", new[] { typeof(string) });
        public static readonly MethodInfo StrBuilder_ToString = typeof(StringBuilder).GetMethod("ToString", Type.EmptyTypes);
    }
}
