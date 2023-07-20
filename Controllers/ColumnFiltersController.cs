using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee")]
    public class ColumnFiltersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ColumnFiltersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetColumns(long receiverID)
        {
            Partner partner = await _context.Partner.FirstOrDefaultAsync(p => p.Id == receiverID);
            if (partner == null)
            {
                return NotFound();
            }
            return Content(partner.ColumnsFilter);
        }

        [HttpPost]
        public async Task<IActionResult> SetColumns(long receiverID, string columns)
        {
            Partner partner = await _context.Partner.FirstOrDefaultAsync(p => p.Id == receiverID);
            if (partner == null)
            {
                return NotFound();
            }
            partner.ColumnsFilter = columns;
            _context.Entry(partner).CurrentValues.SetValues(partner);
            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Ok();
        }
    }
}
