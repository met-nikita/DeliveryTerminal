using ClosedXML.Excel;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DeliveryTerminal.Models.Enums;

namespace DeliveryTerminal.Helpers
{
    public class CustomColumn<T>
    {
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public Func<T, object> ValueExpression { get; set; }
        public bool Aggregate { get; set; }
    }
    public class ExcelExporter<T>
    {
        private List<string> Columns = new List<string>();
        private List<CustomColumn<T>> CustomColumns = new List<CustomColumn<T>>();
        private string Name;
        public ExcelExporter(string columns, string name)
        {
            Columns.AddRange(columns.Split(';'));
            Name = name;
        }
        public void AddCustomColumn(string name, string friendlyName,  Func<T, object> valueExpression, bool aggregate)
        {
            CustomColumns.Add(new CustomColumn<T>() { Name = name, FriendlyName = friendlyName, ValueExpression = valueExpression, Aggregate = aggregate });
        }
        public void ProduceExcelFile(List<T> items, string fileName)
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet(Name);
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (CustomColumns.Any(c => c.Name == Columns[i]))
                    {
                        string friendlyName = CustomColumns.First(c => c.Name == Columns[i]).FriendlyName;
                        worksheet.Row(1).Cell(i + 1).Value = friendlyName;
                    }
                    else
                    {
                        MemberInfo property = typeof(T).GetProperty(Columns[i]);
                        if (property != null)
                        {
                            var attribute = property.GetCustomAttributes(typeof(DisplayAttribute), true)
                                .Cast<DisplayAttribute>().Single();
                            worksheet.Row(1).Cell(i + 1).Value = attribute.Name;
                        }
                    }
                }
                int row = 2;
                foreach (var item in items)
                {
                    for (int i = 0; i < Columns.Count; i++)
                    {
                        object value = null;
                        if (CustomColumns.Any(c => c.Name == Columns[i]))
                        {
                            Func<T, object> valueExpression = CustomColumns.First(c => c.Name == Columns[i]).ValueExpression;
                            value = valueExpression(item);
                        }
                        else
                        {
                            PropertyInfo property = typeof(T).GetProperty(Columns[i]);
                            if (property != null)
                            {
                                value = property.GetValue(item);
                            }
                        }
                        if (value != null)
                        {
                            if(value.GetType() == typeof(string))
                            {
                                worksheet.Row(row).Cell(i + 1).Value = (string)value;
                            }
                            else if (value.GetType() == typeof(DateTime))
                            {
                                worksheet.Row(row).Cell(i + 1).Value = (DateTime)value;
                            }
                            else if (value.GetType() == typeof(TimeSpan))
                            {
                                worksheet.Row(row).Cell(i + 1).Value = (TimeSpan)value;
                            }
                            else if (value.GetType() == typeof(decimal))
                            {
                                worksheet.Row(row).Cell(i + 1).Value = (decimal)value;
                            }
                            else if (value.GetType() == typeof(int))
                            {
                                worksheet.Row(row).Cell(i + 1).Value = (int)value;
                            }
                        }
                    }
                    row++;
                }
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (CustomColumns.Any(c => c.Name == Columns[i] && c.Aggregate))
                    {
                        worksheet.Row(row).Cell(i + 1).FormulaA1 = "=SUM(" + worksheet.Row(row).Cell(i + 1).WorksheetColumn().ColumnLetter() + "2:" + worksheet.Row(row).Cell(i + 1).WorksheetColumn().ColumnLetter() + (row - 1).ToString() + ")";
                    }
                }
                worksheet.Columns("A", "AZ").AdjustToContents();
                workbook.SaveAs(fileName);
            }
        }
    }
}
