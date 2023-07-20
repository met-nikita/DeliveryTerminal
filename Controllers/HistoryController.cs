using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;
using Newtonsoft.Json;
using NuGet.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using DeliveryTerminal.Models.Enums;
using DeliveryTerminal.Helpers;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee")]
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string table, long id)
        {
            Type type = null;
            switch (table)
            {
                case "Tariff":
                    type = typeof(Tariff);
                    break;
                case "Registry":
                    type= typeof(Registry);
                    break;
                default:
                    return NotFound();
            }
            var history = _context.AuditLogs.Where(a => a.TableName == table && a.Type == AuditType.Update.ToString() && a.PrimaryKey == "{{\"Id\":{0}}}".FormatWith(id)).ToList();
            HistoryViewModel viewModel = new HistoryViewModel();
            if (!history.Any())
            {
                viewModel.Error = "Нет истории изменений для этого объекта";
                return View(viewModel);
            }
            HashSet<string> columnsSet = new HashSet<string>();
            switch (table)
            {
                case "Tariff":
                    viewModel.Table = "Тариф";
                    break;
                case "Registry":
                    viewModel.Table = "Запись в реестре";
                    break;
                default:
                    break;
            }
            viewModel.Id = id.ToString();
            foreach (var key in history)
            {
                var columns = JsonConvert.DeserializeObject<List<string>>(key.AffectedColumns);
                columnsSet.AddRange(columns);
                HistoryEntry historyEntry = new HistoryEntry();
                string userName = "Пользователь удален";
                if(!String.IsNullOrEmpty(key.UserId))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == key.UserId);
                    if(user != null)
                    {
                        userName = user.UserName;
                    }
                }
                historyEntry.UserName = userName;
                historyEntry.Reason = key.Reason;
                historyEntry.DateTime = key.DateTime;
                historyEntry.OldValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(key.OldValues);
                DeliveryTerminalHelper.SubstituteIDs(_context, historyEntry.OldValues);
                historyEntry.NewValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(key.NewValues);
                DeliveryTerminalHelper.SubstituteIDs(_context, historyEntry.NewValues);
                viewModel.HistoryEntries.Add(historyEntry);
            }
            Dictionary<string, string> columnsNames = new();
            foreach(var key in columnsSet)
            {
                MemberInfo property = type.GetProperty(key);
                if (property != null)
                {
                    var attribute = property.GetCustomAttributes(typeof(DisplayAttribute), true)
                        .Cast<DisplayAttribute>().Single();
                    columnsNames[key] = attribute.Name;
                }
                else
                {
                    columnsNames[key] = key;
                }
            }
            viewModel.Columns = columnsNames;
            return View(viewModel);
        }
    }
}
