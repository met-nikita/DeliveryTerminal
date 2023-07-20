using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NuGet.Protocol.Plugins;
using DeliveryTerminal.Data;
using DeliveryTerminal.Helpers;
using DeliveryTerminal.Models;
using DeliveryTerminal.Models.Enums;
using X.PagedList;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Client")]
    public class ReturnsForClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReturnsForClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Returns
        public async Task<IActionResult> Index(long? clientID, DateTime? dateBegin, DateTime? dateEnd, ReturnStatus? returnStatus, int? page)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var returnsRecords = _context.Return.Include(r => r.Client).Include(r => r.Supplier).OrderByDescending(r => r.SendDate).AsQueryable();
            returnsRecords = returnsRecords.Where(s => s.Client != null && s.Client.UserId == userId);
            if (clientID.HasValue)
            {
                returnsRecords = returnsRecords.Where(s => s.Client != null && s.Client.Id == clientID);
            }
            if (dateBegin.HasValue && dateEnd.HasValue)
            {
                returnsRecords = returnsRecords.Where(s => s.SendDate >= dateBegin && s.SendDate <= dateEnd);
                ViewData["DateBegin"] = dateBegin.Value.ToString("yyyy-MM-dd");
                ViewData["DateEnd"] = dateEnd.Value.ToString("yyyy-MM-dd");
            }
            if (returnStatus.HasValue)
            {
                switch (returnStatus)
                {
                    case ReturnStatus.Sent:
                        returnsRecords = returnsRecords.Where(s => s.ReceiveDate == null && s.ReturnDate == null);
                        break;
                    case ReturnStatus.Received:
                        returnsRecords = returnsRecords.Where(s => s.ReceiveDate != null && s.ReturnDate == null);
                        break;
                    case ReturnStatus.Returned:
                        returnsRecords = returnsRecords.Where(s => s.ReceiveDate != null && s.ReturnDate != null);
                        break;
                }
                ViewData["ReturnStatus"] = returnStatus;
            }

            var pageNumber = page ?? 1;

            var list = returnsRecords.ToPagedList(pageNumber, 25);

            ViewData["ClientID"] = new SelectList(_context.Partner.Where(p => p.IsCustomer && p.UserId == userId), "Id", "Name", clientID);

            ViewData["ClientIDSelected"] = clientID;

            foreach (var item in list)
            {
                if (item.ReceiveDate == null && item.ReturnDate == null)
                {
                    ViewData[item.Id.ToString()] = true;
                }
            }

            return View(list);
        }

        public IActionResult ExportExcel(long? clientID, DateTime? dateBegin, DateTime? dateEnd, ReturnStatus? returnStatus, string? columns)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var returnsRecords = _context.Return.Include(r => r.Client).Include(r => r.Supplier).OrderByDescending(r => r.SendDate).AsQueryable();
            returnsRecords = returnsRecords.Where(s => s.Client != null && s.Client.UserId == userId);
            if (clientID.HasValue)
            {
                returnsRecords = returnsRecords.Where(s => s.Client != null && s.Client.Id == clientID);
            }
            if (dateBegin.HasValue && dateEnd.HasValue)
            {
                returnsRecords = returnsRecords.Where(s => s.SendDate >= dateBegin && s.SendDate <= dateEnd);
                ViewData["DateBegin"] = dateBegin.Value.ToString("yyyy-MM-dd");
                ViewData["DateEnd"] = dateEnd.Value.ToString("yyyy-MM-dd");
            }
            if (returnStatus.HasValue)
            {
                switch (returnStatus)
                {
                    case ReturnStatus.Sent:
                        returnsRecords = returnsRecords.Where(s => s.ReceiveDate == null && s.ReturnDate == null);
                        break;
                    case ReturnStatus.Received:
                        returnsRecords = returnsRecords.Where(s => s.ReceiveDate != null && s.ReturnDate == null);
                        break;
                    case ReturnStatus.Returned:
                        returnsRecords = returnsRecords.Where(s => s.ReceiveDate != null && s.ReturnDate != null);
                        break;
                }
                ViewData["ReturnStatus"] = returnStatus;
            }
            var returns = returnsRecords.ToList();
            long tempid = DateTime.Now.ToFileTime();
            string path = Path.Combine(Path.GetTempPath(), "export_" + tempid + ".xlsx");
            DeliveryTerminalHelper.ExportReturns(returns, columns, path);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = "export_" + tempid + ".xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        // GET: Returns/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            if (id == null || _context.Return == null)
            {
                return NotFound();
            }

            var @return = await _context.Return
                .Include(r => r.Client)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id && m.Client.UserId == userId);
            if (@return == null)
            {
                return NotFound();
            }

            return View(@return);
        }

        // GET: Returns/Create
        public IActionResult Create()
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            ViewData["ClientId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer && p.UserId == userId), "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Partner.Include(p=>p.PartnersAssigned).Where(p => p.PartnersAssigned.Any(p => p.AssignedPartner.UserId==userId)), "Id", "Name");
            return View(new ReturnViewModel());
        }

        // POST: Returns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReturnViewModel returnVM)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!await _context.Partner.AnyAsync(p => p.Id == returnVM.ClientId && p.UserId == userId))
                    return NotFound();
                if (!await _context.Partner.Include(p=>p.PartnersAssigned).AnyAsync(p => p.Id == returnVM.SupplierId && p.PartnersAssigned.Any(p => p.AssignedPartnerId == returnVM.ClientId)))
                    return NotFound();
                var @return = new Return();
                @return.ClientId = returnVM.ClientId;
                @return.SupplierId = returnVM.SupplierId;
                @return.SendDate = returnVM.SendDate;
                @return.Count = returnVM.Count;
                @return.Notes = returnVM.Notes;
                _context.Add(@return);
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer && p.UserId == userId), "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Partner.Include(p => p.PartnersAssigned).Where(p => p.PartnersAssigned.Any(p => p.AssignedPartner.UserId == userId)), "Id", "Name");
            return View(returnVM);
        }

        // GET: Returns/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            if (id == null || _context.Return == null)
            {
                return NotFound();
            }

            var @return = await _context.Return
                .Include(r => r.Client)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id && m.Client.UserId == userId && m.ReceiveDate == null && m.ReturnDate == null);
            if (@return == null)
            {
                return NotFound();
            }

            return View(@return);
        }

        // POST: Returns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            string userId = User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            if (_context.Return == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Return'  is null.");
            }
            var @return = await _context.Return
                .Include(r => r.Client)
                .FirstOrDefaultAsync(m => m.Id == id && m.Client.UserId == userId && m.ReceiveDate == null && m.ReturnDate == null);
            if (@return != null)
            {
                _context.Return.Remove(@return);
            }

            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return RedirectToAction(nameof(Index));
        }

        private bool ReturnExists(long id)
        {
            return (_context.Return?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
