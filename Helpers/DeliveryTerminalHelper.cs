using ClosedXML.Excel;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;
using DeliveryTerminal.Models.Enums;

namespace DeliveryTerminal.Helpers
{
    public static class DeliveryTerminalHelper
    {
        public static void CalculateRate(Registry registry, Tariff tariff)
        {
            decimal rate = 0;
            if (registry.DeliveryType == DeliveryType.CrossDock)
            {
                rate += registry.UPDSum * (tariff.TaficcCross / 100);
            }
            else
            {
                rate += registry.Weight.Value * tariff.TaficcExp;
            }
            if (registry.PackagingLoc == PackagingLoc.City)
            {
                rate += tariff.Addition;
            }
            registry.Rate = rate;
        }

        public static void SubstituteIDs(ApplicationDbContext context, Dictionary<string, object> dict)
        {
            foreach (var keyValuePair in dict)
            {
                if (keyValuePair.Key == "RegionId")
                {
                    var region = context.Region.FirstOrDefault(r => r.Id == (long)keyValuePair.Value);
                    if (region != null)
                    {
                        dict[keyValuePair.Key] = region.Name;
                    }
                }
                else if (keyValuePair.Key == "SenderId" || keyValuePair.Key == "ReceiverId")
                {
                    var partner = context.Partner.FirstOrDefault(p => p.Id == (long)keyValuePair.Value);
                    if (partner != null)
                    {
                        dict[keyValuePair.Key] = partner.Name;
                    }
                }
                else if (keyValuePair.Key == "ReceiveDate" || keyValuePair.Key == "UPDDate" || keyValuePair.Key == "ExpDate")
                {
                    if (keyValuePair.Value != null)
                    {
                        dict[keyValuePair.Key] = ((DateTime)keyValuePair.Value).ToString("dd.MM.yyyy");
                    }
                }
                else if (keyValuePair.Key == "ReceiveTime")
                {
                    if (keyValuePair.Value != null)
                    {
                        dict[keyValuePair.Key] = ((DateTime)keyValuePair.Value).ToString("HH:mm");
                    }
                }
            }
        }

        public static void ExportRegistry(List<Registry> registry, string columns, string path)
        {
            if (String.IsNullOrEmpty(columns))
                columns = "DeliveryType;ReceiverId;ReceiveDate;ReceiveTime;SenderId;RegionId;CountPallets;CountBoxes;CountOversized;Weight;PackagingLoc;Driver;OwnPallets;Notes;UPDID;UPDDate;UPDSum;ExpID;ExpDate;Rate";
            ExcelExporter<Registry> excelExporter = new ExcelExporter<Registry>(columns, "Реестр");
            excelExporter.AddCustomColumn("ReceiveDateTime", "Дата и время поступления", r => r.ReceiveDate + r.ReceiveTime.TimeOfDay, false);
            excelExporter.AddCustomColumn("ReceiverId", "Грузополучатель", r => r.Receiver.Name, false);
            excelExporter.AddCustomColumn("SenderId", "Поставщик", r => r.Sender.Name, false);
            excelExporter.AddCustomColumn("RegionId", "Регион", r => r.Region.Name, false);
            excelExporter.AddCustomColumn("UPDSum", "Сумма УПД", r => r.UPDSum, true);
            excelExporter.AddCustomColumn("Rate", "Ставка", r => r.Rate, true);
            excelExporter.AddCustomColumn("ReceiveTime", "Время поступления", r => r.ReceiveTime.TimeOfDay, false);
            excelExporter.AddCustomColumn("DeliveryType", "Тип поставки", r => r.DeliveryType == DeliveryType.CrossDock ? "Кросс-докинг" : "Стандартная поставка", false);
            excelExporter.ProduceExcelFile(registry, path);
        }

        public static void ExportReturns(List<Return> returns, string columns, string path)
        {
            if (String.IsNullOrEmpty(columns))
                columns = "ClientId;SupplierId;SendDate;Count;Notes;ReceiveDate;ReturnDate;RepName";
            ExcelExporter<Return> excelExporter = new ExcelExporter<Return>(columns, "Возвраты");
            excelExporter.AddCustomColumn("ClientId", "Клиент", r => r.Client.Name, false);
            excelExporter.AddCustomColumn("SupplierId", "Поставщик", r => r.Supplier.Name, false);
            excelExporter.ProduceExcelFile(returns, path);
        }
    }
}
