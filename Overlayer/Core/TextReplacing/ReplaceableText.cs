using Overlayer.Core.TextReplacing.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Overlayer.Core.TextReplacing;

public class ReplaceableText : IDisposable {
    private bool disposed;
    public List<IParsed> Replaceables { get; private set; }
    public ReplaceableText(IEnumerable<IParsed> replaceables) {
        Replaceables = replaceables.ToList();
        Replaceables.ForEach(p => {
            if(p is ParsedTag tag) {
                tag.tag.ReferencedCount++;
            }
        });
    }
    public string Replace() {
        return disposed
            ? throw new ObjectDisposedException(GetType().FullName)
            : Replaceables.Aggregate(new StringBuilder(), (sb, p) => {
                if(p is ParsedString str) {
                    sb.Append(str.str);
                } else if(p is ParsedTag tag) {
                    sb.Append(InvokeTag(tag.tag, tag.args.ToArray()));
                }

                return sb;
            }).ToString();
    }
    public static object InvokeTag(Tag tag, params string[] args) => tag.Getter.Invoke(null, args);
    public static ReplaceableText Create(string source, IEnumerable<Tag> tags) => new(Parser.Parse(source, [.. tags]));
    public void Dispose() {
        if(disposed) {
            return;
        }

        Replaceables.ForEach(p => {
            if(p is ParsedTag tag) {
                tag.tag.ReferencedCount--;
            }
        });
        Replaceables = null;
        GC.SuppressFinalize(this);
        disposed = true;
    }
    ~ReplaceableText() => Dispose();
}
