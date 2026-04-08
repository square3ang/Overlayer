namespace Overlayer.Core.TextReplacing.Lexing;

public enum LexConfig : short {
    TagStart = (short)'{',
    TagEnd = (short)'}',
    TagOptSeparator = (short)':',
    TagArgStart = (short)'(',
    TagArgEnd = (short)')',
    TagArgSeparator = (short)','
}
