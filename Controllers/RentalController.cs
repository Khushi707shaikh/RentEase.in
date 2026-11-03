
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentEase.DTOs;
using RentEase.Interfaces;
using RentEase.Models;
using System.Security.Claims;

namespace RentEase.Controllers
{
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        // Tenant Dashboard
        [Authorize(Roles = "Tenant")]
        [HttpGet]
        public async Task<IActionResult> TenantDashboard()
        {
            int tenantUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var dashboard = await _rentalService
                .GetTenantDashboardAsync(tenantUserId);

            if (dashboard == null)
                return NotFound("Tenant details not found.");

            return View(dashboard);
        }
        // GET: Assign Room
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> AssignRoom()
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var tenants = await _rentalService
                .GetTenantsByLandlordAsync(landlordId);

            var rooms = await _rentalService
                .GetAvailableRoomsByLandlordAsync(landlordId);

            ViewBag.Tenants = new SelectList(
                tenants, "TenantId", "FullName");

            ViewBag.Rooms = new SelectList(
                rooms, "RoomId", "RoomNumber");

            return View();
        }

        // POST: Assign Room
        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoom(AssignRoomDTO model)
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            if (!ModelState.IsValid)
            {
                await LoadAssignmentDropdowns(landlordId);
                return View(model);
            }

            bool result = await _rentalService
                .AssignRoomAsync(model, landlordId);

            if (!result)
            {
                ModelState.AddModelError("",
                    "Room assignment failed. Check tenant, room availability and dates.");

                await LoadAssignmentDropdowns(landlordId);
                return View(model);
            }

            TempData["Success"] = "Tenant assigned to room successfully!";

            return RedirectToAction("AssignRoom");
        }

        private async Task LoadAssignmentDropdowns(int landlordId)
        {
            var tenants = await _rentalService
                .GetTenantsByLandlordAsync(landlordId);

            var rooms = await _rentalService
                .GetAvailableRoomsByLandlordAsync(landlordId);

            ViewBag.Tenants = new SelectList(
                tenants, "TenantId", "FullName");

            ViewBag.Rooms = new SelectList(
                rooms, "RoomId", "RoomNumber");
        }
        // GET: Tenant List
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> TenantList()
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var tenants = await _rentalService
                .GetTenantListByLandlordAsync(landlordId);

            return View(tenants);
        }
        // GET: Record Rent Payment
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> RecordPayment()
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var agreements = await _rentalService
                .GetAgreementsByLandlordAsync(landlordId);

            ViewBag.Agreements = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                agreements, "AgreementId", "AgreementId");

            return View();
        }
        // POST: Record Rent Payment
        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(RecordRentPaymentDTO model)
        {
            if (!ModelState.IsValid)
            {
                await LoadAgreementDropdown();
                return View(model);
            }

            var payment = new RentPayment
            {
                AgreementId = model.AgreementId,
                Amount = model.Amount,
                PaymentDate = model.PaymentDate,
                PaymentMethod = model.PaymentMethod,
                TransactionReference = model.TransactionReference,
                Remarks = model.Remarks,
                PaymentStatus = "Paid"
            };

            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            bool result = await _rentalService
                .AddRentPaymentAsync(payment, landlordId);

            if (!result)
            {
                ModelState.AddModelError("",
                    "Payment could not be recorded. Please check the agreement.");

                await LoadAgreementDropdown();
                return View(model);
            }

            TempData["Success"] = "Rent payment recorded successfully!";

            return RedirectToAction("RecordPayment");
        }

        private async Task LoadAgreementDropdown()
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var agreements = await _rentalService
                .GetAgreementsByLandlordAsync(landlordId);

            ViewBag.Agreements = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                agreements, "AgreementId", "AgreementId");
        }
        // GET: Tenant Payment History
        [Authorize(Roles = "Tenant")]
        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            int tenantUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var payments = await _rentalService
                .GetPaymentsForTenantAsync(tenantUserId);

            return View(payments);
        }
        // GET: Add Utility Bill
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> AddUtilityBill()
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var bills = await _rentalService
                .GetAgreementsByLandlordAsync(landlordId);

            ViewBag.Agreements = new SelectList(
                bills, "AgreementId", "AgreementId"
            );

            return View();
        }

        // POST: Add Utility Bill
        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUtilityBill(UtilityBillDTO model)
        {
            if (model.DueDate.Date < model.BillingDate.Date)
            {
                ModelState.AddModelError(
                    "DueDate",
                    "Due date cannot be earlier than billing date."
                );
            }

            if (!ModelState.IsValid)
            {
                await LoadUtilityAgreements();
                return View(model);
            }

            var bill = new UtilityBill
            {
                AgreementId = model.AgreementId,
                BillType = model.BillType,
                Amount = model.Amount,
                BillingDate = model.BillingDate,
                DueDate = model.DueDate,
                PaymentStatus = "Pending"
            };

            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            bool result = await _rentalService
                .AddUtilityBillAsync(bill, landlordId);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to add utility bill. Please check the agreement."
                );

                await LoadUtilityAgreements();
                return View(model);
            }

            TempData["Success"] = "Utility bill added successfully!";

            return RedirectToAction("AddUtilityBill");
        }

        private async Task LoadUtilityAgreements()
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var agreements = await _rentalService
                .GetAgreementsByLandlordAsync(landlordId);

            ViewBag.Agreements = new SelectList(
                agreements, "AgreementId", "AgreementId"
            );
        }
        // GET: Tenant Utility Bills
        [Authorize(Roles = "Tenant")]
        [HttpGet]
        public async Task<IActionResult> MyUtilityBills()
        {
            int tenantUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var bills = await _rentalService
                .GetUtilityBillsForTenantAsync(tenantUserId);

            return View(bills);
        }
    }
}
