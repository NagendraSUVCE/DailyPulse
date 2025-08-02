
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using Models.Finance;



public class TxnTypeRule
{
    public string BankName { get; set; }
        public string Condition { get; set; }
    public string ConditionValue { get; set; }
    public string SecondaryCondition { get; set; }
    public string TxnType { get; set; }
    public string TxnCategory { get; set; }
    public string TxnRemarks { get; set; }
}

public class ProcessBankStatements
{
    private string filePathExpensesPending = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/TxnTypeRules.csv";

    public List<TxnTypeRule> TxnTypeRules { get; set; }

    public ProcessBankStatements()
    {
        TxnTypeRules = new List<TxnTypeRule>();
    }

    public async Task ProcessBankStatementsRuleBased(List<BankStmt> bankStmts)
    {
        var rules = await LoadTxnTypeRulesFromCsv(filePathExpensesPending);

        foreach (var bankStmt in bankStmts)
        {
            var applicableRules = rules.Where(rule => rule.BankName.Equals(bankStmt.BankName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var rule in rules)
            {
                bool conditionMatched = false;

                if (rule.Condition.Equals("StartsWith", StringComparison.OrdinalIgnoreCase))
                {
                    conditionMatched = bankStmt.TxnDetails.StartsWith(rule.ConditionValue, StringComparison.OrdinalIgnoreCase);
                }
                else if (rule.Condition.Equals("Contains", StringComparison.OrdinalIgnoreCase))
                {
                    conditionMatched = bankStmt.TxnDetails.IndexOf(rule.ConditionValue, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (conditionMatched)
                {
                    if (!string.IsNullOrEmpty(rule.SecondaryCondition) &&
                        bankStmt.TxnDetails.IndexOf(rule.SecondaryCondition, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue; // Skip if secondary condition is not met
                    }

                    bankStmt.TxnType = rule.TxnType;
                    bankStmt.TxnCategory = rule.TxnCategory;
                    bankStmt.TxnRemarks = rule.TxnRemarks;
                    break; // Stop processing once a rule matches
                }
            }

            if (string.IsNullOrEmpty(bankStmt.TxnType))
            {
                bankStmt.TxnType = "ToBeFilled";
                bankStmt.TxnCategory = "ToBeFilled";
            }
        }
    }
    public void LoadTxnTypeRules(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        }))
        {
            TxnTypeRules = csv.GetRecords<TxnTypeRule>().ToList();
        }
    }

    private async Task<List<TxnTypeRule>> LoadTxnTypeRulesFromCsv(string filePath)
    {

        var records = new List<TxnTypeRule>();
        var fileContents = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
        foreach (var line in fileContents.Skip(1)) // Skip header row
        {
            var fields = line.Split(',');

            if (fields.Length >= 6) // Ensure there are enough fields
            {
                var rule = new TxnTypeRule
                {
                    BankName = fields[0],
                    Condition = fields[1],
                    ConditionValue = fields[2],
                    SecondaryCondition = fields[3],
                    TxnType = fields[4],
                    TxnCategory = fields[5],
                    TxnRemarks = fields[6]
                };

                records.Add(rule);
            }
        }

        return records;
    }
}