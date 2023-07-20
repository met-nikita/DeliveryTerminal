using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee")]
    public class RegionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Regions
        public async Task<IActionResult> Index()
        {
              return _context.Region != null ? 
                          View(await _context.Region.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Region'  is null.");
        }

        // GET: Regions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Regions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Add(region);
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Index));
            }
            return View(region);
        }

        // GET: Regions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Region == null)
            {
                return NotFound();
            }

            var region = await _context.Region.FindAsync(id);
            if (region == null)
            {
                return NotFound();
            }
            return View(region);
        }

        // POST: Regions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name")] Region region)
        {
            if (id != region.Id)
            {
                return NotFound();
            }

            Region regionDB = await _context.Region.FirstOrDefaultAsync(p => p.Id == id);
            if (regionDB == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(regionDB).CurrentValues.SetValues(region);
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegionExists(region.Id))
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
            return View(region);
        }

        // GET: Regions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Region == null)
            {
                return NotFound();
            }

            var region = await _context.Region
                .FirstOrDefaultAsync(m => m.Id == id);
            if (region == null)
            {
                return NotFound();
            }

            return View(region);
        }

        // POST: Regions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Region == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Region'  is null.");
            }
            var region = await _context.Region.FindAsync(id);
            if (region != null)
            {
                _context.Partner.Where(v => v.RegionId == region.Id).Load();
                _context.Region.Remove(region);
            }
            
            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return RedirectToAction(nameof(Index));
        }

        private bool RegionExists(long id)
        {
          return (_context.Region?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
