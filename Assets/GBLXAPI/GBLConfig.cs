namespace DIG.GBLXAPI
{
    public class GBLConfig
    {
        public const string StandardsDefaultPath = "data/GBLxAPI_Vocab_Default";
        public const string StandardsUserPath = "data/GBLxAPI_Vocab_User";

        public string lrsURL = "https://lrsmocah.lip6.fr/data/xAPI";

        // Fill in these fields for GBLxAPI setup.
        public string lrsUser = "2da3ea73b1dcf6258c02649d1d3f7a9385b74d61";
        public string lrsPassword = "90935a12c7eeb44d1d6acefd0f413e4d4c552467";

        public string companyURI = "https://www.lip6.fr/mocah/";
        public string gameURI = "https://www.lip6.fr/mocah/invalidURI/activity-types/serious-game/LearningScape";
        public string gameName = "E-LearningScape";

        public GBLConfig()
        {

        }

        public GBLConfig(string lrsURL, string lrsUser, string lrsPassword)
        {
            this.lrsURL = lrsURL;
            this.lrsUser = lrsUser;
            this.lrsPassword = lrsPassword;
        }
    }
}
