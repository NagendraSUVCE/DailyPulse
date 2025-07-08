using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

public static class DataTableConverter
{
    public static DataTable ToDataTable<T>(List<T> items)
    {
        DataTable dataTable = new DataTable(typeof(T).Name);

        // Get all the properties
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Create columns in the DataTable
        foreach (PropertyInfo prop in properties)
        {
            Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            dataTable.Columns.Add(prop.Name, propType);
        }

        // Add rows to the DataTable
        foreach (T item in items)
        {
            var values = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(item, null);
            }
            dataTable.Rows.Add(values);
        }

        return dataTable;
    }
    
    public static void DataTableToCsv(DataTable dataTable, string baseFolder, string fileName, bool overwriteAllContents)
    {
        string filePath = System.IO.Path.Combine(baseFolder, fileName);

        // Ensure the directory exists
        if (!string.IsNullOrWhiteSpace(baseFolder))
        {
            System.IO.Directory.CreateDirectory(baseFolder);
        }

        // Open the file for writing
        using (var writer = new System.IO.StreamWriter(filePath, !overwriteAllContents))
        {
            // Write headers if overwriting or if the file is empty
            if (overwriteAllContents || new System.IO.FileInfo(filePath).Length == 0)
            {
                string headerLine = string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName));
                writer.WriteLine(headerLine);
            }

            // Write rows
            foreach (DataRow row in dataTable.Rows)
            {
                string rowLine = string.Join(",", row.ItemArray.Select(item => item?.ToString() ?? string.Empty));
                writer.WriteLine(rowLine);
            }
        }
    }
}
