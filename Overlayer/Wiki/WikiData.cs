using System.Collections.Generic;

namespace Overlayer.Wiki;

public struct Body {
    public string Title;
    public string Content;
}
public class WikiData {
    public string Title;
    public string Language;
    public List<Body> Sections = new();
}
