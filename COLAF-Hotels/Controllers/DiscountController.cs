using COLAFHotel.Data;
using COLAFHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace COLAFHotel.Controllers
{
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Discount/Offers - Public facing discount offers page
        public IActionResult Offers()
        {
            var activeDiscounts = _context.Discounts
                .Where(d => d.status == "Active" && d.expiration_date > DateTime.UtcNow)
                .ToList();

            return View(activeDiscounts);
        }

        // GET: Discount/ManageOffers - Staff management page for discounts
        public IActionResult ManageOffers()
        {
            var allDiscounts = _context.Discounts
                .OrderByDescending(d => d.status == "Active")
                .ThenBy(d => d.expiration_date)
                .ToList();

            return View(allDiscounts);
        }
        public IActionResult Edit()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }

        // POST: Discount/CreateDiscount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscount(Discount discount)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    // Ensure promo code is uppercase for consistency
                    discount.promo_code = discount.promo_code.ToUpper();
                    discount.expiration_date = DateTime.SpecifyKind(discount.expiration_date, DateTimeKind.Utc);

                    _context.Add(discount);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Discount '{discount.name}' was created successfully.";
                    return RedirectToAction(nameof(ManageOffers));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to create discount. " + ex.Message);
                }
            }

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return RedirectToAction(nameof(ManageOffers));
        }

        // POST: Discount/EditDiscount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDiscount(Discount discount)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    discount.promo_code = discount.promo_code.ToUpper();
                    discount.expiration_date = DateTime.SpecifyKind(discount.expiration_date, DateTimeKind.Utc);
                    _context.Update(discount);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Discount '{discount.name}' was updated successfully.";
                    return RedirectToAction(nameof(ManageOffers));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(ManageOffers));
        }


        // POST: Discount/DeleteDiscount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDiscount(string discount_id)
        {
            var discount = await _context.Discounts.FindAsync(discount_id);

            if (discount == null)
            {
                TempData["ErrorMessage"] = "Discount not found.";
                return RedirectToAction(nameof(ManageOffers));
            }

            try
            {
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Discount '{discount.name}' was deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction(nameof(ManageOffers));
        }

        // GET: Discount/ActivateDiscount/5
        public async Task<IActionResult> ActivateDiscount(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out int discountId))
            {
                TempData["ErrorMessage"] = "Invalid discount ID.";
                return RedirectToAction(nameof(ManageOffers));
            }

            var discount = await _context.Discounts.FindAsync(discountId);
            if (discount == null)
            {
                TempData["ErrorMessage"] = "Discount not found.";
                return RedirectToAction(nameof(ManageOffers));
            }

            discount.status = "Active";
            discount.expiration_date = DateTime.SpecifyKind(discount.expiration_date, DateTimeKind.Utc);
            _context.Update(discount);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Discount '{discount.name}' has been activated.";
            return RedirectToAction(nameof(ManageOffers));
        }

        // GET: Discount/DeactivateDiscount/5
        public async Task<IActionResult> DeactivateDiscount(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out int discountId))
            {
                TempData["ErrorMessage"] = "Invalid discount ID.";
                return RedirectToAction(nameof(ManageOffers));
            }

            var discount = await _context.Discounts.FindAsync(discountId);
            if (discount == null)
            {
                TempData["ErrorMessage"] = "Discount not found.";
                return RedirectToAction(nameof(ManageOffers));
            }

            discount.status = "Inactive";
            discount.expiration_date = DateTime.SpecifyKind(discount.expiration_date, DateTimeKind.Utc);
            _context.Update(discount);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Discount '{discount.name}' has been deactivated.";
            return RedirectToAction(nameof(ManageOffers));
        }

        [HttpPost]
        public IActionResult ApplyPromo(string promoCode, decimal discountValue, string discountID)
        {
            HttpContext.Session.SetString("activeDiscount", "true");

            // Convert the decimal to string for session storage
            HttpContext.Session.SetString("discountValue", discountValue.ToString());

            Console.WriteLine($"Discount Value: {discountValue}");
            HttpContext.Session.SetString("promoCode", promoCode);

            HttpContext.Session.SetString("discountID", discountID);

            TempData["discountMessageSuccess"] = $"{promoCode} promo has been activated! Enjoy your discount.";

            return RedirectToAction("List", "Room");
        }





        // Helper method to check if discount exists
        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.discount_id == id);
        }
    }
}