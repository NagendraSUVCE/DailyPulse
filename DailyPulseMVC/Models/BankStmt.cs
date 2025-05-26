using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Finance
{
    public class BankStmt
    {
        public string BankName { get; set; }
        public DateTime? TxnDate { get; set; }
        public string TxnDetails { get; set; }

        public decimal Withdrawals { get; set; }
        public decimal Deposits { get; set; }
        public decimal Balance { get; set; }
        public string Misc { get; set; }
    }
}