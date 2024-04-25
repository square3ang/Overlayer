namespace Overlayer.WebAPI.Core.TUF.Types
{
    public class Level
    {
        public int id;
        public string song;
        public string artist;
        public string creator;
        public string charter;
        public string vfxer;
        public string team;
        public double diff;
        public string pguDiff;
        public double pguDiffNum;
        public double pdnDiff;
        public double baseScore;
        public string vidLink;
        public string dlLink;
        public string workshopLink;
        public static Parameter<string> Query(string query = "My Test Level") => new Parameter<string>("query", query);
        public static Parameter<string> ArtistQuery(string artist = "Frums") => new Parameter<string>("artistQuery", artist);
        public static Parameter<string> SongQuery(string song = "multi_arm") => new Parameter<string>("songQuery", song);
        public static Parameter<string> CreatorQuery(string creator = "Zagon") => new Parameter<string>("creatorQuery", creator);
        public static Parameter<int> Offset(int offset = 10) => new Parameter<int>("offset", offset);
        public static Parameter<int> Limit(int limit = 50) => new Parameter<int>("limit", limit);
        public static Parameter<bool> Random(bool random = false) => new Parameter<bool>("random", random);
        public static Parameter<SortType> Sort(SortType sortType = SortType.RECENT_ASC) => new Parameter<SortType>("sort", sortType);
        public static Parameter<int> Seed(int seed = 1234) => new Parameter<int>("seed", seed);
    }
}
