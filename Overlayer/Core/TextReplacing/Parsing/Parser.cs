using Overlayer.Core.TextReplacing.Lexing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Overlayer.Core.TextReplacing.Parsing;

public static class Parser {
    public static IEnumerable<IParsed> Parse(IEnumerable<Token> tokens, List<Tag> tags) {
        Queue<Token> queue = new(tokens);
        while(queue.Count > 0) {
            Token t = queue.Dequeue();
            if(t.type == TokenType.TagStart) {
                if(queue.Peek().type == TokenType.TagEnd) {
                    queue.Dequeue();
                    yield return new ParsedString(((char)LexConfig.TagStart).ToString() + ((char)LexConfig.TagEnd).ToString());
                    continue;
                }
                Tag found = null;
                StringBuilder sb = new();
                sb.Append((char)LexConfig.TagStart);
                bool tagNotFound = false;
                List<string> arguments = [];
                while(queue.Count > 0 && t.type != TokenType.TagEnd) {
                    t = queue.Dequeue();
                    if(tagNotFound) {
                        sb.Append(t.value);
                        continue;
                    }
                    if(t.type == TokenType.Identifier) {
                        found = tags.FirstOrDefault(tag => tag.Name == t.value);
                        if(tagNotFound = found == null) {
                            sb.Append(t.value);
                            continue;
                        }
                    }
                    if(t.type is TokenType.ArgStart or TokenType.Colon) {
                        while(queue.Count > 0 && t.type != TokenType.ArgEnd && t.type != TokenType.TagEnd) {
                            t = queue.Dequeue();
                            if(t.type == TokenType.Identifier) {
                                arguments.Add(t.value);
                            }
                        }
                    }
                }
                yield return tagNotFound ? new ParsedString(sb.ToString()) : new ParsedTag(found, arguments);
            } else {
                yield return new ParsedString(t.value);
            }
        }
    }
}
