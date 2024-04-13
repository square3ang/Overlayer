using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Overlayer.WebAPI.Core.Adofaigg.Types
{
    public class Json
    {
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
