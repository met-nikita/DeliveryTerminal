using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2010.Excel;
using Humanizer;
using Irony.Parsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee")]
    public class TariffsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public TariffsController(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        // GET: Tariffs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tariff.Include(t => t.Partner).Where(t => t.Partner.IsCustomer);
            var list = await applicationDbContext.ToListAsync();
            foreach(var item in list)
            {
                if(await _context.AuditLogs.Where(a => a.TableName == "Tariff" && a.PrimaryKey == "{{\"Id\":{0}}}".FormatWith(item.Id)).AnyAsync())
                {
                    ViewData[item.Id.ToString()] = true;
                }
            }
            return View(list);
        }

        // GET: Tariffs/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Tariff == null)
            {
                return NotFound();
            }

            var tariff = await _context.Tariff
                .Include(t => t.Partner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tariff == null)
            {
                return NotFound();
            }
            TariffViewModel viewModel = _mapper.Map<TariffViewModel>(tariff);
            viewModel.IdVM = tariff.Id;
            viewModel.PartnerNameVM = tariff.Partner.Name;
            return View(viewModel);
        }

        // POST: Tariffs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, TariffViewModel tariffVM)
        {
            if (id != tariffVM.IdVM)
            {
                return NotFound();
            }

            var tariff = await _context.Tariff
                .Include(t => t.Partner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tariff == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _mapper.Map<TariffViewModel, Tariff>(tariffVM, tariff);
                    tariffVM.PartnerNameVM = tariff.Partner.Name;
                    _context.Entry(tariff).CurrentValues.SetValues(tariff);
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value, tariffVM.EditReason);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TariffExists(tariff.Id))
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
            return View(tariffVM);
        }

        private bool TariffExists(long id)
        {
          return (_context.Tariff?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
