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
            return;
        }

        public static void Create(HotelContext db, Guest? existingGuest)
        {
            var booking = new Booking();
            var invoice = new Invoice();
            var guest = new Guest();
            var startDate = new DateTime();
            var endDate = new DateTime();
            int input = -1;
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
            while (GetAvailableRooms(db, startDate, endDate).Count == 0)
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
                    if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput == "0")
                    {
                        if (dateInput == "0") return;
                        Console.WriteLine("Don't type empty entries");
                    }
                    else
                    {
                        if (dateInput == "today")
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
                    dateInput = Console.ReadLine();
                }
                // end date
                Console.Write("Write the end date of the booking:(YYYY-MM-DD) ");
                dateInput = Console.ReadLine();
                while (true)
                {
                    if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput == "0")
                    {
                        if (dateInput == "0") return;
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
                    dateInput = Console.ReadLine();
                }
            }
            booking.StartDate = startDate;
            booking.EndDate = endDate;
            if (startDate == DateTime.Today) booking.IsActive = true;
            Console.WriteLine("Available rooms: - Room ID. Size, Beds, Price");
            GetAvailableRooms(db, startDate, endDate).ForEach(r => Console.WriteLine($"{r.Id}. {r.Size}, {r.Beds}, {r.Price}"));
            Console.Write("Write the room id of the booking: ");
            while (!int.TryParse(Console.ReadLine(), out input) || !db.Room.Any(r => r.Id == input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter the correct option.");
            }
            booking.RoomId = input;
            db.Booking.Add(booking);
            invoice.BookingId = booking.Id;
            invoice.GuestId = booking.GuestId;
            invoice.DueDate = booking.StartDate.AddDays(10);
            invoice.IsPaid = false;
            invoice.TotalSum = (int)Math.Ceiling((booking.EndDate - booking.StartDate).TotalDays)
                * db.Room.First(r => r.Id == booking.RoomId).Price;
            //db.Invoice.Add(invoice);
            guest.Invoices.Add(invoice);
            guest.IsActive = true;
            db.SaveChanges();
        }

        public static void Delete(HotelContext db)
        {
            var allBookings = db.Booking.ToList();
            if (allBookings.Count == 0) return;
            string? confirmInput = string.Empty;
            int bookingIndex = -1;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a booking\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("ID - Guest - Room ID - Start Date - End Date");
            allBookings.ForEach(b => Console.WriteLine($"{b.Id} - {db.Guest.First(g => g.Id == b.GuestId).Name} - " +
                $"{b.RoomId} - {b.StartDate.ToShortDateString()} - {b.EndDate.ToShortDateString()}"));
            Console.Write("Which booking would you like to delete? ");
            while (!int.TryParse(Console.ReadLine(), out bookingIndex) || !allBookings.Any(b => b.Id == bookingIndex))
            {
                if (bookingIndex == 0) return;
                Console.WriteLine($"Please enter an option");
            }
            Booking booking = db.Booking.First(b => b.Id == bookingIndex);
            Console.WriteLine("\nKeep in mind this will delete invoices related to the booking.\n");
            Console.Write($"Are you sure you want to delete Booking {booking.Id} as a booking?(y/n/h - hard delete) ");
            confirmInput = Console.ReadLine().ToLower();
            while (string.IsNullOrWhiteSpace(confirmInput) || string.IsNullOrEmpty(confirmInput)
                || !(confirmInput == "y" || confirmInput == "n" || confirmInput == "h"))
            {
                Console.WriteLine("Please follow the instructions.");
                confirmInput = Console.ReadLine();
            }
            if (confirmInput == "y")
            {
                db.Invoice.First(i => i.BookingId == booking.Id);
                booking.IsActive = false;
                db.SaveChanges();
            }
            else if (confirmInput == "h")
            {
                db.Booking.Remove(booking);
                db.SaveChanges();
            }
        }

        public static void Update(HotelContext db)
        {
            var allBookings = db.Booking.ToList();
            if (allBookings.Count == 0) return;
            int bookingIndex = -1, input = -1;
            string? dateInput = string.Empty;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a booking\n ");
            Console.WriteLine("0. Back");
            allBookings.Where(b => b.EndDate > DateTime.Today).ToList().ForEach(b => Console.WriteLine($"{b.Id} - {db.Guest.First(g => g.Id == b.GuestId).Name} - " +
                $"{b.RoomId} - {b.StartDate.ToShortDateString()} - {b.EndDate.ToShortDateString()}"));
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
                    var availableRooms = GetAvailableRooms(db, booking.StartDate, booking.EndDate);
                    if (availableRooms.Count == 0)
                    {
                        Console.WriteLine("There are no current rooms available under the bookings dates, press any button to continue.");
                        Console.ReadKey();
                        break;
                    }
                    Console.Write("What would you like to change the room ID to? ");
                    Console.WriteLine("ID - Size - Beds - Price");
                    availableRooms.ForEach(r => Console.WriteLine($"{r.Id} - {r.Size} - {r.Beds} - {r.Price}"));
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
                        if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput == "0")
                        {
                            if (dateInput == "0") return;
                            Console.WriteLine("Don't type empty entries");
                        }
                        else
                        {
                            if (dateInput == "today" && DateTime.Today < booking.StartDate.Date && DateTime.Today < booking.EndDate.Date)
                            {
                                if (CheckSpecificRoomAvailability(db, booking.RoomId, DateTime.Today, booking.EndDate, booking.Id))
                                {
                                    booking.StartDate = DateTime.Now;
                                    booking.IsActive = true;
                                    break;
                                }
                                else Console.WriteLine("Make sure the start date doesn't conflict with other bookings of the same room or isn't same as end date.");
                            }
                            if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                            {
                                if (dateInput != "today")
                                    Console.WriteLine("Please use the correct format yyyy-mm-dd");
                            }
                            else
                            {
                                if (DateTime.Parse(dateInput) > DateTime.Today && DateTime.Parse(dateInput) < booking.EndDate.Date)
                                {
                                    if (CheckSpecificRoomAvailability(db, booking.RoomId, DateTime.Parse(dateInput), booking.EndDate, booking.Id))
                                    {
                                        booking.StartDate = DateTime.Parse(dateInput);
                                        break;
                                    }
                                    else Console.WriteLine("Make sure the start date doesn't conflict with other bookings of the same room.");
                                }
                                else Console.WriteLine("Make sure the start date isn't a date before than today or isn't same as end date");
                            }
                        }
                        dateInput = Console.ReadLine();
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
                    dateInput = Console.ReadLine();
                    while (true)
                    {
                        if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput == "0")
                        {
                            if (dateInput == "0") return;
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
                                    if (CheckSpecificRoomAvailability(db, booking.RoomId, booking.StartDate, DateTime.Parse(dateInput), booking.Id))
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
                        dateInput = Console.ReadLine();
                    }
                    break;
            }
            db.SaveChanges();
        }
        public static void ShowAll(HotelContext db)
        {
            var allBookings = db.Booking.ToList();
            if (allBookings.Count == 0) return;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all bookings\n ");
            Console.WriteLine("Id - Guest - Room ID - Start Date - End Date");
            allBookings.ForEach(b => Console.WriteLine($"{b.Id}. {db.Guest.First(g => g.Id == b.GuestId).Name} " +
                $"- {b.RoomId} - {b.StartDate.Date.ToShortDateString()} - {b.EndDate.ToShortDateString()} - active: {b.IsActive.ToString().ToLower()}"));
            Console.WriteLine("\nPress any button to continue.");
            Console.ReadKey();
        }
        /// <summary>
        /// Checks if the room related to the <paramref name="roomId"/> is available between <paramref name="startDate"/> and <paramref name="endDate"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="roomId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool CheckSpecificRoomAvailability(HotelContext db, int roomId, DateTime startDate, DateTime endDate)
        {
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue) return false; // Return false in case there is no given startDate or endDate
            return !db.Booking.Where(b => b.RoomId == roomId).Any(b =>
            (startDate >= b.StartDate && startDate <= b.EndDate)
            || (endDate >= b.StartDate && endDate <= b.EndDate)
            || (startDate <= b.StartDate && endDate >= b.EndDate));
        }
        public static bool CheckSpecificRoomAvailability(HotelContext db, int roomId, DateTime startDate, DateTime endDate, int bookingToExlude)
        {
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue) return false; // Return false in case there is no given startDate or endDate
            return !db.Booking.Where(b => b.RoomId == roomId && b.Id != bookingToExlude)
                .Any(b => (startDate >= b.StartDate && startDate <= b.EndDate)
            || (endDate >= b.StartDate && endDate <= b.EndDate)
            || (startDate <= b.StartDate && endDate >= b.EndDate));
        }
        public static List<Room> GetAvailableRooms(HotelContext db, DateTime startDate, DateTime endDate)
        {
            var rooms = db.Room.ToList();
            return rooms.Where(r => CheckSpecificRoomAvailability(db, r.Id, startDate, endDate)).ToList();
        }
    }
}