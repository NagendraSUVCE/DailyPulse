using ExcelDataReader;
using Models.DailyLog;
using Models.Finance;
using System.Data;
// Removed invalid namespace reference
using System.IO;

namespace Utility.Excel
{
    public class ExcelUtilities
    {
        public static DataSet GetDataFromExcelNewWay(string filePath)
        {
            DataSet dataSet = new DataSet();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                }
            }
            return dataSet;
        }

        public static void CreateCsvFromList(List<ExpensesCustom> lstDailyLog15Min, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // Add headers
                writer.WriteLine("Date,Id,Description");

                // Add data
                foreach (var log in lstDailyLog15Min)
                {
                    var line = $"{log.TxnDate:yyyy-MM-dd HH:mm:ss},{log.Daily15MinLogId},{log.ExpneseDescription},{log.ExpenseCategory},{log.ExpenseSubCategory},{log.ExpensePaymentType},{log.ExpenseVendor},{log.ExpenseAmt}";
                    writer.WriteLine(line);
                }
            }
        }

        public static void UpdateExpensesPendingObject(ExpensesCustom expensesCustom)
        {
            if (expensesCustom != null)
            {
                if (expensesCustom.Daily15MinLogId.ToLower().Contains("-food-"))
                    expensesCustom.ExpenseCategory = "Food";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-bills-"))
                    expensesCustom.ExpenseCategory = "Bills";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-travel-"))
                    expensesCustom.ExpenseCategory = "Travel";

                var subCategoryMappings = new Dictionary<string, string>
                {
                    { "-eatoutside-", "EatOutside" },
                    { "-grocery-", "Grocery" },
                    { "-local-", "Local" },
                    { "-entertainment-", "Entertainment" },
                    { "-intercity-", "InterCity" },
                    { "-rent-", "Rent" },
                    { "-medicines-", "Medicines" },
                    { "-electricity-", "Electricity" },
                    { "-internet-", "Internet" },
                    { "-water-", "Water" },
                    { "-mobile-", "Mobile" },
                    { "-gas-", "Gas" },
                    { "-fuel-", "Fuel" },
                    { "-storage-", "Storage" },
                    { "-other-", "Other" }
                };

                foreach (var mapping in subCategoryMappings)
                {
                    if (expensesCustom.Daily15MinLogId.ToLower().Contains(mapping.Key))
                    {
                        expensesCustom.ExpenseSubCategory = mapping.Value;
                        break;
                    }
                }



                if (expensesCustom.Daily15MinLogId.ToLower().Contains("-paytmicici-"))
                    expensesCustom.ExpensePaymentType = "PaytmICICI";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-paytm-"))
                    expensesCustom.ExpensePaymentType = "Paytm";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-rajanigpay-"))
                    expensesCustom.ExpensePaymentType = "RajaniGPAY";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-nagendraciti-"))
                    expensesCustom.ExpensePaymentType = "NagendraCITI";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-metrocard-"))
                    expensesCustom.ExpensePaymentType = "MetroCard";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-cash-"))
                    expensesCustom.ExpensePaymentType = "Cash";

                if (expensesCustom.Daily15MinLogId.ToLower().Contains("-taazakitchen-"))
                    expensesCustom.ExpenseVendor = "TaazaKitchen";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-swiggy-"))
                    expensesCustom.ExpenseVendor = "Swiggy";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("rapido"))
                    expensesCustom.ExpenseVendor = "Rapido";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("nammayatri"))
                    expensesCustom.ExpenseVendor = "NammaYatri";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("akshayakalpa"))
                    expensesCustom.ExpenseVendor = "AkshayaKalpa";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("hydmetro"))
                    expensesCustom.ExpenseVendor = "HydMetro";
                else if (expensesCustom.Daily15MinLogId.ToLower().Contains("CreatingLinks"))
                    expensesCustom.ExpenseVendor = "CreatingLinks";


                if (expensesCustom.Daily15MinLogId.ToLower().EndsWith("rs"))
                {
                    var parts = expensesCustom.Daily15MinLogId.Split('-');
                    var lastPart = parts.LastOrDefault();
                    lastPart = lastPart?.Trim().ToLower();
                    if (lastPart != null && lastPart.EndsWith("rs"))
                    {
                        var amountString = lastPart.Substring(0, lastPart.Length - 2); // Remove "rs"
                        if (decimal.TryParse(amountString, out var amount))
                        {
                            expensesCustom.ExpenseAmt = amount;
                        }
                    }
                }
            }
        }
    }
}