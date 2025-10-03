using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlayer.Wiki {
    public struct Body {
        public string Title;
        public string Content;
    }
    public class WikiData {
        public string Title;
        public string Language;
        public List<Body> Sections = new List<Body>();
    }
}
