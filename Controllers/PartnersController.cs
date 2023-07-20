using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Irony.Parsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee")]
    public class PartnersController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public PartnersController(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        // GET: Partners
        public async Task<IActionResult> Index(string? type)
        {
            List<Partner> list = null;
            if (!String.IsNullOrEmpty(type))
            {
                if(type == "Customer")
                {
                    list = await _context.Partner.Where(p => p.IsCustomer).ToListAsync();
                    ViewData["Selected"] = type;
                }
                else if(type == "Supplier")
                {
                    list = await _context.Partner.Include(p => p.PartnersAssigned).ThenInclude(a => a.AssignedPartner).Where(p => p.IsSupplier).ToListAsync();
                    ViewData["Selected"] = type;
                }
            }
              return list != null ? 
                          View(list) :
                          View(await _context.Partner.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ImportSuppliers(IFormFile postedFile)
        {
            if(postedFile != null)
            {
                string filePath = Path.Combine(Path.GetTempPath(), postedFile.FileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }
                XLWorkbook wb = new XLWorkbook(filePath);
                var worksheet = wb.Worksheets.FirstOrDefault();
                int row = 2;
                while(true)
                {
                    var actualRow = worksheet.Row(row);
                    if (!actualRow.Cell(2).Value.IsText || String.IsNullOrEmpty(actualRow.Cell(2).Value.GetText()))
                        break;
                    Partner partner = new Partner();
                    partner.IsSupplier = true;
                    partner.Name = actualRow.Cell(2).Value.ToString();
                    partner.TaxID = actualRow.Cell(3).Value.ToString();
                    if (await _context.Partner.AnyAsync(p => p.TaxID == partner.TaxID))
                    {
                        row++;
                        continue;
                    };
                    partner.RegID = actualRow.Cell(4).Value.ToString();
                    partner.Email = actualRow.Cell(5).Value.ToString();
                    partner.Address = actualRow.Cell(6).Value.ToString();
                    partner.ContactName = actualRow.Cell(7).Value.ToString();
                    partner.ContactPhone = actualRow.Cell(8).Value.ToString();
                    string region = actualRow.Cell(9).Value.ToString();
                    if (region.Length > 0)
                    {
                        Region regionReal = await _context.Region.FirstOrDefaultAsync(r => r.Name == region);
                        if (regionReal == null)
                        {
                            regionReal = new Region();
                            regionReal.Name = region;
                            _context.Add(regionReal);
                        }
                        partner.Region = regionReal;
                    }
                    string customer = actualRow.Cell(10).Value.ToString();
                    if (customer.Length > 0)
                    {
                        Partner partnerReal = await _context.Partner.FirstOrDefaultAsync(r => r.Name == customer);
                        if (partnerReal != null)
                        {
                            PartnerAssignment partnerAssignment = new();
                            partnerAssignment.Partner = partner;
                            partnerAssignment.AssignedPartner = partnerReal;
                            partner.PartnersAssigned.Add(partnerAssignment);
                            _context.Add(partnerAssignment);
                        }
                    }
                    Tariff tariff = new Tariff();
                    tariff.Partner = partner;
                    partner.Tariff = tariff;
                    _context.Add(tariff);
                    _context.Add(partner);
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                    row++;
                }
            }
            return RedirectToAction("Index", new { type = "Supplier" });
        }

        // GET: Partners/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Partner == null)
            {
                return NotFound();
            }

            var partner = await _context.Partner
                .Include(p => p.Region).Include(p => p.TransportingCompany).FirstOrDefaultAsync(m => m.Id == id);
            if (partner == null)
            {
                return NotFound();
            }

            return View(partner);
        }

        // GET: Partners/Create
        public IActionResult Create()
        {
            PartnerViewModel partnerVW = new();
            foreach (Partner partner in _context.Partner.Where(p => p.IsCustomer).ToList())
            {
                partnerVW.AllPartnersNames[partner.Id] = partner.Name;
                partnerVW.AllPartnersSelection[partner.Id] = false;
            }
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["TransportingCompanyId"] = new SelectList(_context.TransportingCompany, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View(partnerVW);
        }

        // POST: Partners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartnerViewModel partnerVW)
        {
            if (ModelState.IsValid)
            {
                Partner partner = _mapper.Map<Partner>(partnerVW);
                foreach (long id in partnerVW.AllPartnersSelection.Where(p => p.Value).ToDictionary(i => i.Key, i => i.Value).Keys)
                {
                    Partner assigned = _context.Partner.Find(id);
                    if (assigned != null)
                    {
                        PartnerAssignment partnerAssignment = new();
                        partnerAssignment.Partner = partner;
                        partnerAssignment.AssignedPartner = assigned;
                        partner.PartnersAssigned.Add(partnerAssignment);
                        _context.Add(partnerAssignment);
                    }
                }
                Tariff tariff = new Tariff();
                tariff.TaficcCross = partnerVW.TaficcCross;
                tariff.TaficcExp = partnerVW.TaficcExp;
                tariff.Addition = partnerVW.Addition;
                tariff.Partner = partner;
                partner.Tariff = tariff;
                _context.Add(partner);
                _context.Add(tariff);
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Index));
            }
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["TransportingCompanyId"] = new SelectList(_context.TransportingCompany, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View(partnerVW);
        }

        // GET: Partners/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Partner == null)
            {
                return NotFound();
            }

            var partner = await _context.Partner.FindAsync(id);
            
            if (partner == null)
            {
                return NotFound();
            }
            PartnerViewModel partnerVW = _mapper.Map<PartnerViewModel>(partner);
            partnerVW.IdVM = partner.Id;
            List<PartnerAssignment> assignments = await _context.PartnerAssignment.Where(p => p.PartnerId == partner.Id).ToListAsync();
            foreach (Partner assigned in await _context.Partner.Where(p => p.IsCustomer).ToListAsync())
            {
                if (assigned.Id == partner.Id) continue;
                partnerVW.AllPartnersNames[assigned.Id] = assigned.Name;
                partnerVW.AllPartnersSelection[assigned.Id] = assignments.Any(p => p.AssignedPartnerId == assigned.Id);
            }
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["TransportingCompanyId"] = new SelectList(_context.TransportingCompany, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View(partnerVW);
        }

        // POST: Partners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, PartnerViewModel partnerVW)
        {
            if (id != partnerVW.IdVM)
            {
                return NotFound();
            }

            Partner partner = await _context.Partner.FirstOrDefaultAsync(p => p.Id == id);
            if (partner == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _mapper.Map(partnerVW,partner);
                    partner.Id = id;
                    List<PartnerAssignment> assignments = await _context.PartnerAssignment.Where(p => p.PartnerId == partner.Id).ToListAsync();
                    foreach (long id1 in partnerVW.AllPartnersSelection.Keys)
                    {
                        if(id1==id) continue;
                        Partner assigned = _context.Partner.Find(id1);
                        if (assigned != null)
                        {
                            if (partnerVW.AllPartnersSelection[id1])
                            {
                                if (assignments.Any(p => p.AssignedPartnerId == assigned.Id)) continue;
                                PartnerAssignment partnerAssignment = new();
                                partnerAssignment.PartnerId = partner.Id;
                                partnerAssignment.AssignedPartnerId = assigned.Id;
                                _context.Add(partnerAssignment);
                            }
                            else
                            {
                                if (!assignments.Any(p => p.AssignedPartnerId == assigned.Id)) continue;
                                PartnerAssignment partnerAssignment = assignments.Find(p => p.AssignedPartnerId == assigned.Id);
                                _context.Remove(partnerAssignment);
                            }
                        }
                    }
                    _context.Entry(partner).CurrentValues.SetValues(partner);
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartnerExists(partnerVW.IdVM))
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
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            ViewData["TransportingCompanyId"] = new SelectList(_context.TransportingCompany, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View(partnerVW);
        }

        // GET: Partners/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Partner == null)
            {
                return NotFound();
            }

            var partner = await _context.Partner.Include(p => p.Region).Include(p => p.TransportingCompany)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (partner == null)
            {
                return NotFound();
            }

            return View(partner);
        }

        // POST: Partners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Partner == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Partner'  is null.");
            }
            var partner = await _context.Partner.FindAsync(id);
            if (partner != null)
            {
                _context.PartnerAssignment.RemoveRange(_context.PartnerAssignment.Where(p => p.PartnerId == id || p.AssignedPartnerId == id));
                _context.Partner.Remove(partner);
            }
            
            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return RedirectToAction(nameof(Index));
        }

        private bool PartnerExists(long id)
        {
          return (_context.Partner?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET: Partners/Assignment/5
        public async Task<IActionResult> Assignment(long? id)
        {
            if (id == null || _context.Partner == null)
            {
                return NotFound();
            }

            var partner = await _context.Partner.FindAsync(id);

            if (partner == null)
            {
                return NotFound();
            }
            PartnerViewModel partnerVW = _mapper.Map<PartnerViewModel>(partner);
            partnerVW.IdVM = partner.Id;
            List<PartnerAssignment> assignments = await _context.PartnerAssignment.Include(p => p.AssignedPartner).Where(p => p.PartnerId == partner.Id).ToListAsync();
            PartnerAssignmentViewModel assignmentViewModel = new ();
            assignmentViewModel.PartnerNameVM = partner.Name;
            assignmentViewModel.IdVM = partner.Id;
            foreach (PartnerAssignment assignment in assignments)
            {
                PartnerAssignmentViewModelEntry partnerAssignmentViewModelEntry = _mapper.Map<PartnerAssignmentViewModelEntry>(assignment);
                partnerAssignmentViewModelEntry.IdVM = assignment.Id;
                partnerAssignmentViewModelEntry.IdAsVM = assignment.AssignedPartnerId;
                partnerAssignmentViewModelEntry.NameVM = assignment.AssignedPartner.Name;
                assignmentViewModel.Entries.Add(partnerAssignmentViewModelEntry);
            }
            return View(assignmentViewModel);
        }

        // POST: Partners/Assignment/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assignment(long id, PartnerAssignmentViewModel partnerVW)
        {
            if (id != partnerVW.IdVM)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    foreach(var assignmentVM in partnerVW.Entries)
                    {
                        var assignment = await _context.PartnerAssignment.FirstOrDefaultAsync(a => a.Id == assignmentVM.IdVM);
                        if (assignment == null || assignment.PartnerId != id)
                        {
                            continue;
                        }
                        _mapper.Map<PartnerAssignmentViewModelEntry, PartnerAssignment>(assignmentVM, assignment);
                        _context.Entry(assignment).CurrentValues.SetValues(assignment);
                    }
                    await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartnerExists(partnerVW.IdVM))
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
            return View(partnerVW);
        }
    }
}
