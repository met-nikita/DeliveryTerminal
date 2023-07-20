using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using DeliveryTerminal.Data;
using DeliveryTerminal.Helpers;
using DeliveryTerminal.Models;
using DeliveryTerminal.Models.Enums;
using X.PagedList;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Client")]
    public class RegistriesForClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistriesForClientController(IMapper mapper, ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Registries
        public async Task<IActionResult> Index(long? receiverID, DateTime? dateBegin, DateTime? dateEnd, DeliveryType? deliveryType, int? page)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }    
            var registryRecords = _context.Registry.Include(r => r.Receiver).Include(r => r.Region).Include(r => r.Sender).OrderByDescending(p => p.ReceiveDate).ThenByDescending(p => p.ReceiveTime).AsQueryable();
            registryRecords = registryRecords.Where(s => s.Receiver != null && s.Receiver.UserId == userId);
            if (receiverID.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.Receiver != null && s.Receiver.Id == receiverID);
            }
            if (dateBegin.HasValue && dateEnd.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.ReceiveDate >= dateBegin && s.ReceiveDate <= dateEnd);
                ViewData["DateBegin"] = dateBegin.Value.ToString("yyyy-MM-dd");
                ViewData["DateEnd"] = dateEnd.Value.ToString("yyyy-MM-dd");
            }
            if (deliveryType.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.DeliveryType == deliveryType);
                ViewData["DeliveryType"] = deliveryType;
            }
            var pageNumber = page ?? 1;

            ViewData["UPDSum"] = registryRecords.Sum(r => r.UPDSum);
            ViewData["RateSum"] = registryRecords.Sum(r => r.Rate);

            ViewData["ReceiverID"] = new SelectList(_context.Partner.Where(p => p.IsCustomer && p.UserId == userId), "Id", "Name", receiverID);
            ViewData["ReceiverIDSelected"] = receiverID;

            var list = registryRecords.ToPagedList(pageNumber, 25);

            return View(list);
        }

        // GET: Registries/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            if (id == null || _context.Registry == null)
            {
                return NotFound();
            }

            var registry = await _context.Registry
                .Include(r => r.Receiver)
                .Include(r => r.Region)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id && m.Receiver != null && m.Receiver.UserId == userId);
            if (registry == null)
            {
                return NotFound();
            }

            return View(registry);
        }

        // GET: Registries/ExportExcel
        public IActionResult ExportExcel(long? receiverID, DateTime? dateBegin, DateTime? dateEnd, DeliveryType? deliveryType, string? columns)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var registryRecords = _context.Registry.Include(r => r.Receiver).Include(r => r.Region).Include(r => r.Sender).OrderByDescending(p => p.ReceiveDate).ThenByDescending(p => p.ReceiveTime).AsQueryable();
            registryRecords = registryRecords.Where(s => s.Receiver != null && s.Receiver.UserId == userId);
            if (receiverID.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.Receiver != null && s.Receiver.Id == receiverID);
            }
            if (dateBegin.HasValue && dateEnd.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.ReceiveDate >= dateBegin && s.ReceiveDate <= dateEnd);
            }
            if (deliveryType.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.DeliveryType == deliveryType);
            }
            var registry = registryRecords.ToList();
            long tempid = DateTime.Now.ToFileTime();
            string path = Path.Combine(Path.GetTempPath(), "export_" + tempid + ".xlsx");
            DeliveryTerminalHelper.ExportRegistry(registry, columns, path);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = "export_" + tempid + ".xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        [HttpGet]
        public async Task<IActionResult> GetColumns(long id)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            Partner partner = await _context.Partner.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == id);
            if (partner == null)
            {
                return NotFound();
            }
            return Content(partner.ColumnsFilter);
        }
    }
}
