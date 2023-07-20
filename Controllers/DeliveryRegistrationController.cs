using AutoMapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using DeliveryTerminal.Data;
using DeliveryTerminal.Helpers;
using DeliveryTerminal.Models;
using DeliveryTerminal.Models.Enums;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator,Employee,User")]
    public class DeliveryRegistrationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public DeliveryRegistrationController(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        public IActionResult Index(string? taxID)
        {
            if (!String.IsNullOrEmpty(taxID))
                ViewData["TaxID"] = taxID;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetReceivers(string taxID)
        {
            DeliveryRegistrationReceiverSelectViewModel viewModel = new();
            var sender = await _context.Partner.FirstOrDefaultAsync(p => p.TaxID == taxID && p.IsSupplier);
            if(sender == null)
            {
                viewModel.Error = "Поставщика с указанным ИНН не существует";
                return PartialView(viewModel);
            }
            if(await NeedToWarn(sender.Id))
            {
                return RedirectToAction("CollectReturns", new { id = sender.Id, ajax = true });
            }
            List<PartnerAssignment> assignedPartners = await _context.PartnerAssignment.Include(p => p.AssignedPartner).Where(p => p.PartnerId == sender.Id && (p.CrossDock || p.Expeditor)).ToListAsync();
            if(assignedPartners.Count < 1)
            {
                viewModel.Error = "Нет назначенных грузополучателей";
                return PartialView(viewModel);
            }
            viewModel.IdS = sender.Id;
            viewModel.NameS = sender.Name;
            foreach (var partner in assignedPartners)
            {
                viewModel.AssignedPartnersNames.Add(partner.AssignedPartner.Id, new NameCrossDockExpeditor { Name=partner.AssignedPartner.Name, CrossDock = partner.CrossDock, Expeditor = partner.Expeditor });
            }
            return PartialView(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> CollectReturns(long id, bool ajax = false)
        {
            var sender = await _context.Partner.FirstOrDefaultAsync(p => p.Id == id && p.IsSupplier);
            if (sender == null)
            {
                return NotFound();
            }
            var returns = await _context.Return.Include(r=>r.Client).Where(r => r.SupplierId == sender.Id && r.ReceiveDate != null && r.ReturnDate == null).ToListAsync();
            if(returns.Count<1)
            {
                return NotFound();
            }
            ViewData["SupplierName"] = sender.Name;
            ReturnAcceptViewModel viewModel = new ReturnAcceptViewModel();
            viewModel.SupplierId = sender.Id;
            viewModel.Returns = returns;
            var condition = await _context.SupplierReturn.FirstOrDefaultAsync(s => s.SupplierId == id);
            viewModel.CanSkip = condition == null || condition.WarningCount < 3;
            if (ajax)
                return PartialView(viewModel);
            else
                return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> SkipReturns(long id)
        {
            var sender = await _context.Partner.FirstOrDefaultAsync(p => p.Id == id && p.IsSupplier);
            if (sender == null)
            {
                return NotFound();
            }
            var returns = await _context.Return.Include(r => r.Client).Where(r => r.SupplierId == sender.Id && r.ReceiveDate != null && r.ReturnDate == null).ToListAsync();
            if (returns.Count < 1)
            {
                return NotFound();
            }
            ViewData["SupplierName"] = sender.Name;
            ReturnAcceptViewModel viewModel = new ReturnAcceptViewModel();
            viewModel.SupplierId = sender.Id;
            viewModel.Returns = returns;
            var condition = await _context.SupplierReturn.FirstOrDefaultAsync(s => s.SupplierId == id);
            if(condition == null)
            {
                condition = new SupplierReturn();
                condition.SupplierId = id;
                condition.WarningCount = 1;
                condition.LastWarning = DateTime.Now;
                _context.Add(condition);
            }
            else if (condition.WarningCount>=3)
            {
                return RedirectToAction("Index");
            }
            else
            {
                condition.WarningCount++;
                condition.LastWarning = DateTime.Now;
                _context.Entry(condition).CurrentValues.SetValues(condition);
            }
            await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            return RedirectToAction("Index", new {TaxID = sender.TaxID});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CollectReturns(long id, ReturnAcceptViewModel viewModel)
        {
            if(id != viewModel.SupplierId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var sender = await _context.Partner.FirstOrDefaultAsync(p => p.Id == id && p.IsSupplier);
                if (sender == null)
                {
                    return NotFound();
                }
                var returns = await _context.Return.Include(r => r.Client).Where(r => r.SupplierId == sender.Id && r.ReceiveDate != null && r.ReturnDate == null).ToListAsync();
                if (returns.Count < 1)
                {
                    return NotFound();
                }
                foreach(var item in returns)
                {
                    item.ReturnDate = DateTime.Today;
                    item.RepName = viewModel.RepName;
                    _context.Entry(item).CurrentValues.SetValues(item);
                }
                var condition = await _context.SupplierReturn.FirstOrDefaultAsync(s => s.SupplierId == id);
                if(condition != null)
                {
                    _context.Remove(condition);
                }
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction("Index", new { TaxID = sender.TaxID });
            }
            else
            {
                var sender = await _context.Partner.FirstOrDefaultAsync(p => p.Id == id && p.IsSupplier);
                if (sender == null)
                {
                    return NotFound();
                }
                var returns = await _context.Return.Include(r => r.Client).Where(r => r.SupplierId == sender.Id && r.ReceiveDate != null && r.ReturnDate == null).ToListAsync();
                if (returns.Count < 1)
                {
                    return NotFound();
                }
                ViewData["SupplierName"] = sender.Name;
                viewModel.SupplierId = sender.Id;
                viewModel.Returns = returns;
                var condition = await _context.SupplierReturn.FirstOrDefaultAsync(s => s.SupplierId == id);
                viewModel.CanSkip = condition == null || condition.WarningCount < 3;
                return View(viewModel);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Register(long sender, long receiver, DeliveryType type)
        {
            DeliveryRegistrationViewModel viewModel = new();
            PartnerAssignment assignment = await _context.PartnerAssignment.Include(p=> p.Partner).Include(p=>p.AssignedPartner).FirstOrDefaultAsync(p=>p.PartnerId == sender && p.AssignedPartnerId == receiver && (type==DeliveryType.CrossDock?p.CrossDock:p.Expeditor));
            if (assignment == null)
            {
                viewModel.Error = "Заказчик не назначен поставщику по этому типу поставки";
                return View(viewModel);
            }
            if (await NeedToWarn(sender))
            {
                return RedirectToAction("CollectReturns", new { id = sender });
            }
            viewModel.IdS = assignment.PartnerId;
            viewModel.IdR = assignment.AssignedPartnerId;
            viewModel.NameS = assignment.Partner.Name;
            viewModel.NameR = assignment.AssignedPartner.Name;
            viewModel.DeliveryType = type;
            if (assignment.AssignedPartner.RegionId != null)
            {
                viewModel.RegionId = (long)assignment.AssignedPartner.RegionId;
            }
            if(type == DeliveryType.CrossDock)
                viewModel.ExpRequired = viewModel.LockExpRequired = assignment.AssignedPartner.NeedExp;
            else
                viewModel.ExpRequired = viewModel.LockExpRequired = true;
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register (DeliveryRegistrationViewModel viewModel)
        {
            PartnerAssignment assignment = await _context.PartnerAssignment.Include(p => p.Partner).Include(p => p.AssignedPartner).FirstOrDefaultAsync(p => p.PartnerId == viewModel.IdS && p.AssignedPartnerId == viewModel.IdR && (viewModel.DeliveryType == DeliveryType.CrossDock ? p.CrossDock : p.Expeditor));
            if (assignment == null)
            {
                viewModel.Error = "Заказчик не назначен поставщику по этому типу поставки";
                return View(viewModel);
            }
            if (ModelState.IsValid)
            {
                Registry registry = _mapper.Map<Registry>(viewModel);
                registry.SenderId = viewModel.IdS;
                registry.ReceiverId = viewModel.IdR;
                registry.ReceiveDate = DateTime.Today;
                registry.ReceiveTime = DateTime.Now;
                if(viewModel.ExpRequired || assignment.AssignedPartner.NeedExp || viewModel.DeliveryType == DeliveryType.Expeditor)
                {
                    registry.ExpID = DateTime.Now.ToString("HHmmssddMMyy");
                    registry.ExpDate = DateTime.Today;
                }
                Tariff tariff = await _context.Tariff.FirstOrDefaultAsync(t => t.PartnerId == registry.ReceiverId);
                if (tariff != null)
                {
                    DeliveryTerminalHelper.CalculateRate(registry, tariff);
                }
                _context.Add(registry);
                await _context.SaveChangesAsync(User?.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction("PrintDocuments", "DeliveryRegistration", new { SenderID = registry.SenderId, ReceiverID = registry.ReceiverId, RegistryID = registry.Id, Weights = viewModel.Weights});
            }
            viewModel.NameS = assignment.Partner.Name;
            viewModel.NameR = assignment.AssignedPartner.Name;
            if (viewModel.DeliveryType == DeliveryType.CrossDock)
                viewModel.ExpRequired = viewModel.LockExpRequired = assignment.AssignedPartner.NeedExp;
            else
                viewModel.ExpRequired = viewModel.LockExpRequired = true;
            ViewData["RegionId"] = new SelectList(_context.Region, "Id", "Name");
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> CodeParse(long assignmentID)
        {
            PartnerAssignment assignment = await _context.PartnerAssignment.FirstOrDefaultAsync(p => p.Id == assignmentID && (p.CrossDock || p.Expeditor));
            if(assignment == null)
            {
                ViewData["Error"] = "Связь не найдена";
                return View();
            }
            if (await NeedToWarn(assignment.PartnerId))
            {
                return RedirectToAction("CollectReturns", new { id = assignment.PartnerId });
            }
            if (assignment.CrossDock && assignment.Expeditor)
            {
                ViewData["Sender"] = assignment.PartnerId;
                ViewData["Receiver"] = assignment.AssignedPartnerId;
                return View();
            }
            else if (assignment.CrossDock)
            {
                return RedirectToAction("Register", new { Sender = assignment.PartnerId, Receiver = assignment.AssignedPartnerId, Type = DeliveryType.CrossDock });
            }
            else
            {
                return RedirectToAction("Register", new { Sender = assignment.PartnerId, Receiver = assignment.AssignedPartnerId, Type = DeliveryType.Expeditor });
            }
        }
        [HttpGet]
        public async Task<IActionResult> PrintDocuments(long senderID, long receiverID, long registryID, List<int>? weights)
        {
            Registry registry = await _context.Registry.Include(p => p.Sender).Include(p => p.Receiver).Include(p => p.Region).FirstOrDefaultAsync(p => p.Id == registryID && p.SenderId == senderID && p.ReceiverId == receiverID);
            if (registry == null)
            {
                return RedirectToAction("Index");
            }
            PrintDocumentsViewModel viewModel = new();
            viewModel.Registry = registry;
            viewModel.Entries = new List<PrintDocumentsPrintEntry>();
            int wcount = 0;
            if(weights == null || weights.Count<1)
            {
                wcount = -1;
            }
            List<string> types = new List<string>();
            if(registry.CountPallets > 0)
            {
                for(int i = 0; i< registry.CountPallets; i++)
                {
                    viewModel.Entries.Add(new PrintDocumentsPrintEntry() { Name = "Паллета", Weight = wcount == -1 ? null : weights[wcount++], Text = (i+1).ToString() + " из " + registry.CountPallets});
                }
                types.Add("Паллеты");
            }
            if (registry.CountBoxes > 0)
            {
                for (int i = 0; i < registry.CountBoxes; i++)
                {
                    viewModel.Entries.Add(new PrintDocumentsPrintEntry() { Name = "Коробка", Weight = wcount == -1 ? null : weights[wcount++], Text = (i + 1).ToString() + " из " + registry.CountBoxes });
                }
                types.Add("Коробки");
            }
            if (registry.CountOversized > 0)
            {
                for (int i = 0; i < registry.CountOversized; i++)
                {
                    viewModel.Entries.Add(new PrintDocumentsPrintEntry() { Name = "Негабарит", Weight = wcount == -1 ? null : weights[wcount++], Text = (i + 1).ToString() + " из " + registry.CountOversized });
                }
                types.Add("Негабарит");
            }
            viewModel.PackageName = String.Join(", ", types.ToArray());
            viewModel.PackageSlots = 0;
            if (registry.CountPallets > 0)
                viewModel.PackageSlots += (int)registry.CountPallets;
            if (registry.CountBoxes > 0)
                viewModel.PackageSlots += (int)registry.CountBoxes;
            if (registry.CountOversized > 0)
                viewModel.PackageSlots += (int)registry.CountOversized;
            if (registry.Receiver.TransportingCompany != null)
            {
                viewModel.TransportingCompany = registry.Receiver.TransportingCompany;
            }
            else
            {
                viewModel.TransportingCompany = await _context.TransportingCompany.FirstOrDefaultAsync(t => t.DefaultTC);
            }
            return View(viewModel);
        }
        private async Task<bool> NeedToWarn(long id)
        {
            if (await _context.Return.AnyAsync(r => r.Supplier != null && r.Supplier.Id == id && r.ReceiveDate != null && r.ReturnDate == null))
            {
                var condition = await _context.SupplierReturn.FirstOrDefaultAsync(s => s.SupplierId == id);
                if (condition == null || (DateTime.Now - condition.LastWarning).Hours > 6)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
