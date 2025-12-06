namespace MyFinance.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; }
        public string Color { get; set; } = "#FFCCCCCC";
        public ICollection<Transaction> Transactions { get; set; }
    }
}
