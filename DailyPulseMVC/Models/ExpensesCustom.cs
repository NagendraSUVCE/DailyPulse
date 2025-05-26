using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Finance
{
    public class ExpensesCustom
    {
        public DateTime TxnDate { get; set; }
        public string Daily15MinLogId { get; set; }
        public string ExpneseDescription { get; set; }
        public string ExpenseCategory { get; set; }
        public string ExpenseSubCategory { get; set; }
        public string ExpensePaymentType { get; set; }
        public string ExpenseVendor { get; set; }
        public decimal ExpenseAmt { get; set; }
        public decimal ExpenseDeposits { get; set; }
        public string ExpenseMisc { get; set; }
    }

    public class ExpensesPending
    {
        public DateTime ExpensesPendingLogRunDate { get; set; }
        public int InExpenseSheet { get; set; }
        public int CompletelyAccounted { get; set; }
        public int InDailyLogSheet { get; set; }
        public int AccountingPending { get; set; }
        public int ExpensesLoggingPending { get; set; }
        public int ExpenseLoggingAutomatic { get; set; }
        public string ExpenseMisc { get; set; }
    }
}
