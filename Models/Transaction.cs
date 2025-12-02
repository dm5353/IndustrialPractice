using System;

namespace MyFinance.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // Income или Expense
        public int CategoryId { get; set; }
        public int AccountId { get; set; }
        public string Note { get; set; } = "";
    }
}
