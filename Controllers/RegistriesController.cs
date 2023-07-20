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
    [Authorize(Roles = "Administrator,Employee")]
    public class RegistriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RegistriesController(IMapper mapper, ApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Registries
        public async Task<IActionResult> Index(long? receiverID, DateTime? dateBegin, DateTime? dateEnd, DeliveryType? deliveryType, int? page)
        {
            var registryRecords = _context.Registry.Include(r => r.Receiver).Include(r => r.Region).Include(r => r.Sender).OrderByDescending(p => p.ReceiveDate).ThenByDescending(p => p.ReceiveTime).AsQueryable();
            if (receiverID.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.Receiver != null && s.Receiver.Id == receiverID);
            }
            if(dateBegin.HasValue && dateEnd.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.ReceiveDate >= dateBegin && s.ReceiveDate <= dateEnd);
                ViewData["DateBegin"] = dateBegin.Value.ToString("yyyy-MM-dd");
                ViewData["DateEnd"] = dateEnd.Value.ToString("yyyy-MM-dd");
            }
            if(deliveryType.HasValue)
            {
                registryRecords = registryRecords.Where(s => s.DeliveryType == deliveryType);
                ViewData["DeliveryType"] = deliveryType;
            }
            var pageNumber = page ?? 1;

            ViewData["UPDSum"] = registryRecords.Sum(r => r.UPDSum);
            ViewData["RateSum"] = registryRecords.Sum(r => r.Rate);

            ViewData["ReceiverID"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name", receiverID);

            ViewData["ReceiverIDSelected"] = receiverID;

            var list = registryRecords.ToPagedList(pageNumber, 25);

            foreach (var item in list)
            {
                if (await _context.AuditLogs.Where(a => a.TableName == "Registry" && a.PrimaryKey == "{{\"Id\":{0}}}".FormatWith(item.Id)).AnyAsync())
                {
                    ViewData[item.Id.ToString()] = true;
                }
            }

            return View(list);
        }

        // GET: Registries/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Registry == null)
            {
                return NotFound();
            }

            var registry = await _context.Registry
                .Include(r => r.Receiver)
                .Include(r => r.Region)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registry == null)
            {
                return NotFound();
            }

            return View(registry);
        }

        // GET: Registries/Create
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.Partner.Where(p=> p.IsCustomer), "Id", "Name");
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["SenderId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View();
        }

        // GET: Registries/ExportExcel
        public IActionResult ExportExcel(long? receiverID, DateTime? dateBegin, DateTime? dateEnd, DeliveryType? deliveryType, string? columns)
        {
            var registryRecords = _context.Registry.Include(r => r.Receiver).Include(r => r.Region).Include(r => r.Sender).OrderByDescending(p => p.ReceiveDate).ThenByDescending(p => p.ReceiveTime).AsQueryable();
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

        // POST: Registries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DeliveryType,ReceiverId,ReceiveDate,ReceiveTime,SenderId,RegionId,CountPallets,CountBoxes,CountOversized,Weight,PackagingLoc,Driver,OwnPallets,Notes,UPDID,UPDDate,UPDSum,ExpID,ExpDate,Rate")] Registry registry)
        {
            if (ModelState.IsValid)
            {
                if(registry.Rate == null)
                {
                    Tariff tariff = await _context.Tariff.FirstOrDefaultAsync(t => t.PartnerId == registry.ReceiverId);
                    if (tariff != null)
                    {
                        DeliveryTerminalHelper.CalculateRate(registry, tariff);
                    }
                }
                _context.Add(registry);
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name");
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["SenderId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View(registry);
        }

        // GET: Registries/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Registry == null)
            {
                return NotFound();
            }

            var registry = await _context.Registry.FindAsync(id);
            if (registry == null)
            {
                return NotFound();
            }
            RegistryViewModel registryVM = _mapper.Map<RegistryViewModel>(registry);
            registryVM.IdVM = registry.Id;
            ViewData["ReceiverId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name");
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["SenderId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View(registryVM);
        }

        // POST: Registries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, RegistryViewModel registry)
        {
            if (id != registry.IdVM)
            {
                return NotFound();
            }

            Registry registryDB = await _context.Registry.FirstOrDefaultAsync(p => p.Id == id);
            if (registryDB == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _mapper.Map(registry, registryDB);
                    if (registryDB.Rate == null)
                    {
                        Tariff tariff = await _context.Tariff.FirstOrDefaultAsync(t => t.PartnerId == registry.ReceiverId);
                        if (tariff != null)
                        {
                            DeliveryTerminalHelper.CalculateRate(registryDB, tariff);
                        }
                    }
                    _context.Entry(registryDB).CurrentValues.SetValues(registryDB);
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value, registry.EditReason);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistryExists(registry.IdVM))
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
            ViewData["ReceiverId"] = new SelectList(_context.Partner.Where(p => p.IsCustomer), "Id", "Name");
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["SenderId"] = new SelectList(_context.Partner.Where(p => p.IsSupplier), "Id", "Name");
            return View(registry);
        }

        // GET: Registries/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Registry == null)
            {
                return NotFound();
            }

            var registry = await _context.Registry
                .Include(r => r.Receiver)
                .Include(r => r.Region)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registry == null)
            {
                return NotFound();
            }

            return View(registry);
        }

        // POST: Registries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Registry == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Registry'  is null.");
            }
            var registry = await _context.Registry.FindAsync(id);
            if (registry != null)
            {
                _context.Registry.Remove(registry);
            }
            
            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return RedirectToAction(nameof(Index));
        }

        private bool RegistryExists(long id)
        {
          return (_context.Registry?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
