using System.Text;

namespace Overlayer.Wiki;

public static class WikiParser {

    public static string ReplacePaired(string text, string marker, string openTag, string closeTag) {
        if(string.IsNullOrEmpty(text) || string.IsNullOrEmpty(marker)) {
            return text;
        }

        bool inCode = false;
        int count = 0;
        for(int i = 0; i < text.Length;) {
            if(text[i] == '`') {
                inCode = !inCode;
                i++;
                continue;
            }

            if(!inCode && i + marker.Length <= text.Length &&
                string.CompareOrdinal(text, i, marker, 0, marker.Length) == 0) {
                count++;
                i += marker.Length;
                continue;
            }
            i++;
        }
        if(count < 2 || (count & 1) != 0) {
            return text;
        }

        var sb = new StringBuilder(text.Length);
        inCode = false;
        bool open = true;

        for(int i = 0; i < text.Length;) {
            if(text[i] == '`') {
                inCode = !inCode;
                sb.Append('`');
                i++;
                continue;
            }

            if(!inCode && i + marker.Length <= text.Length &&
                string.CompareOrdinal(text, i, marker, 0, marker.Length) == 0) {
                sb.Append(open ? openTag : closeTag);
                i += marker.Length;
                open = !open;
            } else {
                sb.Append(text[i]);
                i++;
            }
        }
        return sb.ToString();
    }

    public static string Parse(string text) {
        string[] lines = text.Split('\n');
        for(int i = 0; i < lines.Length; i++) {

            bool ignoreHeader = false;
            if(lines[i].StartsWith("\\")) {
                lines[i] = lines[i].TrimStart('\\').TrimStart();
                ignoreHeader = true;
            }

            if(!ignoreHeader) {
                if(lines[i].StartsWith("###### ")) {
                    lines[i] = $"<size=10><b>{lines[i].Substring(7).Trim()}</b></size>";
                } else if(lines[i].StartsWith("##### ")) {
                    lines[i] = $"<size=12><b>{lines[i].Substring(6).Trim()}</b></size>";
                } else if(lines[i].StartsWith("#### ")) {
                    lines[i] = $"<size=14><b>{lines[i].Substring(5).Trim()}</b></size>";
                } else if(lines[i].StartsWith("### ")) {
                    lines[i] = $"<size=18><b>{lines[i].Substring(4).Trim()}</b></size>";
                } else if(lines[i].StartsWith("## ")) {
                    lines[i] = $"<size=24><b>{lines[i].Substring(3).Trim()}</b></size>";
                } else if(lines[i].StartsWith("# ")) {
                    lines[i] = $"<size=32><b>{lines[i].Substring(2).Trim()}</b></size>";
                } else if(lines[i].StartsWith("* ") || lines[i].StartsWith("- ")) {
                    lines[i] = $"• {lines[i].Substring(2).Trim()}";
                } else if(lines[i].StartsWith("> ")) {
                    lines[i] = $"| <i>{lines[i].Substring(2).Trim()}</i>";
                }
            }
            lines[i] = ReplacePaired(lines[i], "***", "<b><i>", "</i></b>");
            lines[i] = ReplacePaired(lines[i], "**", "<b>", "</b>");
            lines[i] = ReplacePaired(lines[i], "*", "<i>", "</i>");
        }
        return string.Join("\n", lines);
    }
}
