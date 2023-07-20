using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
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
    [Authorize(Roles = "Administrator,Employee")]
    public class ReturnsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReturnsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Returns
        public async Task<IActionResult> Index(long? clientID, DateTime? dateBegin, DateTime? dateEnd, ReturnStatus? returnStatus, int? page)
        {
            var returnsRecords = _context.Return.Include(r => r.Client).Include(r => r.Supplier).OrderByDescending(r => r.SendDate).AsQueryable();
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
                switch(returnStatus)
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

            ViewData["ClientID"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name", clientID);

            ViewData["ClientIDSelected"] = clientID;

            var list = returnsRecords.ToPagedList(pageNumber, 25);

            foreach (var item in list)
            {
                if (item.ReceiveDate == null)
                {
                    ViewData[item.Id.ToString()] = true;
                }
            }

            return View(list);
        }

        public IActionResult ExportExcel(long? clientID, DateTime? dateBegin, DateTime? dateEnd, ReturnStatus? returnStatus, string? columns)
        {
            var returnsRecords = _context.Return.Include(r => r.Client).Include(r => r.Supplier).OrderByDescending(r => r.SendDate).AsQueryable();
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

        [HttpGet]
        public async Task<IActionResult> Receive(long? id)
        {
            if (id == null || _context.Return == null)
            {
                return NotFound();
            }

            var @return = await _context.Return
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@return == null)
            {
                return NotFound();
            }
            if(@return.ReceiveDate !=  null)
            {
                return NotFound();
            }
            @return.ReceiveDate = DateTime.Today;
            _context.Entry(@return).CurrentValues.SetValues(@return);
            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Ok();
        }

        // GET: Returns/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Return == null)
            {
                return NotFound();
            }

            var @return = await _context.Return
                .Include(r => r.Client)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@return == null)
            {
                return NotFound();
            }

            return View(@return);
        }

        // GET: Returns/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Partner.Where(p=>p.IsCustomer), "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View();
        }

        // POST: Returns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,SupplierId,SendDate,Count,Notes,ReceiveDate,ReturnDate,RepName")] Return @return)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@return);
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View(@return);
        }

        // GET: Returns/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Return == null)
            {
                return NotFound();
            }

            var @return = await _context.Return.FindAsync(id);
            if (@return == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View(@return);
        }

        // POST: Returns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ClientId,SupplierId,SendDate,Count,Notes,ReceiveDate,ReturnDate,RepName")] Return @return)
        {
            if (id != @return.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var @returnreal = await _context.Return.FirstOrDefaultAsync(r => r.Id == id);
                    _context.Entry(@returnreal).CurrentValues.SetValues(@return);
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReturnExists(@return.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View(@return);
        }

        // GET: Returns/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Return == null)
            {
                return NotFound();
            }

            var @return = await _context.Return
                .Include(r => r.Client)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            if (_context.Return == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Return'  is null.");
            }
            var @return = await _context.Return.FindAsync(id);
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
