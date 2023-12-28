using Azure;
using HotelApp.Core;
using HotelApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HotelApp.Core.Handlers
{
    public class BookingHandler : IHandler
    {
        public static void Create(HotelContext db)
        {
            Create(db, null);
        }
        public static void Delete(HotelContext db)
        {
            Delete(db, true);
        }
        public static void Create(HotelContext db, Guest? existingGuest)
        {
            var booking = new Booking();
            var invoice = new Invoice();
            var guest = new Guest();
            var recommendedRooms = new List<Room>();
            var availableRooms = new List<Room>();
            var startDate = new DateTime();
            var endDate = new DateTime();
            int input = 0, guests;
            string? dateInput = string.Empty;
            bool firstCheck = false;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Creating a new booking\n ");
            Console.WriteLine("0. Back");
            if (existingGuest == null)
            {
                if (db.Guest.ToList().Count == 0) return;
                Console.WriteLine("If you'd like to skip the prompt, just input a minus (-)\n");
                Console.WriteLine("Guest Id. Name");
                db.Guest.ToList().ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}"));
                Console.Write("Write the guest id for the booking: ");
                while (!int.TryParse(Console.ReadLine(), out input) || !db.Guest.Any(g => g.Id == input))
                {
                    if (input == 0) return;
                    Console.WriteLine("Please enter the correct option.");
                }
                booking.GuestId = input;
                guest = db.Guest.First(g => g.Id == input);
            }
            else
            {
                booking.GuestId = existingGuest.Id;
                guest = existingGuest;
            }
            Console.Write("How many will be staying at the hotel?(1-4) ");
            while (!int.TryParse(Console.ReadLine(), out guests) || !Enumerable.Range(0, 5).Contains(guests))
            {
                if (input == 0) return;
                Console.WriteLine("You may only have up to 4 guests. (1-4)");
            }
            while (RoomHandler.GetAvailableRooms(db, startDate, endDate).Count == 0)
            {
                if (firstCheck)
                {
                    Console.WriteLine($"There was no available rooms between {startDate.ToShortDateString()} - {endDate.ToShortDateString()}, please try another range of dates.");
                }
                // start date
                Console.Write("Write the start date of the booking:(write 'today' for today's date, otherwise YYYY-MM-DD) ");
                dateInput = Console.ReadLine().ToLower();
                while (true)
                {
                    if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput.Equals("0"))
                    {
                        if (dateInput.Equals("0")) return;
                        Console.WriteLine("Don't type empty entries");
                    }
                    else
                    {
                        if (dateInput.Equals("today"))
                        {
                            startDate = DateTime.Today;
                            break;
                        }
                        if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                        {
                            Console.WriteLine("Please use the correct format yyyy-mm-dd");
                        }
                        else
                        {
                            if (DateTime.Parse(dateInput) >= DateTime.Today)
                            {
                                startDate = DateTime.Parse(dateInput);
                                break;
                            }
                            else Console.WriteLine("Make sure the start date isn't a date before than today");
                        }
                    }
                    dateInput = Console.ReadLine().ToLower();
                }
                // end date
                Console.Write("Write the end date of the booking:(YYYY-MM-DD) ");
                dateInput = Console.ReadLine().ToLower();
                while (true)
                {
                    if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput.Equals("0"))
                    {
                        if (dateInput.Equals("0")) return;
                        Console.WriteLine("Don't type empty entries");
                    }
                    else
                    {
                        if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                        {
                            Console.WriteLine("Please use the correct format yyyy-mm-dd");
                        }
                        else
                        {
                            if (DateTime.Parse(dateInput) > DateTime.Today)
                            {
                                endDate = DateTime.Parse(dateInput).AddDays(1).AddTicks(-1);
                                firstCheck = true;
                                break;
                            }
                            else Console.WriteLine("Make sure the end date isn't a date earlier than today");
                        }
                    }
                    dateInput = Console.ReadLine().ToLower();
                }
                recommendedRooms = RoomHandler.GetRecommendedRooms(db, startDate, endDate, guests);
                availableRooms = RoomHandler.GetAvailableRooms(db, startDate, endDate);
            }
            var activeBookings = db.Booking.Where(b => b.GuestId == guest.Id && b.IsActive).ToList();
            if (activeBookings.Count > 0)
            {
                activeBookings.ForEach(b =>
                {
                    if ((startDate >= b.StartDate && startDate <= b.EndDate)
                    || (endDate >= b.StartDate && endDate <= b.EndDate)
                    || (startDate <= b.StartDate && endDate >= b.EndDate))
                    {
                        Console.WriteLine("\nYou can't have another booking during an active one.");
                        Console.WriteLine("If you'd like to try another date, enter 'yes' or 'y'");
                        string? tryAnotherDate = Console.ReadLine().ToLower();
                        if (tryAnotherDate.Equals("yes") || tryAnotherDate.Equals("y")) Create(db, guest);
                        else return;
                    }
                });
            }
            booking.StartDate = startDate;
            booking.EndDate = endDate;
            if (startDate == DateTime.Today) booking.IsActive = true;
            Console.Clear();
            Console.WriteLine("To find availability of other rooms with less/more guests and/or dates, enter -10");
            Console.WriteLine("To exit, enter 0");
            Console.WriteLine("\nRoom ID. Size, Beds, Extra Beds, Price");
            //availableRooms.ForEach(r => Console.WriteLine($"{r.Id}. {r.Size}m^2, {r.Beds}, {r.ExtraBeds}, {r.Price}kr"));
            if (recommendedRooms.Count > 0)
            {
                Console.WriteLine("\nRecommended room(s) based on how many will be staying:");
                recommendedRooms.ForEach(r => Console.WriteLine($"{r.Id}. {r.Size}m^2, {r.Beds}, {r.ExtraBeds}, {r.Price}kr"));
            }
            Console.WriteLine("\nIf you'd like to see all available rooms at this date, enter 'yes' or 'y'");
            string? seeAll = Console.ReadLine().ToLower();
            if (seeAll.Equals("yes") || seeAll.Equals("y")) availableRooms.ForEach(r => Console.WriteLine($"{r.Id}. {r.Size}m^2, {r.Beds}, {r.ExtraBeds}, {r.Price}kr"));
            Console.Write("\nWrite the room id of the booking: ");
            while (!int.TryParse(Console.ReadLine(), out input) || !availableRooms.Any(r => r.Id == input))
            {
                if (input == 0) return;
                if (input == -10) Create(db, guest);
                Console.WriteLine("Please enter the correct option.");
            }
            booking.RoomId = input;
            db.Booking.Add(booking);
            invoice.BookingId = booking.Id;
            invoice.GuestId = booking.GuestId;
            invoice.DueDate = booking.StartDate.AddDays(10);
            invoice.IsPaid = false;
            invoice.IsArchived = false;
            invoice.TotalSum = (int)Math.Ceiling((booking.EndDate - booking.StartDate).TotalDays)
                * db.Room.First(r => r.Id == booking.RoomId).Price;
            //db.Invoice.Add(invoice);
            guest.Invoices.Add(invoice);
            guest.IsActive = true;
            db.SaveChanges();
        }

        public static void Delete(HotelContext db, bool showAll)
        {
            List<Booking> allBookings = new();
            if (showAll) allBookings = db.Booking.ToList();
            else allBookings = db.Booking.Where(b => !b.IsArchived).ToList();
            if (allBookings.Count == 0) return;
            int bookingIndex;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Cancelling a booking\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("ID - Guest - Room ID - Start Date - End Date");
            allBookings.ForEach(b => Console.WriteLine($"{b.Id} - {db.Guest.First(g => g.Id == b.GuestId).Name} - " +
                $"{b.RoomId} - {b.StartDate.ToShortDateString()} - {b.EndDate.ToShortDateString()}"));
            Console.Write("Which booking would you like to cancel? ");
            while (!int.TryParse(Console.ReadLine(), out bookingIndex) || !allBookings.Any(b => b.Id == bookingIndex))
            {
                if (bookingIndex == 0) return;
                Console.WriteLine($"Please enter an option");
            }
            Booking booking = db.Booking.First(b => b.Id == bookingIndex);
            Invoice invoice = db.Invoice.First(i => i.BookingId == booking.Id);
            Console.WriteLine("\nKeep in mind this will delete invoices related to the booking.\n");
            Console.Write($"Are you sure you want to cancel Booking {booking.Id} as a booking?(y/n/h - hard delete) ");
            string? confirmInput = Console.ReadLine().ToLower();
            while (string.IsNullOrWhiteSpace(confirmInput) || string.IsNullOrEmpty(confirmInput)
                || !(confirmInput.Equals("y") || confirmInput.Equals("n") || confirmInput.Equals("h")))
            {
                Console.WriteLine("Please follow the instructions.");
                confirmInput = Console.ReadLine().ToLower();
            }
            if (confirmInput.Equals("y"))
            {
                invoice.IsArchived = true;
                booking.IsActive = false;
                booking.IsArchived = true;
            }
            else if (confirmInput.Equals("h"))
            {
                db.Invoice.Remove(invoice);
                db.Booking.Remove(booking);
            }
            db.SaveChanges();
        }

        public static void Update(HotelContext db)
        {
            var allBookings = db.Booking.ToList();
            if (allBookings.Count == 0) return;
            int bookingIndex, input = 0;
            string? dateInput = string.Empty;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a booking\n ");
            Console.WriteLine("0. Back");
            allBookings.Where(b => b.EndDate > DateTime.Today && !b.IsArchived).ToList().ForEach(b => Console.WriteLine($"{b.Id} - " +
                $"{db.Guest.First(g => g.Id == b.GuestId).Name} - {b.RoomId} - {b.StartDate.ToShortDateString()} - {b.EndDate.ToShortDateString()}"));
            Console.Write("Which booking would you like to edit? ");
            while (!int.TryParse(Console.ReadLine(), out bookingIndex) || !allBookings.Any(b => b.Id == bookingIndex))
            {
                if (bookingIndex == 0) return;
                Console.WriteLine("Please enter an option");
            }
            Booking booking = db.Booking.First(b => b.Id == bookingIndex);
            Invoice invoice = db.Invoice.First(i => i.BookingId == booking.Id);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting booking {booking.Id}\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Guest Id");
            Console.WriteLine("2. Room Id");
            Console.WriteLine("3. Start date");
            Console.WriteLine("4. End date");
            Console.Write("Which part would you like to edit? ");
            while (!int.TryParse(Console.ReadLine(), out bookingIndex) || !Enumerable.Range(0, 5).Contains(bookingIndex))
            {
                if (bookingIndex == 0) return;
                Console.WriteLine("Please enter an option (0-4)");
            }
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Booking {booking.Id} " +
                $"- from {booking.StartDate.ToShortDateString()} to {booking.EndDate.ToShortDateString()}\n");
            switch (bookingIndex)
            {
                case 1:

                    db.Guest.Where(g => g.Id != booking.GuestId).ToList().ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}"));
                    Console.Write("What would you like to change the guest ID to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || !db.Guest.Any(g => g.Id == input))
                    {
                        if (input == 0) return;
                        Console.WriteLine("Please enter an option");
                    }
                    booking.GuestId = input;
                    break;
                case 2:
                    var availableRooms = RoomHandler.GetAvailableRooms(db, booking.StartDate, booking.EndDate);
                    if (availableRooms.Count == 0)
                    {
                        Console.WriteLine("There are no current rooms available under the bookings dates, press any button to continue.");
                        Console.ReadKey();
                        break;
                    }
                    Console.Write("What would you like to change the room ID to? ");
                    Console.WriteLine("ID - Size - Beds - Price");
                    availableRooms.ForEach(r => Console.WriteLine($"{r.Id} - {r.Size}m^2 - {r.Beds} - {r.Price}kr"));
                    while (!int.TryParse(Console.ReadLine(), out input) || !availableRooms.Any(r => r.Id == input))
                    {
                        if (input == 0) return;
                        Console.WriteLine("Please enter an option");
                    }
                    // If the room change happens after the booking started, make sure to include the price of the first room of the booking.
                    // This probably doesn't work if you change it more than once though..
                    if (DateTime.Today > booking.StartDate)
                    {
                        var dateAfterBookingStart = DateTime.Today - booking.StartDate;
                        var sumFromFirstRoom = db.Room.First(r => r.Id == booking.RoomId).Price * Math.Ceiling(dateAfterBookingStart.TotalDays);
                        invoice.TotalSum = ((int)Math.Ceiling((booking.EndDate - DateTime.Today).TotalDays)
                        * db.Room.First(r => r.Id == input).Price) + (int)sumFromFirstRoom;
                    }
                    else
                    {
                        invoice.TotalSum = (int)Math.Ceiling((booking.EndDate - booking.StartDate).TotalDays)
                            * db.Room.First(r => r.Id == input).Price;
                    }
                    booking.RoomId = input;
                    break;
                case 3:
                    if (booking.IsActive)
                    {
                        Console.WriteLine("You cannot change the start date of an already active booking, press any button to continune.");
                        Console.ReadKey();
                        return;
                    }
                    // Maybe bad way of doing this but since we aren't going to have many bookings it's fine.
                    Console.WriteLine("List of bookings, their ids and their dates with same room:");
                    db.Booking.Where(b => b.GuestId != booking.GuestId).ToList().ForEach(r => Console.WriteLine($"{r.Id} " +
                        $"- Start: {r.StartDate.ToShortDateString()} - End: {r.EndDate.ToShortDateString()}"));
                    Console.Write("\nWrite the new start date of the booking:(write 'today' for today's date, otherwise YYYY-MM-DD) ");
                    dateInput = Console.ReadLine().ToLower();
                    while (true)
                    {
                        if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput.Equals("0"))
                        {
                            if (dateInput.Equals("0")) return;
                            Console.WriteLine("Don't type empty entries");
                        }
                        else
                        {
                            if (dateInput.Equals("today") && DateTime.Today < booking.StartDate.Date && DateTime.Today < booking.EndDate.Date)
                            {
                                if (RoomHandler.CheckSpecificRoomAvailability(db, booking.RoomId, DateTime.Today, booking.EndDate, booking.Id))
                                {
                                    booking.StartDate = DateTime.Now;
                                    booking.IsActive = true;
                                    break;
                                }
                                else Console.WriteLine("Make sure the start date doesn't conflict with other bookings of the same room or isn't same as end date.");
                            }
                            if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                            {
                                if (!dateInput.Equals("today"))
                                    Console.WriteLine("Please use the correct format yyyy-mm-dd");
                            }
                            else
                            {
                                if (DateTime.Parse(dateInput) > DateTime.Today && DateTime.Parse(dateInput) < booking.EndDate.Date)
                                {
                                    if (RoomHandler.CheckSpecificRoomAvailability(db, booking.RoomId, DateTime.Parse(dateInput), booking.EndDate, booking.Id))
                                    {
                                        booking.StartDate = DateTime.Parse(dateInput);
                                        break;
                                    }
                                    else Console.WriteLine("Make sure the start date doesn't conflict with other bookings of the same room.");
                                }
                                else Console.WriteLine("Make sure the start date isn't a date before than today or isn't same as end date");
                            }
                        }
                        dateInput = Console.ReadLine().ToLower();
                    }
                    break;
                case 4:
                    if (booking.EndDate < DateTime.Today)
                    {
                        Console.WriteLine("Why would you change the end date of a booking that has already expired? Press any button to continue");
                        Console.ReadKey();
                        return;
                    }
                    // Maybe bad way of doing this but since we aren't going to have many bookings it's fine.
                    Console.WriteLine("List of bookings, their ids and their dates with same room:");
                    db.Booking.Where(b => b.GuestId != booking.GuestId).ToList().ForEach(r => Console.WriteLine($"{r.Id} " +
                        $"- Start: {r.StartDate.ToShortDateString()} - End: {r.EndDate.ToShortDateString()}"));
                    Console.Write("\nWrite the new end date of the booking:(YYYY-MM-DD) ");
                    dateInput = Console.ReadLine().ToLower();
                    while (true)
                    {
                        if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput.Equals("0"))
                        {
                            if (dateInput.Equals("0")) return;
                            Console.WriteLine("Don't type empty entries");
                        }
                        else
                        {
                            if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                            {
                                Console.WriteLine("Please use the correct format yyyy-mm-dd");
                            }
                            else
                            {
                                if (DateTime.Parse(dateInput) > DateTime.Today && booking.StartDate.Date < DateTime.Parse(dateInput))
                                {
                                    if (RoomHandler.CheckSpecificRoomAvailability(db, booking.RoomId, booking.StartDate, DateTime.Parse(dateInput), booking.Id))
                                    {
                                        booking.EndDate = DateTime.Parse(dateInput).AddDays(1).AddTicks(-1);
                                        invoice.TotalSum = (int)Math.Ceiling((booking.EndDate - booking.StartDate).TotalDays)
                                            * db.Room.First(r => r.Id == input).Price;
                                        break;
                                    }
                                    else Console.WriteLine("Make sure the end date doesn't conflict with other bookings of the same room.");
                                }
                                else Console.WriteLine("Make sure the end date isn't a date earlier than today");
                            }
                        }
                        dateInput = Console.ReadLine().ToLower();
                    }
                    break;
            }
            db.SaveChanges();
        }
        public static void ShowAll(HotelContext db)
        {
            var allBookings = db.Booking.ToList();
            if (allBookings.Count == 0) return;
            int input;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all bookings\n ");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Show all bookings");
            Console.WriteLine("2. Only show archived bookings");
            Console.WriteLine("3. Only show unarchived bookings");
            while (!int.TryParse(Console.ReadLine(), out input) || !Enumerable.Range(0, 4).Contains(input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter an option (0-3)");
            }
            Console.Clear();
            switch (input)
            {
                case 1:
                    Console.Clear();
                    Console.WriteLine("Id - Guest - Room ID - Start Date - End Date - Active - Archived");
                    allBookings.ForEach(b => Console.WriteLine($"{b.Id}. {db.Guest.First(g => g.Id == b.GuestId).Name} " +
                        $"- {b.RoomId} - {b.StartDate.Date.ToShortDateString()} - {b.EndDate.ToShortDateString()} " +
                        $"- active: {b.IsActive.ToString().ToLower()} - archived: {b.IsArchived.ToString().ToLower()}"));
                    break;
                case 2:
                    Console.WriteLine("Id - Guest - Room ID - Start Date - End Date");
                    allBookings.Where(b => b.IsArchived)
                        .ToList()
                        .ForEach(b => Console.WriteLine($"{b.Id}. {db.Guest.First(g => g.Id == b.GuestId).Name} " +
                        $"- {b.RoomId} - {b.StartDate.Date.ToShortDateString()} - {b.EndDate.ToShortDateString()}"));
                    break;
                case 3:
                    Console.WriteLine("Id - Guest - Room ID - Start Date - End Date");
                    allBookings.Where(b => !b.IsArchived)
                        .ToList()
                        .ForEach(b => Console.WriteLine($"{b.Id}. {db.Guest.First(g => g.Id == b.GuestId).Name} " +
                        $"- {b.RoomId} - {b.StartDate.Date.ToShortDateString()} - {b.EndDate.ToShortDateString()}"));
                    break;
                case 0:
                    return;
            }
            Console.WriteLine("\nPress any button to continue.");
            Console.ReadKey();
        }
    }
}