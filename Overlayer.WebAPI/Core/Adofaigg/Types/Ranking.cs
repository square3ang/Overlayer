using System.Threading.Tasks;

namespace Overlayer.WebAPI.Core.Adofaigg.Types
{
    public class Ranking : Json
    {
        public int id;
        public string name;
        public double totalPp;
        public Play bestPlay;
        public static Parameter<int> Offset(int offset = 0) => new Parameter<int>("offset", offset);
        public static Parameter<int> Amount(int amount = 50) => new Parameter<int>("amount", amount);
    }
}
