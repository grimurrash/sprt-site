namespace NewSprt.Data.App.Models
{
    public class DactyloscopyStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public const int NotSelected = 1;
        public const int Selected = 2;
        public const int SentEarlier = 3;
    }
}