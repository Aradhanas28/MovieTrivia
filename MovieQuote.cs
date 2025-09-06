namespace MovieTrivia.Models
{
    public class MovieQuote
    {
        public int Id { get; set; }
        public string QuoteText { get; set; }
        public string CorrectMovie { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
    }
}