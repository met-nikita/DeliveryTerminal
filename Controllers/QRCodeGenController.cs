using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeliveryTerminal.Data;
using DeliveryTerminal.Models;
using Net.Codecrete.QrCodeGenerator;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee")]
    public class QRCodeGenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QRCodeGenController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var viewModel = new QRCodeGenViewModel
            {
                Senders = _context.Partner.Where(p => p.IsSupplier).ToList(),
                Receivers = _context.Partner.Where(p => p.IsCustomer).ToList()
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(QRCodeGenViewModel viewModel)
        {
            List<PartnerAssignment> assignments = _context.PartnerAssignment.Where(p => p.PartnerId == viewModel.Sender && p.AssignedPartnerId == viewModel.Receiver).ToList();
            if(!assignments.Any())
            {
                return RedirectToAction(nameof(Index),viewModel);
            }
            viewModel.SenderName = _context.Partner.First(p => p.Id == viewModel.Sender).Name;
            viewModel.ReceiverName = _context.Partner.First(p => p.Id == viewModel.Receiver).Name;
            var qr = QrCode.EncodeText(assignments.First().Id.ToString(), QrCode.Ecc.Medium);
            viewModel.Svg = qr.ToGraphicsPath();
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Create(long sender, long receiver)
        {
            List<PartnerAssignment> assignments = _context.PartnerAssignment.Where(p => p.PartnerId == sender && p.AssignedPartnerId == receiver).ToList();
            if (!assignments.Any())
            {
                return RedirectToAction(nameof(Index));
            }
            QRCodeGenViewModel viewModel = new QRCodeGenViewModel();
            viewModel.SenderName = _context.Partner.First(p => p.Id == sender).Name;
            viewModel.ReceiverName = _context.Partner.First(p => p.Id == receiver).Name;
            var qr = QrCode.EncodeText(assignments.First().Id.ToString(), QrCode.Ecc.Medium);
            viewModel.Svg = qr.ToGraphicsPath();
            return View(viewModel);
        }
    }
}
