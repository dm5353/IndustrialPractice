namespace MyFinance.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } // Income или Expense
    }
}
