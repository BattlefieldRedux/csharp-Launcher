namespace OSH_Launcher
{
    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public string birthday { get; set; }
        public string description { get; set; }
        public string language { get; set; }
        public string game_token { get; set; }
        public string error { get; set; }
    }

    public class Token
    {
        public string token { get; set; }
        public string error { get; set; }
    }
}
