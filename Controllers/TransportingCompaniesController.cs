using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Irony.Parsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee")]
    public class TransportingCompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportingCompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransportingCompanies
        public async Task<IActionResult> Index()
        {
              return _context.TransportingCompany != null ? 
                          View(await _context.TransportingCompany.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.TransportingCompany'  is null.");
        }

        // GET: TransportingCompanies/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.TransportingCompany == null)
            {
                return NotFound();
            }

            var transportingCompany = await _context.TransportingCompany
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transportingCompany == null)
            {
                return NotFound();
            }

            return View(transportingCompany);
        }

        // GET: TransportingCompanies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TransportingCompanies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,TaxID,RegID,BankName,RS,KS,BIK,DefaultTC")] TransportingCompany transportingCompany)
        {
            if (ModelState.IsValid)
            {
                if(!transportingCompany.DefaultTC && !_context.TransportingCompany.Any(t => t.DefaultTC))
                    transportingCompany.DefaultTC = true;
                _context.Add(transportingCompany);
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Index));
            }
            return View(transportingCompany);
        }

        // GET: TransportingCompanies/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.TransportingCompany == null)
            {
                return NotFound();
            }

            var transportingCompany = await _context.TransportingCompany.FindAsync(id);
            if (transportingCompany == null)
            {
                return NotFound();
            }
            return View(transportingCompany);
        }

        // POST: TransportingCompanies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,TaxID,RegID,BankName,RS,KS,BIK,DefaultTC")] TransportingCompany transportingCompany)
        {
            if (id != transportingCompany.Id)
            {
                return NotFound();
            }

            TransportingCompany transportingCompanyDB = await _context.TransportingCompany.FirstOrDefaultAsync(p => p.Id == id);
            if (transportingCompanyDB == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!transportingCompany.DefaultTC && !_context.TransportingCompany.Any(t => t.DefaultTC))
                        transportingCompany.DefaultTC = true;
                    else if (transportingCompany.DefaultTC)
                    {
                        await _context.TransportingCompany.Where(t => t.Id != transportingCompany.Id).ExecuteUpdateAsync(t => t.SetProperty(t => t.DefaultTC, false));
                    }
                    _context.Entry(transportingCompanyDB).CurrentValues.SetValues(transportingCompany);
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value, null);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransportingCompanyExists(transportingCompany.Id))
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
            return View(transportingCompany);
        }

        // GET: TransportingCompanies/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.TransportingCompany == null)
            {
                return NotFound();
            }

            var transportingCompany = await _context.TransportingCompany
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transportingCompany == null)
            {
                return NotFound();
            }

            return View(transportingCompany);
        }

        // POST: TransportingCompanies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.TransportingCompany == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TransportingCompany'  is null.");
            }
            var transportingCompany = await _context.TransportingCompany.FindAsync(id);
            if (transportingCompany != null)
            {
                if (transportingCompany.DefaultTC)
                    return Problem("Нельзя удалить траспортную компанию установленную по-умолчанию");
                _context.TransportingCompany.Remove(transportingCompany);
            }
            
            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return RedirectToAction(nameof(Index));
        }

        private bool TransportingCompanyExists(long id)
        {
          return (_context.TransportingCompany?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
