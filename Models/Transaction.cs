using System;

namespace MyFinance.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } // <- имя категории

        public int AccountId { get; set; }
        public Account Account { get; set; }   // <- имя счета
    }

}
