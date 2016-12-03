using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.Domain
{
    public class Transaction
    {
        public int Suid { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Listing { get; set; }
        public decimal Price { get; set; }
        public int ShareAmount { get; set; }
    }
}
