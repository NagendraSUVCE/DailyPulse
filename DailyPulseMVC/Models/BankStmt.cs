using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Finance
{
    public class BankStmt
    {
        public string? BankName { get; set; }
        public DateTime? TxnDate { get; set; }
        public string? TxnDetails { get; set; }

        public decimal Withdrawals { get; set; }
        public decimal Deposits { get; set; }
        public decimal Balance { get; set; }
        public string? Misc { get; set; }

        public string? TxnType { get; set; } // Investment/Income/Expense/Transfer
        public string? TxnCategory { get; set; } // Salary/Bonus/Dividend/Interest/ExpenseType/TransferType
        public string? TxnSubCategory { get; set; } // Subcategory of the transaction
        public string? TxnRemarks { get; set; } // Any additional remarks for the transaction
    }
}