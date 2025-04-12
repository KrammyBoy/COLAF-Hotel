using COLAFHotel.Data;
using COLAFHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace COLAFHotel.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("Role"); // Get the role of the logged-in user
            var guest_id = HttpContext.Session.GetString("GuestId");

            List<Booking> bookings;

            if (userRole == "Staff" || userRole == "Admin")
            {
                // Staff/Admin can see all bookings
                bookings = _context.Bookings
                    .Include(b => b.Guest)
                    .ThenInclude(g => g.User)
                    .Include(b => b.Room)
                    .OrderBy(b => b.check_in_date)
                    .ToList();
            }
            else if (int.TryParse(guest_id, out int guestId))
            {
                // Guests only see their own bookings
                bookings = _context.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .Where(b => b.guest_id == guestId)
                    .OrderBy(b => b.check_in_date)
                    .ToList();
            }
            else
            {
                // No valid guest ID, return empty list
                bookings = new List<Booking>();
            }

            return View(bookings);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Guest)
                    .ThenInclude(g => g.User)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.booking_id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
        public async Task<IActionResult> Invoice(int id)
        {
            Console.WriteLine($"Booking ID: {id}");

            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Guest)
                        .ThenInclude(g => g.User)
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Room)
                .Include(i => i.Booking)
                    .ThenInclude(b => b.BookingServices)
                        .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(i => i.booking_id == id);

            if (invoice?.Booking?.Guest?.User != null)
            {
                Console.WriteLine($"{invoice.Booking.Guest.User.firstname} {invoice.Booking.Guest.User.lastname}");
                Console.WriteLine($"{invoice.Booking.BookingServices}");
            }
            else
            {
                Console.WriteLine("Invoice, booking, guest, or user not found.");
            }

            if (invoice == null)
            {
                Console.WriteLine($"No invoice found for booking_id: {id}");
                ViewBag.ErrorMessage = "Invoice not found.";
                return View();
            }

            Console.WriteLine($"Invoice found: {invoice.invoice_id}");
            return View(invoice);
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Guest)
                        .ThenInclude(g => g.User)
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Room)
                .Include(i => i.Booking)
                    .ThenInclude(b => b.BookingServices)
                        .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(i => i.booking_id == id);

            if (invoice == null)
            {
                Console.WriteLine("Invoice not found");
                return NotFound();
            }

            var pdfDocument = CreateInvoicePdf(invoice);

            using (var stream = new MemoryStream())
            {
                pdfDocument.GeneratePdf(stream);
                stream.Position = 0; // Reset the stream position before returning
                return File(stream.ToArray(), "application/pdf", $"Invoice_{invoice.invoice_id}.pdf");
            }
        }

        private Document CreateInvoicePdf(Invoice invoice)
        {
            // Define the coffee theme colors
            var coffeeDark = "#2C1E1A";
            var coffeeMedium = "#5E4B3B";
            var coffeeLight = "#9B7E6B";
            var coffeeCream = "#E6DBCA";
            var coffeeAccent = "#D4A067";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(coffeeMedium));

                    page.Content().Column(col =>
                    {
                        // Header with hotel details
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(innerCol =>
                            {
                                innerCol.Item().Text("COLAF HOTEL").FontSize(24).Bold().FontColor(coffeeDark);
                                innerCol.Item().Text("An unforgettable stay with a coffee-inspired twist").FontSize(12).Italic().FontColor(coffeeLight);
                                innerCol.Item().Text("R.530 St., UCM Boulevard").FontSize(10).FontColor(coffeeMedium);
                                innerCol.Item().Text("reservations@colafhotel.com | +1 (800) 420-1337").FontSize(10).FontColor(coffeeMedium);
                            });

                            row.RelativeItem(1).AlignRight().Text(text =>
                            {
                                text.Span("INVOICE").FontSize(24).Bold().FontColor(coffeeAccent);
                            });
                        });

                        // Divider
                        col.Item().BorderBottom(1).BorderColor(coffeeCream).PaddingBottom(10);

                        // Invoice details
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(innerCol =>
                            {
                                innerCol.Item().Text("INVOICE DETAILS").Bold().FontColor(coffeeDark).FontSize(12);
                                innerCol.Item().Grid(grid =>
                                {
                                    grid.Columns(2);
                                    grid.Item().Text("Invoice No:").FontColor(coffeeLight);
                                    grid.Item().Text($"{invoice.invoice_id}").Bold();
                                    grid.Item().Text("Booking Ref:").FontColor(coffeeLight);
                                    grid.Item().Text($"{invoice.booking_id}").Bold();
                                    grid.Item().Text("Issue Date:").FontColor(coffeeLight);
                                    grid.Item().Text($"{invoice.issue_date:MMM dd, yyyy}").Bold();
                                    grid.Item().Text("Due Date:").FontColor(coffeeLight);
                                    grid.Item().Text($"{invoice.issue_date.AddDays(14):MMM dd, yyyy}").Bold();
                                });
                            });

                            row.RelativeItem().Column(innerCol =>
                            {
                                innerCol.Item().Text("GUEST INFORMATION").Bold().FontColor(coffeeDark).FontSize(12);
                                innerCol.Item().Text($"Name: {invoice.Booking.Guest.User.firstname} {invoice.Booking.Guest.User.lastname}").FontColor(coffeeMedium);
                                innerCol.Item().Text($"Phone: {invoice.Booking.Guest.phone}").FontColor(coffeeMedium);
                                innerCol.Item().Text($"Email: {invoice.Booking.Guest.User.email}").FontColor(coffeeMedium);
                            });
                        });

                        // Stay details
                        col.Item().PaddingTop(20).Column(innerCol =>
                        {
                            innerCol.Item().Text("STAY DETAILS").Bold().FontColor(coffeeDark).FontSize(12);
                            innerCol.Item().Grid(grid =>
                            {
                                grid.Columns(2);
                                grid.Item().Text("Check-in Date:").FontColor(coffeeLight);
                                grid.Item().Text($"{invoice.Booking.check_in_date:dddd, MMM dd, yyyy} (from 3:00 PM)").Bold();
                                grid.Item().Text("Check-out Date:").FontColor(coffeeLight);
                                grid.Item().Text($"{invoice.Booking.check_out_date:dddd, MMM dd, yyyy} (by 11:00 AM)").Bold();
                                grid.Item().Text("Length of Stay:").FontColor(coffeeLight);
                                grid.Item().Text($"{(invoice.Booking.check_out_date - invoice.Booking.check_in_date).TotalDays} Nights").Bold();
                                grid.Item().Text("Room:").FontColor(coffeeLight);
                                grid.Item().Text($"{invoice.Booking.Room.RoomNumber} - {invoice.Booking.Room.Category}").Bold();
                            });
                        });

                        // Charges table
                        col.Item().PaddingTop(20).Column(innerCol =>
                        {
                            innerCol.Item().Text("CHARGES").Bold().FontColor(coffeeDark).FontSize(12);

                            // Table header
                            innerCol.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Header row
                                table.Header(header =>
                                {
                                    header.Cell().Background(coffeeDark).Padding(5).Text("Description").FontColor(Colors.White).Bold();
                                    header.Cell().Background(coffeeDark).Padding(5).Text("Quantity").FontColor(Colors.White).Bold().AlignRight();
                                    header.Cell().Background(coffeeDark).Padding(5).Text("Rate").FontColor(Colors.White).Bold().AlignRight();
                                    header.Cell().Background(coffeeDark).Padding(5).Text("Amount").FontColor(Colors.White).Bold().AlignRight();
                                });

                                // Room charge
                                int nights = (int)(invoice.Booking.check_out_date - invoice.Booking.check_in_date).TotalDays;
                                decimal roomRate = invoice.Booking.Room.Price;
                                decimal roomTotal = roomRate * nights;

                                table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5)
                                    .Text($"Room Charge ({invoice.Booking.Room.RoomNumber} - {invoice.Booking.Room.Category})");
                                table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text($"{nights}").AlignRight();
                                table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text($"${roomRate:F2}").AlignRight();
                                table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text($"${roomTotal:F2}").AlignRight();

                                // Services
                                decimal servicesTotal = 0;
                                if (invoice.Booking.BookingServices != null && invoice.Booking.BookingServices.Any())
                                {
                                    foreach (var bookingService in invoice.Booking.BookingServices)
                                    {
                                        if (bookingService?.Service == null)
                                        {
                                            Console.WriteLine("Missing service in bookingService");
                                            continue;
                                        }
                                        decimal servicePrice = bookingService.Service.price;
                                        servicesTotal += servicePrice;

                                        table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text(bookingService.Service.name);
                                        table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text("1").AlignRight();
                                        table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text($"${servicePrice:F2}").AlignRight();
                                        table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text($"${servicePrice:F2}").AlignRight();
                                    }
                                }

                                // Discount (if any)
                                decimal discountAmount = 0;
                                if (invoice.Booking.discount_id.HasValue)
                                {
                                    // Add discount logic here if you have discount information
                                    table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text("Discount").FontColor(Colors.Green.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text("1").AlignRight().FontColor(Colors.Green.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text("-").AlignRight().FontColor(Colors.Green.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(coffeeCream).Padding(5).Text($"-${discountAmount:F2}").AlignRight().FontColor(Colors.Green.Medium);
                                }
                            });
                        });

                        // Summary
                        col.Item().PaddingTop(10).Column(innerCol =>
                        {
                            innerCol.Item().AlignRight().Row(row =>
                            {
                                row.RelativeItem(4);
                                row.RelativeItem(6).Grid(grid =>
                                {
                                    grid.Columns(2);
                                    grid.Item().Text("Total:").FontColor(coffeeDark).Bold().AlignRight();
                                    grid.Item().Text($"${invoice.Booking.total_amount:F2}").FontSize(14).Bold().FontColor(coffeeAccent).AlignRight();

                                    grid.Item().Text("Amount Paid:").FontColor(coffeeLight).AlignRight();
                                    grid.Item().Text($"${(invoice.Booking.total_amount - invoice.Booking.totalBalance):F2}").Bold().AlignRight();

                                    grid.Item().Text("Balance Due:").FontColor(coffeeDark).Bold().AlignRight();
                                    grid.Item().Text($"${invoice.Booking.totalBalance:F2}").FontSize(14).Bold().FontColor(coffeeDark).AlignRight();
                                });
                            });
                        });

                        // Payment Information
                        col.Item().PaddingTop(20).Column(innerCol =>
                        {
                            innerCol.Item().Text("PAYMENT INFORMATION").Bold().FontColor(coffeeDark).FontSize(12);
                            innerCol.Item().Text("Please include your invoice number with your payment.").FontColor(coffeeMedium);
                            innerCol.Item().Grid(grid =>
                            {
                                grid.Columns(2);
                                grid.Item().Text("Bank Transfer:").FontColor(coffeeLight);
                                grid.Item().Text("COLAF Bank • Acc: 1234-5678-9012 • Sort: 12-34-56").Bold();
                                grid.Item().Text("Payment Due:").FontColor(coffeeLight);
                                grid.Item().Text($"{invoice.issue_date.AddDays(14):MMM dd, yyyy}").Bold();
                            });
                        });

                        // Footer
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(innerCol =>
                            {
                                innerCol.Item().BorderTop(1).BorderColor(coffeeCream).PaddingTop(5);
                                innerCol.Item().Text(text =>
                                {
                                    text.Span("Thank you for choosing ").FontColor(coffeeMedium).FontSize(10);
                                    text.Span("COLAF Hotel").Bold().FontColor(coffeeDark).FontSize(10);
                                });
                                innerCol.Item().Text("We look forward to welcoming you back soon!").FontColor(coffeeMedium).FontSize(10);
                            });
                        });
                    });
                });
            });
        }

        // POST: Booking/UpdateBookingDates/5
        [HttpPost]
        public async Task<IActionResult> UpdateBookingDates(int id, DateTime check_in_date, DateTime check_out_date)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.booking_id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Ensure the dates are valid and logical
            if (check_in_date >= check_out_date)
            {
                ModelState.AddModelError("", "Check-out date must be later than check-in date.");
                return RedirectToAction(nameof(Details), new { id = booking.booking_id });
            }

            // Convert the dates to UTC before saving to the database
            booking.check_in_date = check_in_date.ToUniversalTime();
            booking.check_out_date = check_out_date.ToUniversalTime();

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = booking.booking_id });
        }

        public IActionResult Create(int roomId, string roomNumber, string roomImg, string roomCategory, decimal roomPrice)
        {
            var room = new Room
            {
                RoomId = roomId,
                RoomNumber = roomNumber,
                Category = roomCategory,
                ImageUrl = roomImg,
                Price = roomPrice
            };
            
            return View(room);
        }
        public IActionResult AddBookingWalkIn()
        {
            var rooms = _context.Room
                .Where(r => r.Status == "Vacant")  // Filter only vacant rooms
                .OrderBy(r => r.RoomNumber)        // Order by room number
                .ToList();

            return View(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> StaffAddBooking
            (
            string guestId,
            string userId,
            string SelectedRoom, // RoomID
            string roomNumber,
            string category,
            string imageUrl,
            string Price, DateTime CheckInDate, DateTime CheckOutDate, decimal totalPrice, string cardGuestName, string cashGuestName, decimal totalBalance)
        {
            Console.WriteLine($"Staff Booking Confirmed: GuestName={cardGuestName} or {cashGuestName}, RoomId={SelectedRoom}, CheckIn={CheckInDate}, CheckOut={CheckOutDate}, TotalPrice={totalPrice}, TotalBalance={totalBalance}");

            if (guestId == "null")
            {
                // Add Guest for the user
                var newGuest = new Guest
                {
                    user_id = Convert.ToInt32(userId)
                };
                _context.Guests.Add(newGuest);
                await _context.SaveChangesAsync();
                guestId = newGuest.guest_id.ToString();
                Console.WriteLine($"Guest ID: {guestId}");
            }

            var guestName = (cardGuestName != null) ? cardGuestName : cashGuestName;
            var room = new Room
            {
                RoomId = Convert.ToInt32(SelectedRoom),
                RoomNumber = roomNumber,
                Category = category,
                ImageUrl = imageUrl,
                Price = Convert.ToDecimal(Price)
            };

            CheckInDate = DateTime.SpecifyKind(CheckInDate, DateTimeKind.Utc);
            CheckOutDate = DateTime.SpecifyKind(CheckOutDate, DateTimeKind.Utc);

            // Validate Check-in and Check-out Dates
            if (CheckInDate < DateTime.UtcNow)
            {
                TempData["Error"] = "Check-in date cannot be in the past.";
                return View("AddBookingWalkIn", room);
            }

            if (CheckOutDate <= CheckInDate)
            {
                TempData["Error"] = "Check-out date must be after check-in date.";
                return View("AddBookingWalkIn", room);
            }

            var booking = new Booking
            {
                guestName = guestName,
                room_id = Convert.ToInt32(SelectedRoom),
                check_in_date = CheckInDate,
                check_out_date = CheckOutDate,
                status = "Confirmed", // Options: Confirmed, Pending, Cancelled
                total_amount = totalPrice,
                totalBalance = totalBalance
            };

            var guest = await _context.Guests.Include(g => g.Bookings)
                                             .FirstOrDefaultAsync(g => g.guest_id == Convert.ToInt32(guestId));

            if (guest != null)
            {
                guest.Bookings.Add(booking);  // Add the booking to the guest's booking history
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();


            TempData["Success"] = "Your booking has been confirmed";
            TempData["BookingId"] = booking.booking_id;
            TempData["GuestName"] = guestName;
            TempData["CheckInDate"] = booking.check_in_date.ToString("MMM d, yyyy");
            TempData["CheckOutDate"] = booking.check_out_date.ToString("MMM d, yyyy");
            TempData["TotalAmount"] = booking.total_amount.ToString();
            TempData["CurrentDate"] = DateTime.Now.ToString("MMM d, yyyy");
            TempData["RoomNumber"] = roomNumber;
            DateTime checkInDate = booking.check_in_date;
            DateTime checkOutDate = booking.check_out_date;

            int numberOfNights = (checkOutDate - checkInDate).Days;
            TempData["NumberOfNights"] = numberOfNights;

            // Prevent division by zero
            decimal roomRate = numberOfNights > 0 ? totalPrice / numberOfNights : 0;
            TempData["RoomRate"] = roomRate.ToString();



            return RedirectToAction("Index", "Booking");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(string GuestId, string UserId, string RoomId, string RoomNumber, string Category, string ImageUrl, string Price, DateTime CheckInDate, DateTime CheckOutDate, decimal totalPrice, decimal totalBalance, string haveTransport)
        {
            Console.WriteLine($"Booking Confirmed: GuestId={GuestId}, UserId={UserId}, RoomId={RoomId}, CheckIn={CheckInDate}, CheckOut={CheckOutDate}, TotalPrice={totalPrice}, TotalBalance={totalBalance}, Transport={haveTransport}");

            if (GuestId == "null")
            {
                // Add Guest for the user
                var newGuest = new Guest
                {
                    user_id = Convert.ToInt32(UserId)
                };
                _context.Guests.Add(newGuest);
                await _context.SaveChangesAsync();
                GuestId = newGuest.guest_id.ToString();
                Console.WriteLine($"Guest ID: {GuestId}");
            }

            CheckInDate = DateTime.SpecifyKind(CheckInDate, DateTimeKind.Utc);
            CheckOutDate = DateTime.SpecifyKind(CheckOutDate, DateTimeKind.Utc);
            // Create room object (you don't need to persist the room object, as it already exists in the DB)
            var room = new Room
            {
                RoomId = Convert.ToInt32(RoomId),
                RoomNumber = RoomNumber,
                Category = Category,
                ImageUrl = ImageUrl,
                Price = Convert.ToDecimal(Price)
            };

            // Validate Check-in and Check-out Dates
            if (CheckInDate < DateTime.UtcNow)
            {
                TempData["Error"] = "Check-in date cannot be in the past.";
                return View("Create", room);
            }

            if (CheckOutDate <= CheckInDate)
            {
                TempData["Error"] = "Check-out date must be after check-in date.";
                return View("Create", room);
            }

            var booking = new Booking
            {
                guest_id = Convert.ToInt32(GuestId),
                room_id = Convert.ToInt32(RoomId),
                check_in_date = CheckInDate,
                check_out_date = CheckOutDate,
                status = "Confirmed", // Options: Confirmed, Pending, Cancelled
                total_amount = totalPrice,
                totalBalance = totalBalance
            };

            // Find the guest and add the new booking to the guest's bookings list
            var guest = await _context.Guests.Include(g => g.Bookings)
                                             .FirstOrDefaultAsync(g => g.guest_id == Convert.ToInt32(GuestId));

            if (guest != null)
            {
                guest.Bookings.Add(booking);  // Add the booking to the guest's booking history
            }

            // Save both the booking and the updated guest to the database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Check if transport service is selected
            if (haveTransport == "true")
            {
                // Add transport booking service
                await AddTransportBookingService(booking.booking_id, "Transport Service");
            }
            else
            {
                // No transport service selected
                Console.WriteLine("No transport service selected.");
            }

            // Add invoice for the booking
            await AddInvoice(booking.booking_id, DateTime.UtcNow);

            TempData["Success"] = "Your booking has been confirmed";
            TempData["BookingId"] = booking.booking_id;
            TempData["CheckInDate"] = booking.check_in_date.ToString("MMM d, yyyy");
            TempData["CheckOutDate"] = booking.check_out_date.ToString("MMM d, yyyy");
            TempData["TotalAmount"] = booking.total_amount.ToString();
            TempData["CurrentDate"] = DateTime.Now.ToString("MMM d, yyyy");
            TempData["RoomNumber"] = RoomNumber;
            DateTime checkInDate = booking.check_in_date;
            DateTime checkOutDate = booking.check_out_date;

            int numberOfNights = (checkOutDate - checkInDate).Days;
            TempData["NumberOfNights"] = numberOfNights;

            // Prevent division by zero
            decimal roomRate = numberOfNights > 0 ? totalPrice / numberOfNights : 0;
            TempData["RoomRate"] = roomRate.ToString();


            return RedirectToAction("Index", "Booking");
        }

        public async Task AddTransportBookingService(int booking_id, string transportService)
        {
            int service_id = 0; // Default value

            switch (transportService)
            {
                case "Transport Service":
                    service_id = 1;
                    break;
                default:
                    service_id = 0;
                    break;
            }

            var Booking_Service = new Booking_Service
            {
                booking_id = booking_id,
                service_id = service_id
            };
            _context.Booking_Services.Add(Booking_Service);
            await _context.SaveChangesAsync();


        }
        public async Task AddInvoice(int bookingId, DateTime issue_date)
        {
            var invoice = new Invoice
            {
                booking_id = bookingId,
                issue_date = issue_date,
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }


        [HttpPost]
        public async Task<IActionResult> SaveForLater(string GuestId, string UserId, string RoomId, string RoomNumber, string Category, string ImageUrl, string Price, DateTime CheckInDate, DateTime CheckOutDate, decimal totalPrice)
        {
            Console.WriteLine($"Booking Confirmed: GuestId={GuestId}, UserId={UserId}, RoomId={RoomId}, CheckIn={CheckInDate}, CheckOut={CheckOutDate}, TotalPrice={totalPrice}");

            if (GuestId == "null")
            {
                // Add Guest for the user
                var newGuest = new Guest
                {
                    user_id = Convert.ToInt32(UserId)
                };
                _context.Guests.Add(newGuest);
                await _context.SaveChangesAsync();
                GuestId = newGuest.guest_id.ToString();
                Console.WriteLine($"Guest ID: {GuestId}");
            }

            CheckInDate = DateTime.SpecifyKind(CheckInDate, DateTimeKind.Utc);
            CheckOutDate = DateTime.SpecifyKind(CheckOutDate, DateTimeKind.Utc);

            // Create room object (you don't need to persist the room object, as it already exists in the DB)
            var room = new Room
            {
                RoomId = Convert.ToInt32(RoomId),
                RoomNumber = RoomNumber,
                Category = Category,
                ImageUrl = ImageUrl,
                Price = Convert.ToDecimal(Price)
            };

            // Validate Check-in and Check-out Dates
            if (CheckInDate < DateTime.UtcNow)
            {
                TempData["Error"] = "Check-in date cannot be in the past.";
                return View("Create", room);
            }

            if (CheckOutDate <= CheckInDate)
            {
                TempData["Error"] = "Check-out date must be after check-in date.";
                return View("Create", room);
            }

            var booking = new Booking
            {
                guest_id = Convert.ToInt32(GuestId),
                room_id = Convert.ToInt32(RoomId),
                check_in_date = CheckInDate,
                check_out_date = CheckOutDate,
                status = "Pending", // Options: Confirmed, Pending, Cancelled
                total_amount = totalPrice
            };

            // Find the guest and add the new booking to the guest's bookings list
            var guest = await _context.Guests.Include(g => g.Bookings)
                                             .FirstOrDefaultAsync(g => g.guest_id == Convert.ToInt32(GuestId));

            if (guest != null)
            {
                guest.Bookings.Add(booking);  // Add the booking to the guest's booking history
            }

            // Save both the booking and the updated guest to the database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your booking has been confirmed";
            TempData["BookingId"] = booking.booking_id;
            TempData["CheckInDate"] = booking.check_in_date.ToString("yyyy-MM-dd");
            TempData["CheckOutDate"] = booking.check_out_date.ToString("yyyy-MM-dd");
            TempData["TotalAmount"] = booking.total_amount.ToString();

            return RedirectToAction("Index", "Booking");
        }
        [HttpGet("GetUnavailableDates")]
        public IActionResult GetUnavailableDates(int roomId)
        {
            var unavailableDates = _context.Bookings
                .Where(b => b.room_id == roomId && b.status == "Confirmed")
                .AsEnumerable() // Move to memory processing
                .SelectMany(b => Enumerable.Range(0, (b.check_out_date - b.check_in_date).Days)
                    .Select(d => b.check_in_date.AddDays(d).ToString("yyyy-MM-dd")))
                .ToList();

            return Ok(unavailableDates);
        }


    }
}
