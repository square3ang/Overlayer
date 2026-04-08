using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overlayer.Core.TextReplacing.Parsing;

public static class Parser {
    public static IEnumerable<IParsed> Parse(string input, List<Tag> tags) {
        int lastIndex = 0;

        foreach(Match m in TagRegex.Matches(input)) {
            if(m.Index > lastIndex) {
                yield return new ParsedString(input.Substring(lastIndex, m.Index - lastIndex));
            }

            string tagName = m.Groups[1].Value;
            string sep = m.Groups[2].Value;
            string rawArgs = m.Groups[3].Value;

            Tag found = tags.FirstOrDefault(tag => tag.Name == tagName);
            bool tagNotFound = found == null;
            List<string> arguments = [];
            Tag.FormatType formatType = Tag.FormatType.None;

            if(!tagNotFound) {
                if(sep == ":") {
                    arguments.Add(rawArgs);
                } else if(sep == ";") {
                    if(found.FormattingType != Tag.FormatType.None) {
                        try {
                            if(found.FormattingType == Tag.FormatType.Float) {
                                float test = 123.456f;
                                _ = test.ToString(rawArgs);
                            } else {
                                double test = 123.456d;
                                _ = test.ToString(rawArgs);
                            }
                            arguments.Add(rawArgs);
                            formatType = found.FormattingType;
                        } catch {
                            tagNotFound = true;
                        }
                    } else {
                        tagNotFound = true;
                    }
                } else if(sep == "(") {
                    rawArgs = rawArgs.TrimEnd(')');

                    if(!string.IsNullOrWhiteSpace(rawArgs)) {
                        foreach(var arg in rawArgs.Split(',')) {
                            arguments.Add(arg.Trim());
                        }
                    }
                }
            }

            if(tagNotFound) {
                yield return new ParsedString(m.Value);
            } else if(formatType != Tag.FormatType.None) {
                yield return new ParsedFormatTag(found, formatType, arguments[0]);
            } else {
                yield return new ParsedTag(found, arguments);
            }

            lastIndex = m.Index + m.Length;
        }

        if(lastIndex < input.Length) {
            yield return new ParsedString(input.Substring(lastIndex));
        }
    }

    public static readonly Regex TagRegex = new(
        @"\{(\w+)([:;\(]?)([^}]*)\}",
        RegexOptions.Compiled
    );
}
