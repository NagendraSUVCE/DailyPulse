using ExcelDataReader;
using Models.DailyLog;
using Models.Finance;
using System.Data;
// Removed invalid namespace reference
using System.IO;
using System.Globalization;

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

       

        
    }
}